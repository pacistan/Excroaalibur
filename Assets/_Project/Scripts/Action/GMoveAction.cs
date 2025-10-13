using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class GMoveAction : GAction
{
    private EHexDirection[] _path = new EHexDirection[] { };
    private Vector3[] _wayPoints = new Vector3[] { };
    float _progress = 0;
    
    public override void PreProcess()
    {
        _path = GGridManager.Instance.GetPath(linkedPawn.currentCell ,targetCell, true);
        if (_path == null || _path.Length == 0) return;

        GCell cell = linkedPawn.currentCell;
        List<Vector3> wayPoints = new List<Vector3>();
        wayPoints.Add(cell.transform.position);
        foreach (var direction in _path)
        {
            cell = cell._neighbors[(int)direction];
            if (cell == null) break;
            wayPoints.Add(cell.transform.position);
        }
        _wayPoints = wayPoints.ToArray();
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
        if (_progress >= _wayPoints.Length - 1) 
        {
            End_Action();
            return;
        }
        int id = Mathf.FloorToInt(_progress);
        linkedPawn.transform.position = Vector3.Lerp(_wayPoints[id], _wayPoints[id+1], Mathf.Repeat(_progress, 1));
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