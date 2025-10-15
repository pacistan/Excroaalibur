using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "AI Behavior/Sentry", fileName = "Sentry Behaviour")]
public class GSentryBehavior : GAIBehavior
{
    [SerializeReference]
    GMoveAction _moveAction;

    [SerializeReference]
    GPushAction _pushAction;
    
    [SerializeReference]
    GPlaceOnAltarAction _placeOnAltarAction;
    
    public override void Init(GAIController controller)
    {
        base.Init(controller);
        actions = new List<GAction>()        {
            _moveAction,
            _pushAction,
            _placeOnAltarAction
        };

        controller.pawn.OnEquip += OnReceivedEquipment;
    }
    
    public override GAction GetAction()
    {
        switch (behaviorState)
        {
            case EBehaviorState.LookingForTarget:
            {
                GCrown crown = GetPotentialTargetCrown(out int distance);
                if (crown)
                {
                    if (distance == 1 && crown.owner && crown.owner.isPlayer)
                    {
                        return CreatePushAction(crown.GetCell(), () => ChangeState(EBehaviorState.LookingForTarget));
                    }
                    else
                    {
                        ChangeState(EBehaviorState.TryingToPush);
                        return CreateMoveAction(crown.GetCell());
                    }
                }
                
            } break;
            case EBehaviorState.RunningToAltar:
            {
                GAltar altar = GetPotentialTargetAltar(out int distance);
                if (altar != null)
                {
                    if (distance == 1)
                    {
                        return CreatePlaceOnAltarAction(altar.currentCell, () => ChangeState(EBehaviorState.LookingForTarget));
                    }
                    else
                    {
                        return  CreateMoveAction(altar.currentCell, ()=>ChangeState(EBehaviorState.TryingToPlaceCrown));
                    }
                }
            } break;
            case EBehaviorState.TryingToPush:
            {
                GCrown crown = GetPotentialTargetCrown(out int distance);
                if (distance == 1 && crown.owner.isPlayer)
                {
                    return CreatePushAction(crown.GetCell(), () => ChangeState(EBehaviorState.LookingForTarget));
                }
            } break;
            case EBehaviorState.TryingToPlaceCrown:
            {
                GAltar altar = GetPotentialTargetAltar(out int distance);
                if (distance == 1)
                {
                    return CreatePlaceOnAltarAction(altar.currentCell, () => ChangeState(EBehaviorState.LookingForTarget));
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
            GCell crownCell = crown.owner ? crown.owner.currentCell : crown.currentCell;
            int crownStep =  GGridManager.Instance.GetStep(crownCell, true);
            if (crownStep != -1 && crownStep < shortestDistance) 
            {
                shortestDistance = crownStep;
                targetCrown = crown;
            }
        }
        distance = shortestDistance;
        return targetCrown;
    }
    
    private GAltar GetPotentialTargetAltar(out int distance)
    {
        distance = -1;
        List<GAltar> receptacles = GGridObjectRegistry.Instance.GetItems<GAltar>();

        if (receptacles == null || receptacles.Count() == 0) return null;
        
        GGridManager.Instance.GenerateStepMap(_controller.pawn.currentCell);
        
        int shortestDistance = int.MaxValue;
        GAltar targetAltar = null;
        foreach (var receptacle in receptacles)
        {
            int crownStep = GGridManager.Instance.GetStep(receptacle.currentCell, true);
            if (crownStep != -1 && crownStep < shortestDistance) 
            {
                shortestDistance = crownStep;
                targetAltar = receptacle;
            }
        }
        distance = shortestDistance;
        return targetAltar;
    }

    protected override GMoveAction CreateMoveAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GMoveAction moveAction = (GMoveAction)_moveAction.CloneAction();
        moveAction.linkedPawn = _controller.pawn;
        moveAction.targetCell = GetClosestCellToTargetCell(_controller.pawn.currentCell, targetCell, moveAction._maxMoveDistance);
        moveAction.OnActionFinished += OnActionOver;
        moveAction.OnActionFinished += inOnActionFinished;
        return moveAction;
    }

    protected override GPushAction CreatePushAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GPushAction pushAction = (GPushAction)_pushAction.CloneAction();
        pushAction.linkedPawn = _controller.pawn;
        pushAction.targetCell = targetCell;
        pushAction.OnActionFinished += OnActionOver;
        pushAction.OnActionFinished += inOnActionFinished;
        return pushAction;
    }
    
    protected override GPlaceOnAltarAction CreatePlaceOnAltarAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GPlaceOnAltarAction placeOnAltarAction = (GPlaceOnAltarAction)_placeOnAltarAction.CloneAction();
        placeOnAltarAction.linkedPawn = _controller.pawn;
        placeOnAltarAction.targetCell = targetCell;
        placeOnAltarAction.OnActionFinished += OnActionOver;
        placeOnAltarAction.OnActionFinished += inOnActionFinished;
        return placeOnAltarAction;
    }

}
