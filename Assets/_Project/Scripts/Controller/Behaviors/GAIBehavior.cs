using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GAIBehavior : ScriptableObject
{
    public enum EBehaviorState {LookingForTarget, RunningToAltar, TryingToPush, TryingToPlaceCrown}
    
    [HideInInspector]
    public List<GAction> actions;
    [HideInEditorMode]
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
        Debug.Log(newState.ToString());
        behaviorState = newState;
        OnStateEnter();
    }

    public abstract void OnActionOver();
    
    public void OnReceivedEquipment(GEquipment equipment)
    {
        if (equipment is GCrown)
        {
            ChangeState(EBehaviorState.RunningToAltar);
        }
    }

    public void OnLoseEquipment(GEquipment lostItem)
    {
        if (lostItem is GCrown)
        {
            ChangeState(EBehaviorState.LookingForTarget);
        }
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
        moveAction.targetCell = GetClosestCellToTargetCell(_controller.pawn.currentCell, targetCell, moveAction._maxMoveDistance);
        moveAction.OnActionFinished += OnActionOver;
        moveAction.OnActionFinished += inOnActionFinished;
        return moveAction;
    }
    
    protected virtual GPushAction CreatePushAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GPushAction pushAction = new GPushAction();
        pushAction.linkedPawn = _controller.pawn;
        pushAction.targetCell = targetCell;
        pushAction.OnActionFinished += OnActionOver;
        pushAction.OnActionFinished += inOnActionFinished;
        return pushAction;
    }
    
    
    protected GCell GetClosestCellToTargetCell(GCell startCell, GCell endCell, int distance)
    {
        var path = GGridManager.Instance.GetPath(startCell, GGridManager.Instance.GetLowestAdjacentCell(endCell), false, distance);
        GCell cell = startCell;
        foreach (EHexDirection direction in path)
        {
            cell = cell._neighbors[(int)direction];
        }
        return cell;
    }
    
    protected virtual GPlaceOnAltarAction CreatePlaceOnAltarAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GPlaceOnAltarAction pushAction = new GPlaceOnAltarAction();
        pushAction.linkedPawn = _controller.pawn;
        pushAction.targetCell = targetCell;
        pushAction.OnActionFinished += OnActionOver;
        pushAction.OnActionFinished += inOnActionFinished;
        return pushAction;
    }

}