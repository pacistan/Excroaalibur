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
        if (distance == 1) return null;
        GCell cell = player.currentCell;
        GAction currentAction = CreateMoveAction(cell);
        return currentAction;
    }

    protected override void GetAction<T>(out T action)
    {
        string typeName = typeof(T).Name;
        if (typeName == "GMoveAction")
        {
            action = _moveAction as T;
        }
        else action = null;
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
}
