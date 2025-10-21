using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GMoveOutOfHoleAction : GAction
{
    public override void PreProcess(GActionContext context = null)
    {
        if (!targetCell || targetCell.GetGridObject<GPawn>() || targetCell.GetGridObject<GPawn>() == linkedPawn) return;
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
        if (linkedPawn.GetCell().GetTileType != ETileType.Hole)
            return validCells = new GHexCoordinate[]{};
        
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();
        
        foreach (var cell in linkedPawn.GetCell().neighbors)
        {
            if (!cell || !cell.IsWalkable()) continue;
            
            newValidCells.Add(cell.hexCoordinates);
        }
        
        return validCells = newValidCells.ToArray();
    }
}