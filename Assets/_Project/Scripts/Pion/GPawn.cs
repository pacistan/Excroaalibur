using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector.Editor;

public class GPawn : GGridObject
{
    public event Action<GEquipment> OnEquip;
    public event Action<GEquipment> OnUnequip;
    public event Action OnKill;
    public event Action OnStunned;
    public event Action OnUnstunned;
    public event Action OnHealthChanged;
    public event Action OnAnimationPush;
    public event Action OnAnimationThrow;
        
    public const string MoveAnimationName = "Move";
    public const string IdleAnimationName = "Idle";
    public const string PushAnimationName = "Push";
    public const string ThrowAnimationName = "Throw";
    public const string PushedStartAnimationName = "Pushed_Start";
    public const string PushedEndAnimationName = "Pushed_End";

    [SerializeReference]
    public GPawnData data;

    [field: SerializeField]
    public bool hasCrown { get; private set; } = false;

    [SerializeField, ReadOnly, BoxGroup("Important Info")]
    public GEquipment equipment;



    [FoldoutGroup("Other", false)]
    [SerializeField, FoldoutGroup("Other/Events"), HideIf("@hp < 0")]
    private UnityEvent _OnDeath;

    [FoldoutGroup("Other", false)]
    [field : SerializeField, FoldoutGroup("Other/Components")]
    public GPawnVisualsController visuals { get; private set; }

    [FoldoutGroup("Other", false)]
    [field : SerializeField, FoldoutGroup("Other/Components")]
    public Transform equipmentParentTr { get; private set; }



    [SerializeField, ReadOnly, HideInEditorMode]
    public int remainingActionToken;
    
    [SerializeField, ReadOnly, HideInEditorMode, FormerlySerializedAs("_stunTurn")] 
    public int stunTurn = 0;
    
    [field: SerializeField, HideIf("@hp == -1"), HideInEditorMode]
    public int hp { get; protected set; } = 3;

    public bool IsStunned => stunTurn > 0;
    // Cache for quick look-up of override reactions
    private Dictionary<Type, GAction> _overrideCache;
    
    public GAction GetReaction(GAction action)
    {
        if (action == null) return null;
      
        var t = action.GetType();
        
        if (_overrideCache != null)
        {
            // Exact type 
            if (_overrideCache.TryGetValue(t, out var overExact) && overExact != null)
                return overExact.CloneAction();

            // Parent type
            Type bt = t.BaseType;
            while (bt != null && typeof(GAction).IsAssignableFrom(bt))
            {
                if (_overrideCache.TryGetValue(bt, out GAction overBase) && overBase != null)
                    return overBase.CloneAction();
                bt = bt.BaseType;
            }
        }
        
        // Fallback to base reaction data
        if (data.baseReactionData && data.baseReactionData.HasReaction(action))
            return data.baseReactionData.GetReaction(action);

        return null;
    }
    
    public void GiveEquipement(GEquipment _equipment, bool updateTransform, bool invokeEvent)
    {
        equipment = _equipment;
        equipment.owner = this;
        if (invokeEvent) OnEquip?.Invoke(equipment);

        if (updateTransform)
        {
            equipment.transform.parent = equipmentParentTr;
            equipment.transform.localPosition = Vector3.zero; 
            equipment.transform.localRotation = Quaternion.identity;
            if (equipment is GCrown)
            {
                RuntimeManager.PlayOneShotAttached("event:/Crown/Grab", gameObject);
            }
        }

    }
    
    public void ReleaseEquipement(bool giveToCell, bool resetCrownPassCount = false)
    {
        if (equipment == null) return;
        equipment.owner = null;
        OnUnequip?.Invoke(equipment);

        if (resetCrownPassCount && equipment && equipment is GCrown)
        {
            GCrown crown = (GCrown)equipment;
            crown.ResetCrown();
        }
        
        // TODO : This used to give parenting to the cell. Not anymore, might cause bugs !!!
        if (giveToCell) 
            GetCell().SetGridObject(equipment, false);
        
        equipment = null;
    }
    
