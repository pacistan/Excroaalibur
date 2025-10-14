using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class GPawn : GGridObject
{
    public event Action<GEquipment> OnEquip;
    public event Action<GEquipment> OnUnequip;
    
    [SerializeField]
    public bool isPlayer;
    [SerializeReference]
    public List<GAction> actions = new List<GAction>();
    public bool IsStunned => _stunTurn > 0;
    
    [SerializeField] 
    private int _hp = 3;
    
    [ReadOnly] 
    int _stunTurn = 0;

    [FormerlySerializedAs("_equipments")]
    [SerializeField, ReadOnly]
    GEquipment _equipment;

    [SerializeField, FoldoutGroup("Components")]
    Transform _equipmentParentTr;

    void Start()
    {
        if (!isPlayer) return;
        GPlayerController controller = FindFirstObjectByType<GPlayerController>();
        if (controller)
            controller.RegisterPawn(this);
    }

    public void Posess(GEquipment equipment)
    {
        _equipment = equipment;
        OnEquip?.Invoke(equipment);
        _equipment.transform.parent = _equipmentParentTr;
        _equipment.transform.localPosition = Vector3.zero; 
    }

    public void Release()
    {
        _equipment.OnReleased();
        OnUnequip?.Invoke(_equipment);
        _equipment = null;
    }
    
    public override void SetCell(GHexCoordinate newCoordinate)
    {
        base.SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public override void SetCell(GCell newCell)
    {
        base.SetCell(newCell);
        currentCell.SetPawn(this);
    }
    
    public bool RequestAction(GAction action)
    {
        if (action == null) return false;
        action.linkedPawn = this;
        print("Request " + action.ToString());
        if (!GTurnBaseManager.Instance.TryPlayAction(action, false))
        {
            print("Action Failed");
            return false;
        }
        
        return true;
    }

    public void TakeDamage(int damage = 1, int stun = 0)
    {
        if (_hp <= 0) return;
        _stunTurn += stun;
        _hp--;
        if (_hp < 0)
        {
            Kill();
        }
    }

    public void OnStartTurn()
    {
        if (_stunTurn > 0)
            _stunTurn--;
    }

    public void OnEndTurn()
    {
        
    }

    public void Kill()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}