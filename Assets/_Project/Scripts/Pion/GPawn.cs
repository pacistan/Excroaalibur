using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class GPawn : GGridObject
{
    [SerializeField]
    public bool isPlayer;
    [SerializeReference]
    public List<GAction> actions = new List<GAction>();
    public bool IsStunned => _stunTurn > 0;
    
    [SerializeField] 
    private int _hp = 3;
    
    [ReadOnly] 
    int _stunTurn = 0;

    [SerializeField, ReadOnly]
    List<GEquipment> _equipments = new List<GEquipment>();

    void Start()
    {
        if (!isPlayer) return;
        GPlayerController controller = FindFirstObjectByType<GPlayerController>();
        if (controller)
            controller.RegisterPawn(this);
    }

    public void Posess(GEquipment equipment)
    {
        _equipments.Add(equipment);
    }

    public void Release(GEquipment equipment)
    {
        if (!_equipments.Contains(equipment))
        {
            Debug.LogWarning("Release not owned Equipment", this);
        }
        equipment.OnReleased();
        _equipments.Remove(equipment);
    }
    
    public void ForceRelease()
    {
        _equipments.ForEach(a => Release(a));
        _equipments.Clear();
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
    }

    public void OnStartTurn()
    {
        if (_stunTurn > 0)
            _stunTurn--;
    }

    public void OnEndTurn()
    {
        
    }
}