    public bool RequestAction(GAction action)
    {
        if (action == null) return false;
        action.linkedPawn = this;
        print("Request " + action.ToString());
        if (!GTurnBaseManager.Instance.TryPlayAction(action))
        {
            print("Action Failed");
            return false;
        }
        
        return true;
    }

    public bool IsAlive => !(hp == 0);
   
    
    public void TakeDamage(int damage = 1)
    {
        if (hp <= 0 || data.isPlayer) return;
        
        hp = Mathf.Max(0, hp - damage);
        OnHealthChanged?.Invoke();
        
        if (hp == 0 && GetCell().gridObject == this)
        {
            GetCell().SetGridObject(null);
        } 
    }
    
    public void Stun(int stunTurnNumber)
    {
        bool justGotStunned = stunTurn == 0;
        stunTurn += stunTurnNumber;
        if(justGotStunned)
            OnStunned?.Invoke();
    }

    public void Fall()
    {
        if (data.isPlayer)
        {
            Stun(1);
        }
        else
        {
            TakeDamage(hp); // instant kill
        }
        
        RuntimeManager.PlayOneShotAttached("event:/Pawn/Fall", gameObject);
    }

    public void Kill()
    {
        OnKill?.Invoke();
        _OnDeath?.Invoke();
        isMarkedForDestruction = true;
        Destroy(gameObject);
    }

    public void OnStartAction()
    {
        
    }
    
    public void OnStartTurn()
    {
        remainingActionToken = data.actionTokens;
        if (stunTurn > 0 && !data.isPlayer)
        {
            stunTurn--;
            visuals.OnUpdateStunTurn();
        }
        else if (data.isPlayer)
        {
            visuals.OnUpdateActionsToken();
        }
        
        if (stunTurn == 0)
        {
            OnUnstunned?.Invoke();
        }
    }

    public void UpdateStunTurn()
    {
        visuals.OnUpdateStunTurn();
    }

    public void UpdateHpNumber()
    {
        visuals.OnUpdateHealthPoints();
    }
    
    public void OnEndTurn()
    {
        if (stunTurn > 0 && data.isPlayer)
        {
            stunTurn--;
            visuals.OnUpdateStunTurn();
        }
    }
    
    
    public override bool TrySetCell(GCell newCell)
    {
        if (!base.TrySetCell(newCell)) return false;
        if (newCell.GetTileType == ETileType.Hole) 
            Fall();

        return true;
    }
    
    private void RebuildOverrideCache()
    {
        _overrideCache = new Dictionary<Type, GAction>();

        if (data == null || data.overrideReactionByType == null) return;

        foreach (var kv in data.overrideReactionByType)
        {
            var key = kv.Key;   // SerializableType<GAction>
            var val = kv.Value; // GAction
            var type = key != null ? key.Type : null;

            if (type == null || val == null) continue;

            // La dernière entrée gagnante écrase l’ancienne (pratique si doublons)
            _overrideCache[type] = val;
        }
    #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
    #endif
    }

    protected virtual void Awake()
    {
        if (data.isPlayer) hp = -1; // Player has infinite HP
        RebuildOverrideCache();
        data = ScriptableObject.Instantiate(data);
    }

    protected virtual void Start()
    {
        _currentCell.SetGridObject(this);
        remainingActionToken = data.actionTokens;
        hp = data.startHp;
        if (!data.isPlayer && TryGetComponent(out GController aiController))
        {
            aiController.RegisterPawn(this);
        }
        else if(data.isPlayer)
        {
            GPlayerController controller = FindFirstObjectByType<GPlayerController>();
            visuals.OnUpdateActionsToken();
            if (controller)
            {
                foreach (var action in data.actions)
                {
                    action.linkedPawn = this;
                    controller.RegisterPawn(this);
                    action.OnActionFinished += controller.OnActionOver;
                }
            }
        }
    }

    public void OnPushEvent()
    {
        OnAnimationPush?.Invoke();
    }

    public void OnThrowEvent()
    {
        OnAnimationThrow?.Invoke();
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            visuals.UpdatePawnVisuals(); 
        }
        
        RebuildOverrideCache();
    }
#endif
    
}