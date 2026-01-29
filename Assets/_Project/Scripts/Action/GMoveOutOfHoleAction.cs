using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GMoveOutOfHoleAction : GAction
{
    GMoveAction _moveAction = null; 
    
    public override EZoneActionType GetHighlightActionType() => EZoneActionType.Move;
    
    public override List<GCell> Previsualisation(in GActionContext previsuContext)
    {
        List<GCell> previewCells = new List<GCell>();
        previewCells.Add(targetCell);
        return previewCells;
    }

    public override void PreProcess(GActionContext context = null)
    {
        if (!targetCell || targetCell.GetGridObject<GPawn>() || targetCell.GetGridObject<GPawn>() == linkedPawn) return;
        
        ETileType[] overrideTileType = new ETileType[] { ETileType.Normal, ETileType.Hole };
        
        _moveAction = new GMoveAction();
        _moveAction.InitAction(linkedPawn);
        _moveAction.OverrideTileType(overrideTileType, overrideTileType);
        _moveAction.targetCell = targetCell;
        _moveAction.moveAnimationName = GPawn.GetOutOfHoleAnimationName;
        _moveAction.MovementMode = EMovementMode.TPAfter;
        _moveAction.IsAnimationDriven = true;
        _moveAction.OutAnimBlendTime = 0;
        GTurnBaseManager.Instance.PreProcessReaction(_moveAction, new GActionContext());
    }

    public override void Start_Action()
    {
        base.Start_Action();
        
        if (_moveAction != null)
            GTurnBaseManager.Instance.TryStartReaction(_moveAction);
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        
        if (_moveAction == null || _moveAction.CurrentState == GAction.EActionState.Finished)
            End_Action();
    }

    public override void End_Action()
    {
        base.End_Action();
        linkedPawn.visuals.SetAnimationParameter(GPawn.AnimParam_IsInHole, false);
        linkedPawn.visuals.SetAnimationState(GPawn.IdleAnimationName);
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