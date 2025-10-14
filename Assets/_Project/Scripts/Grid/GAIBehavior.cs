using System;
using System.Collections.Generic;
using System.Linq;

public abstract class GAIBehavior
{
    public enum EBehaviorState {LookingForCrown, RunningToAltar, TryingToPunch, TryingToPlaceCrown}
    public EBehaviorState behaviorState = EBehaviorState.LookingForCrown;

    protected GAIController _controller;
    
    protected GAIBehavior(){ }
    
    public GAIBehavior(GAIController controller)
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
        ChangeState(EBehaviorState.LookingForCrown);
    }

    public void OnLoseCrown()
    {
        ChangeState(EBehaviorState.LookingForCrown);
    }
    
    protected virtual void  OnStateEnter()
    {
        
    }

    protected virtual void OnStateExit()
    {
        
    }

    protected GMoveAction CreateMoveAction(GCell targetCell, Action inOnActionFinished = null)
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
        var path = GGridManager.Instance.GetPath(startCell, endCell, false, _controller.pawn.moveDistance);
        GCell cell = startCell;
        foreach (EHexDirection direction in path)
        {
            cell = cell._neighbors[(int)direction];
        }
        return cell;
    }

}

public class GSentryBehavior : GAIBehavior
{
    public GSentryBehavior(GAIController controller)
    {
        _controller = controller;
    }
    
    public override GAction GetAction()
    {
        switch (behaviorState)
        {
            case EBehaviorState.LookingForCrown:
            {
                GCrown crown = GetPotentialTargetCrown(out int distance);
                if (!crown)
                {
                    if (distance == 1 && crown.owner.isPlayer)
                    {
                        ChangeState(EBehaviorState.LookingForCrown);
                        // TODO : Return PunchAction with ChangeState LookingForCrown
                        return null;
                    }
                    else
                    {
                        return CreateMoveAction(crown.cell, ()=>ChangeState(EBehaviorState.TryingToPunch));
                    }
                }
                
            } break;
            case EBehaviorState.RunningToAltar:
            {
                GReceptacle receptacle = GetPotentialTargetAltar(out int distance);
                if (receptacle != null)
                {
                    if (distance == 1)
                    {
                        // TODO : Return Place Crown with ChangeState LookingForCrown
                        return null;
                    }
                    else
                    {
                        return  CreateMoveAction(receptacle.cell, ()=>ChangeState(EBehaviorState.TryingToPlaceCrown));
                    }
                }
            } break;
            case EBehaviorState.TryingToPunch:
            {
                GCrown crown = GetPotentialTargetCrown(out int distance);
                if (distance == 1 && crown.owner.isPlayer)
                {
                    // TODO : Return PunchAction with Change State t LookingForCrown
                    return null;
                }
            } break;
            case EBehaviorState.TryingToPlaceCrown:
            {
                GReceptacle crown = GetPotentialTargetAltar(out int distance);
                if (distance == 1)
                {
                    // TODO : Return PlaceCrownAction with Change State Looking for Crown
                    return null;
                }
            } break;
        }
        return null;
    }

    public override void OnTurnEnd()
    {
        if (behaviorState == EBehaviorState.TryingToPunch)
        {
            ChangeState(EBehaviorState.LookingForCrown);
        }
        else if (behaviorState == EBehaviorState.TryingToPlaceCrown)
        {
            ChangeState(EBehaviorState.RunningToAltar);
        }
    }

    public override void OnActionOver()
    {
    }

    private GCrown GetPotentialTargetCrown(out int distance)
    {
        distance = -1;
        IEnumerable<GCrown> crowns = GGridObjectRegistry.Instance.GetItemsByPredicate<GCrown>(crown => crown.owner == null || crown.owner.isPlayer);

        if (crowns == null || crowns.Count() == 0) return null;
        
        GGridManager.Instance.GenerateStepMap(_controller.pawn.currentCell);
        
        int shortestDistance = int.MaxValue;
        GCrown targetCrown = null;
        foreach (var crown in crowns)
        {
            int crownStep = GGridManager.Instance.GetStep(crown.cell);
            if (crownStep != -1 && crownStep < shortestDistance) 
            {
                shortestDistance = crownStep;
                targetCrown = crown;
            }
        }
        distance = shortestDistance;
        return targetCrown;
    }
    
    private GReceptacle GetPotentialTargetAltar(out int distance)
    {
        //TODO : Create a Manager that has all altars reference in order to reduce overhead of FindObjects all the time
        distance = -1;
        List<GReceptacle> receptacles = GGridObjectRegistry.Instance.GetItems<GReceptacle>();

        if (receptacles == null || receptacles.Count() == 0) return null;
        
        GGridManager.Instance.GenerateStepMap(_controller.pawn.currentCell);
        
        int shortestDistance = int.MaxValue;
        GReceptacle targetReceptacle = null;
        foreach (var receptacle in receptacles)
        {
            int crownStep = GGridManager.Instance.GetStep(receptacle.cell);
            if (crownStep != -1 && crownStep < shortestDistance) 
            {
                shortestDistance = crownStep;
                targetReceptacle = receptacle;
            }
        }
        distance = shortestDistance;
        return targetReceptacle;
    }

}