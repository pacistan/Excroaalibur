using System.Collections.Generic;
using UnityEngine;

public class GMoveAction : GAction
{
    private EHexDirection[] _path = new EHexDirection[] { };

    float _progress = 0;
    
    public override void PreProcess()
    {
        _path = GGridManager.Instance.GetPath(linkedPion.currentCell ,targetCell, true);
        if (_path == null || _path.Length == 0) return;
    }

    public override void Start_Action()
    {
        base.Start_Action();
        linkedPion.SetCell(targetCell);
        _progress = 0;
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        _progress += delta;
        if (_progress >= 1)
        {
            //Switch state end action
        }
        linkedPion.transform.position = Vector3.Lerp(linkedPion.transform.position, targetCell.transform.position, _progress);
    }

    public override void End_Action()
    {
        base.End_Action();
        linkedPion.transform.position = targetCell.transform.position;
    }

    public override bool IsValid()
    {
        //TODO Check valid path
        return true;
    }
}