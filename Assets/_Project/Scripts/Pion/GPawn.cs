using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GPawn : GGridObject
{
    public event Action<GEquipment> OnEquip;
    public event Action<GEquipment> OnUnequip;
    public event Action OnKill;
    public event Action OnStunned;
    public event Action OnUnstunned;
    public event Action OnHealthChanged;
    
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

    [SerializeField, FoldoutGroup("Components")]
    GPawnVisualsController _visuals;
    
    [SerializeField, ReadOnly, HideInEditorMode, FormerlySerializedAs("_stunTurn")] 
    public int stunTurn = 0;
    
    public bool IsStunned => stunTurn > 0;

    [field: SerializeField, HideIf("@hp == -1")]
    public int hp { get; protected set; } = 3;

    [field : SerializeField, FoldoutGroup("Components")]
    public Transform _equipmentParentTr { get; private set; }
    
    // Cache for quick look-up of override reactions
    private Dictionary<Type, GAction> _overrideCache;


    
    public GAction GetReaction(GAction action)
    {
        if (action == null) return null;
      
        var t = action.GetType();
        
        // Exact type 
        if (_overrideCache != null && _overrideCache.TryGetValue(t, out var overExact) && overExact != null)
            return overExact;

        // Parent type
        var bt = t.BaseType;
        while (bt != null && typeof(GAction).IsAssignableFrom(bt))
        {
            if (_overrideCache.TryGetValue(bt, out var overBase) && overBase != null)
                return overBase;
            bt = bt.BaseType;
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
            equipment.transform.parent = _equipmentParentTr;
            equipment.transform.localPosition = Vector3.zero; 
        }
    }
    
    public void ReleaseEquipement(bool giveToCell)
    {
        if (equipment == null) return;
        equipment.owner = null;
        OnUnequip?.Invoke(equipment);
        
        if (giveToCell) 
            GetCell().gridObject = equipment;
        
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
            GetCell().gridObject = null;
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
    }

    public void Kill()
    {
        OnKill?.Invoke();
        isMarkedForDestruction = true;
        Destroy(gameObject);
    }

    public void OnStartAction()
    {
        
    }
    
    public void OnStartTurn()
    {
        remainingActionToken = actionTokens;
        if (stunTurn > 0)
            stunTurn--;
        if (stunTurn == 0)
        {
            OnUnstunned?.Invoke();
        }
    }

    public void UpdateStunTurn()
    {
        _visuals.OnUpdateStunTurn();
    }

    public void UpdateHpNumber()
    {
        _visuals.OnUpdateHealthPoints();
    }
    
    public void OnEndTurn()
    {
        
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
        GetCell().gridObject = this;
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
        
        EditorUtility.SetDirty(this);
    }


    protected virtual void Awake()
    {
        if (isPlayer) hp = -1; // Player has infinite HP
        RebuildOverrideCache();
    }

    protected virtual void Start()
    {
        remainingActionToken = actionTokens;
        if (!isPlayer && TryGetComponent(out GController aiController))
        {
            aiController.RegisterPawn(this);
        }
        else
        {
            GPlayerController controller = FindFirstObjectByType<GPlayerController>();
            if (controller)
            {
                foreach (var action in actions)
                {
                    action.linkedPawn = this;
                    controller.RegisterPawn(this);
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            _visuals.UpdatePawnVisuals(); 
        }
        
        RebuildOverrideCache();
    }
#endif
    
}