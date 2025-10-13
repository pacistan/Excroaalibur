using System;
using System.Linq;
using UnityEngine;

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
                if (crown != null)
                {
                    if (distance == 1 && crown.owner.isPlayer)
                    {
                        ChangeState(EBehaviorState.LookingForCrown);
                        // TODO : Return PunchAction
                        return null;
                    }
                    else
                    {
                        ChangeState(EBehaviorState.TryingToPunch);
                        GMoveAction moveAction = new GMoveAction();
                        moveAction.linkedPawn = _controller.pawn;
                        moveAction.targetCell = crown.cell;
                        return moveAction;
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
                        ChangeState(EBehaviorState.LookingForCrown);
                        // TODO : Return Place Crown
                        return null;
                    }
                    else
                    {
                        GMoveAction moveAction = new GMoveAction();
                        moveAction.linkedPawn = _controller.pawn;
                        moveAction.targetCell = receptacle.cell;
                        ChangeState(EBehaviorState.TryingToPlaceCrown);
                        return moveAction;
                    }
                }
            } break;
            case EBehaviorState.TryingToPunch:
            {
                GCrown crown = GetPotentialTargetCrown(out int distance);
                if (distance == 1 && crown.owner.isPlayer)
                {
                    ChangeState(EBehaviorState.LookingForCrown);
                    // TODO : Return PunchAction
                    return null;
                }
            } break;
            case EBehaviorState.TryingToPlaceCrown:
            {
                GReceptacle crown = GetPotentialTargetAltar(out int distance);
                if (distance == 1)
                {
                    ChangeState(EBehaviorState.LookingForCrown);
                    // TODO : Return PlaceCrownAction
                    return null;
                }
            } break;
        }
        throw new Exception();
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

    private GCrown GetPotentialTargetCrown(out int distance)
    {
        distance = -1;
        GCrown[] crowns = GameObject.FindObjectsOfType<GCrown>().Where(crown => crown.owner == null || crown.owner.isPlayer).ToArray();

        if (crowns == null || crowns.Length == 0) return null;
        
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
        distance = -1;
        GReceptacle[] receptacles = GameObject.FindObjectsOfType<GReceptacle>();

        if (receptacles == null || receptacles.Length == 0) return null;
        
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