using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "AI Behavior/Sentry", fileName = "Sentry Behaviour")]
public class GSentryBehavior : GAIBehavior
{
    [SerializeReference]
    GMoveAction _moveAction;

    public override void Init(GAIController controller)
    {
        base.Init(controller);
        // TODO : Replace nulls by Push Action and Place Crown Action
        actions = new List<GAction>()        {
            _moveAction,
            null,
            null
        };
    }
    
    public override GAction GetAction()
    {
        switch (behaviorState)
        {
            case EBehaviorState.LookingForTarget:
            {
                GCrown crown = GetPotentialTargetCrown(out int distance);
                if (!crown)
                {
                    if (distance == 1 && crown.owner.isPlayer)
                    {
                        // TODO : Return PunchAction with ChangeState LookingForCrown
                        return null;
                    }
                    else
                    {
                        return CreateMoveAction(crown.currentCell, ()=>ChangeState(EBehaviorState.TryingToPush));
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
            case EBehaviorState.TryingToPush:
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
        if (behaviorState == EBehaviorState.TryingToPush)
        {
            ChangeState(EBehaviorState.LookingForTarget);
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
            int crownStep = GGridManager.Instance.GetStep(crown.currentCell);
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

    protected override GMoveAction CreateMoveAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GMoveAction moveAction = (GMoveAction)_moveAction.CloneAction();
        moveAction.linkedPawn = _controller.pawn;
        //TODO : Replace moveDistance by the field in the action
        moveAction.targetCell = GetTargetCell(_controller.pawn.currentCell, targetCell, moveAction._maxMoveDistance);
        moveAction.OnActionFinished += OnActionOver;
        moveAction.OnActionFinished += inOnActionFinished;
        return moveAction;
    }
}
