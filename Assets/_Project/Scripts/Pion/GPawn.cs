using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class GPawn : MonoBehaviour
{
    [ReadOnly]
    public GHexCoordinate coordinate;
    [ReadOnly]
    public GCell currentCell;
    [SerializeField] public int moveDistance = 1;
    [SerializeReference] public List<GAction> actions = new List<GAction>();
    public bool isPlayer;
    public bool IsStunned => _stunTurn > 0;
    
    [SerializeField] private int _hp = 3;
    [ReadOnly] int _stunTurn = 0;


    public void SetCell(GHexCoordinate newCoordinate)
    {
        SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public void SetCell(GCell newCell)
    {
        if (!newCell) return;
        if (currentCell) currentCell.SetPawn(null); 
        currentCell = newCell;
        coordinate = newCell._hexCoordinates;
        currentCell.SetPawn(this);
    }
    
    public void RequestAction(GAction action)
    {
        if (action == null || !action.IsValid()) return;
        action.linkedPawn = this;
        print("Request " + action.ToString());
        if (!GTurnBaseManager.Instance.TryPlayAction(action, false))
        {
            print("Action Failed");
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (_hp <= 0) return;
        _hp--;
    }

    public void EndTurn()
    {
        if (_stunTurn > 0)
            _stunTurn--;
    }
}