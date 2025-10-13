using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class GPion : MonoBehaviour
{
    [ReadOnly]
    public GHexCoordinate coordinate;
    [ReadOnly]
    public GCell currentCell;
    [SerializeField] public int moveDistance = 1;
    [SerializeReference] public List<GAction> actions = new List<GAction>();
    public bool isPlayer;

    [SerializeField] private int _hp = 3;
    [ReadOnly] int _stunTurn = 0;

    public void SetCell(GHexCoordinate newCoordinate)
    {
        SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public void SetCell(GCell newCell)
    {
        if (!newCell) return;
        if (currentCell) currentCell.SetPion(null); 
        currentCell = newCell;
        coordinate = newCell._hexCoordinates;
        currentCell.SetPion(this);
    }
    
    public void RequestAction(GAction action)
    {
        if (action == null || !action.IsValid()) return;
        
        //TODO Request action to the turn manager
    }

    public void TakeDamage(int damage = 1)
    {
        if (_hp <= 0) return;
        _hp--;
    }
}