using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

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
    public const string IdlenimationName = "Idle";
    public const string PushAnimationName = "Push";
    public const string ThrowAnimationName = "Throw";
    public const string PushedStartAnimationName = "Pushed_Start";
    public const string PushedEndAnimationName = "Pushed_End";
    
    [SerializeField, FoldoutGroup("Events"), HideIf("@hp < 0")]
    private UnityEvent _OnDeath;
    
    [field : SerializeField, Min(1)]
    public int actionTokens { get; set; }

    [SerializeField, ReadOnly]
    public int remainingActionToken;
    
    [field: SerializeField]
    public bool hasCrown { get; private set; } = false;
    
    [SerializeField]
    public bool isPlayer;
    
    [SerializeReference, ShowIf("isPlayer")]
    public List<GAction> actions = new List<GAction>();
    
    [SerializeField, FormerlySerializedAs("BaseReactionData")]
    public GReactionData baseReactionData;
    
    // Cache in _overrideCache at Awake and OnValidate
    [OdinSerialize, DictionaryDrawerSettings(KeyLabel = "Action Type", ValueLabel = "Reaction"), Tooltip("Dictionary mapping action types to reaction actions that override both base reactions and default reactions.")] 
    public Dictionary<SerializableType<GAction>, GAction> overrideReactionByType = new();
    
    [SerializeField, ReadOnly, FoldoutGroup("Components")]
    public GEquipment equipment;

    [field : SerializeField, FoldoutGroup("Components")]
    public GPawnVisualsController visuals { get; private set; }
    
    [SerializeField, ReadOnly, HideInEditorMode, FormerlySerializedAs("_stunTurn")] 
    public int stunTurn = 0;
    
    public bool IsStunned => stunTurn > 0;

    [field: SerializeField, HideIf("@hp == -1")]
    public int hp { get; protected set; } = 3;
    
    public int startHp { get; protected set; } 

    [field : SerializeField, FoldoutGroup("Components")]
    public Transform equipmentParentTr { get; private set; }

    public EventReference hoverSound;
    public EventReference SelectSound;
    
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
        if (baseReactionData && baseReactionData.HasReaction(action))
            return baseReactionData.GetReaction(action);

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
            crown.visuals.OnUpdateDebugTextContent(crown._currentDamage);
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
        if (hp <= 0 || isPlayer) return;
        
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
        if (isPlayer)
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
        remainingActionToken = actionTokens;
        if (stunTurn > 0 && !isPlayer)
        {
            stunTurn--;
            visuals.OnUpdateStunTurn();
        }
        else if (isPlayer)
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
        if (stunTurn > 0 && isPlayer)
        {
            stunTurn--;
            visuals.OnUpdateStunTurn();
        }
    }
    
    public override void SetCell(GHexCoordinate newCoordinate)
    {
        base.SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public override void SetCell(GCell newCell)
    {
        base.SetCell(newCell);
        if (newCell.GetTileType == ETileType.Hole)
        {
            Fall();
        }
        // TODO : This used to give parenting to the cell. Not anymore, might cause bugs !!!
        GetCell().SetGridObject(this, false);
    }
    
    private void RebuildOverrideCache()
    {
        _overrideCache = new Dictionary<Type, GAction>();

        if (overrideReactionByType == null) return;

        foreach (var kv in overrideReactionByType)
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
        if (isPlayer) hp = -1; // Player has infinite HP
        _currentCell.SetGridObject(this);
        RebuildOverrideCache();
    }

    protected virtual void Start()
    {
        remainingActionToken = actionTokens;
        startHp = hp;
        if (!isPlayer && TryGetComponent(out GController aiController))
        {
            aiController.RegisterPawn(this);
        }
        else if(isPlayer)
        {
            GPlayerController controller = FindFirstObjectByType<GPlayerController>();
            visuals.OnUpdateActionsToken();
            if (controller)
            {
                foreach (var action in actions)
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