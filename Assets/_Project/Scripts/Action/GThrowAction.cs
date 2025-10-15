using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GThrowAction : GAction
{
    [SerializeField, Min(0)]
    private int _maxThrowDistance = 10;
    [SerializeField]
    private int _damage = 1;
    
    EHexDirection _direction = EHexDirection.NE;
    
    public override void PreProcess()
    {
        _direction = linkedPawn.coordinate.GetLineDirection(targetCell._hexCoordinates);
        
        GCell pathCell = linkedPawn.currentCell;
        List<GCell> pathCells = new List<GCell>();
        
        for (int i = 0; i < _maxThrowDistance; i++)
        {
            GCell neighbor = pathCell.GetNeighbor(_direction);
            pathCells.Add(pathCell);
            pathCell = neighbor;
        }
    }

    public override void Start_Action()
    {
        base.Start_Action();
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
    }

    public override void End_Action()
    {
        base.End_Action();
    }

    public override GHexCoordinate[] GetValidCells()
    {
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();
        GCell startCell = linkedPawn.currentCell;
    
        foreach (EHexDirection direction in Enum.GetValues(typeof(EHexDirection)))
        {
            GCell cell = startCell;
            for (int i = 0; i < _maxThrowDistance; i++)
            {
                cell = cell.GetNeighbor(direction);

                if (!cell || cell.GetTileType == GCellData.ETileType.Wall) break;
                if (cell.GetTileType == GCellData.ETileType.Hole) continue;
                
                newValidCells.Add(cell._hexCoordinates);
            }
        }
        
        return validCells = newValidCells.ToArray();
    }
}