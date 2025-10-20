using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


public class GPawn : GGridObject
{
    public event Action<GEquipment> OnEquip;
    public event Action<GEquipment> OnUnequip;

    public event Action OnKill;

    public event Action OnStunned;
    public event Action OnUnstunned;
    public event Action<int> OnHealthChanged;
    
    [SerializeField]
    public EEquipmentType equipmentType = EEquipmentType.None;
    
    [SerializeField]
    public bool isPlayer;
    
    [SerializeReference, ShowIf("isPlayer")]
    public List<GAction> actions = new List<GAction>();
    
    [SerializeField, FormerlySerializedAs("BaseReactionData")]
    public GReactionData baseReactionData;
    
    [SerializeField, FormerlySerializedAs("OverrideReactionData")]
    public GReactionData overrideReactionData;
    
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
    
    public GReaction GetReaction(GAction action)
    {
        if (overrideReactionData && overrideReactionData.HasReaction(action))
            return overrideReactionData.GetReaction(action);
        if (baseReactionData && baseReactionData.HasReaction(action))
            return baseReactionData.GetReaction(action);

        return null;
    }
    
    public void GiveEquipement(GEquipment _equipment)
    {
        equipment = _equipment;
        OnEquip?.Invoke(equipment);
        equipment.SetOwner(this);
        equipment.transform.parent = _equipmentParentTr;
        equipment.transform.localPosition = Vector3.zero; 
    }
    
    public void ReleaseEquipement(bool giveToCell = true)
    {
        if (equipment == null) return;
        equipment.OnReleased();
        OnUnequip?.Invoke(equipment);
        
        if (giveToCell) 
            currentCell.GiveEquipement(equipment);
        
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
        OnHealthChanged?.Invoke(hp);
        if (hp == 0)
        {
            Kill();
        }
    }
    
    public void Stun(int stunTurnNumber)
    {
        if (stunTurn == 0)
        {
            OnStunned?.Invoke();
        }
        stunTurn += stunTurnNumber;
    }

    public void Fall()
    {
        if (isPlayer)
        {
            Stun(1);
        }
        else
        {
            Kill();
        }
    }

    public void Kill()
    {
        //SetCell(null);
        OnKill?.Invoke();
        Destroy(gameObject);
    }

    public void OnStartAction()
    {
        
    }
    
    public void OnStartTurn()
    {
        if (stunTurn > 0)
            stunTurn--;
        if (stunTurn == 0)
        {
            OnUnstunned?.Invoke();
        }
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
        if (newCell.GetTileType == ETileType.Hole)
        {
            Fall();
        }
        base.SetCell(newCell);
        currentCell.ownedPawn = this;
    }

    protected virtual void Awake()
    {
        if (isPlayer) hp = -1; // Player has infinite HP
    }

    protected virtual void Start()
    {
        if (!isPlayer && TryGetComponent(out GController aiController))
        {
            aiController.RegisterPawn(this);
        }
        else
        {
            GPlayerController controller = FindFirstObjectByType<GPlayerController>();
            if (controller)
            foreach (var action in actions)
                action.linkedPawn = this;
        }
    }

    protected virtual void OnDestroy()
    {
        if (equipment != null)
        {
            ReleaseEquipement(true);
        }
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            _visuals.UpdatePawnVisuals(); 
        }
    }
#endif
    
}