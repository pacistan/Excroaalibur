using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GMoveOutOfHoleAction : GAction
{
    public override void PreProcess(GActionContext context = null)
    {
        if (!targetCell || targetCell.GetPawn() || targetCell.GetPawn() == linkedPawn) return;
        //validate
    }

    public override void Start_Action()
    {
        base.Start_Action();
        linkedPawn.SetCell(targetCell);
        linkedPawn.transform.DOMove(targetCell.transform.position, .5f).SetEase(Ease.OutBack).onComplete = End_Action;
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
        if (linkedPawn.currentCell.GetTileType != ETileType.Hole)
            return validCells = new GHexCoordinate[]{};
        
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();

        foreach (var cell in linkedPawn.currentCell._neighbors)
        {
            if (!cell || !cell.IsWalkable()) continue;
            
            newValidCells.Add(cell._hexCoordinates);
        }
        
        return validCells = newValidCells.ToArray();
    }
}