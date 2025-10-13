using System.Collections.Generic;
using UnityEngine;

public class GMoveAction : GAction
{
    private EHexDirection[] _path = new EHexDirection[] { };

    float _progress = 0;
    
    public override void PreProcess()
    {
        _path = GGridManager.Instance.GetPath(linkedPawn.currentCell ,targetCell, true);
        if (_path == null || _path.Length == 0) return;
    }

    public override void Start_Action()
    {
        base.Start_Action();
        linkedPawn.SetCell(targetCell);
        _progress = 0;
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        _progress += delta;
        if (_progress >= 1)
        {
            End_Action();
            return;
        }
        linkedPawn.transform.position = Vector3.Lerp(linkedPawn.transform.position, targetCell.transform.position, _progress);
    }

    public override void End_Action()
    {
        base.End_Action();
        linkedPawn.transform.position = targetCell.transform.position;
    }

    public override bool IsValid()
    {
        //TODO Check valid path
        return true;
    }
}