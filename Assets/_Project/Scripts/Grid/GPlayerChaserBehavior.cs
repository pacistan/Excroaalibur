using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "AI Behavior/Player Chaser", fileName = "Player Chaser Behaviour")]
public class GPlayerChaserBehavior : GAIBehavior
{
    [SerializeReference]
    GMoveAction _moveAction;
    
    public override void Init(GAIController controller)
    {
        base.Init(controller);
        actions = new List<GAction>()        {
            _moveAction
        };
    }
    
    public override GAction GetAction()
    {
        GGridManager.Instance.GenerateStepMap(_controller.pawn.currentCell);
        GPawn player = GetPotentialTargetPlayer(out int distance);
        GCell cell = player.currentCell;
        GAction currentAction = CreateMoveAction(cell);
        return currentAction;
    }

    public override void OnTurnEnd()
    {
    }

    public override void OnActionOver()
    {
    }
    
    private GPawn GetPotentialTargetPlayer(out int distance)
    {
        distance = -1;
        IEnumerable<GPawn> pawns = GGridObjectRegistry.Instance.GetItems<GPawn>();
        GPawn[] players =  pawns.Where(p => p.isPlayer).ToArray();

        if (players == null || players.Count() == 0) return null;
        
        GGridManager.Instance.GenerateStepMap(_controller.pawn.currentCell);
        
        int shortestDistance = int.MaxValue;
        GPawn targetPlayer = null;
        foreach (var player in players)
        {
            int step = GGridManager.Instance.GetStep(player.currentCell, true);
            if (step != -1 && step < shortestDistance) 
            {
                shortestDistance = step;
                targetPlayer = player;
            }
        }
        distance = shortestDistance;
        return targetPlayer;
    }
    
    
    protected override GMoveAction CreateMoveAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GMoveAction moveAction = (GMoveAction)_moveAction.Duplicate();
        moveAction.linkedPawn = _controller.pawn;
        //TODO : Replace moveDistance by the field in the action
        moveAction.targetCell = GetTargetCell(_controller.pawn.currentCell, targetCell, moveAction._maxMoveDistance);
        moveAction.OnActionFinished += OnActionOver;
        moveAction.OnActionFinished += inOnActionFinished;
        return moveAction;
    }
}
