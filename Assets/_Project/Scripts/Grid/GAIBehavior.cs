using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GAIBehavior : ScriptableObject
{
    public enum EBehaviorState {LookingForTarget, RunningToAltar, TryingToPush, TryingToPlaceCrown}
    
    [HideInInspector]
    public List<GAction> actions;
    [HideInInspector]
    public EBehaviorState behaviorState = EBehaviorState.LookingForTarget;

    protected GAIController _controller;

    public virtual void Init(GAIController controller)
    {
        _controller = controller;
    }
    
    public abstract GAction GetAction();

    public abstract void OnTurnEnd();
    
    public virtual void ChangeState(EBehaviorState newState)
    {
        OnStateExit();
        behaviorState = newState;
        OnStateEnter();
    }

    public abstract void OnActionOver();
    
    public void OnReceivedCrown()
    {
        ChangeState(EBehaviorState.LookingForTarget);
    }

    public void OnLoseCrown()
    {
        ChangeState(EBehaviorState.LookingForTarget);
    }
    
    protected virtual void  OnStateEnter()
    {
        
    }

    protected virtual void OnStateExit()
    {
        
    }

    protected virtual GMoveAction CreateMoveAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GMoveAction moveAction = new GMoveAction();
        moveAction.linkedPawn = _controller.pawn;
        moveAction.targetCell = GetTargetCell(_controller.pawn.currentCell, targetCell, _controller.pawn.moveDistance);
        moveAction.OnActionFinished += OnActionOver;
        moveAction.OnActionFinished += inOnActionFinished;
        return moveAction;
    }
    
    
    protected GCell GetTargetCell(GCell startCell, GCell endCell, int distance)
    {
        
        var path = GGridManager.Instance.GetPath(startCell, GGridManager.Instance.GetLowestAdjacentCell(endCell), false, _controller.pawn.moveDistance);
        GCell cell = startCell;
        foreach (EHexDirection direction in path)
        {
            cell = cell._neighbors[(int)direction];
        }
        return cell;
    }

}