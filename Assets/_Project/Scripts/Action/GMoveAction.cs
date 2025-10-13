using System.Collections.Generic;
using UnityEngine;

public class GMoveAction : GAction
{
    private HexDirection[] _path = new HexDirection[] { };
    
    public override void PreProcess()
    {
        _path = GPathfindingUtility.GetPath(linkedPion.currentCell ,targetCell, GPathfindingUtility.GetStepMap(linkedPion.currentCell));
        if (_path == null || _path.Length == 0) return;
    }

    public override void Start_Action()
    {
        base.Start_Action();
        linkedPion.SetCell(targetCell);
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
    }

    public override void End_Action()
    {
        base.End_Action();
        
    }
}