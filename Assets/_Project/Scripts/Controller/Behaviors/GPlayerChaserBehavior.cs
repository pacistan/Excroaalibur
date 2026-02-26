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
        actions = new List<GAction>()        {
            _moveAction
        };
        base.Init(controller);
    }
    
    public override GAction GetAction()
    {
        GGridManager.Instance.GenerateStepMap(_controller.currentPawn.GetCell());
        GPawn player = GetPotentialTargetPlayer(out int distance);
        if (distance == 1) return null;
        GCell cell = player.GetCell();
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
}
