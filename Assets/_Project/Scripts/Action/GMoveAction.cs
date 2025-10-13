using System.Collections.Generic;
using UnityEngine;

public class GMoveAction : GAction
{
    private EHexDirection[] _path = new EHexDirection[] { };
    
    public override void PreProcess()
    {
        _path = GGridManager.Instance.GetPath(linkedPion.currentCell ,targetCell, true);
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