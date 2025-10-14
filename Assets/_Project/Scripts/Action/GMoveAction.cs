using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;


public class GMoveAction : GAction
{
    [SerializeField]
    public int _maxMoveDistance = 2;
    
    private EHexDirection[] _path = new EHexDirection[] { };
    private Vector3[] _wayPoints = new Vector3[] { };
    Dictionary<int, GCell> _wayPointsCells;
    float _progress = 0;
    int _currentWayPoint = 0;

    public GMoveAction(){}
    
    public GMoveAction(GPawn inLinkedPawn, GCell inTargetCell, Action inOnActionStarted = null, Action inOnActionFinished = null) : base(inLinkedPawn, inTargetCell, inOnActionStarted, inOnActionFinished)
    {
    }

    public override void PreProcess()
    {
        _path = GGridManager.Instance.GetPath(linkedPawn.currentCell ,targetCell, true);
        if (_path == null || _path.Length == 0 || _path.Length > _maxMoveDistance) return;
        
        _wayPointsCells = new Dictionary<int, GCell>();

        GCell cell = linkedPawn.currentCell;
        List<Vector3> wayPoints = new List<Vector3>();
        wayPoints.Add(cell.transform.position);
        for(int i = 0; i < _path.Length; i++)
        {
            cell = cell._neighbors[(int)_path[i]];
            if (cell == null) break;
            _wayPointsCells.Add(i, cell);
            wayPoints.Add(cell.transform.position);
        }
        _wayPoints = wayPoints.ToArray();
        
    }

    public override void Start_Action()
    {
        base.Start_Action();
        linkedPawn.SetCell(targetCell);
        _progress = 0;
        _currentWayPoint = 0;
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        _progress += delta;

        if (_progress > _currentWayPoint)
        {
            if (_wayPointsCells.ContainsKey(_currentWayPoint))
            {
                GCell cell = _wayPointsCells[_currentWayPoint];
                GEquipment equipment = cell._equipment;
                if (equipment)
                {
                    cell.ReleaseEquipement();
                    linkedPawn.Posess(equipment);
                }
            }
            _currentWayPoint++;
        }

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

    public override GHexCoordinate[] GetValidCells()
    {
        GGridManager.Instance.GenerateStepMap(linkedPawn.currentCell);
        Dictionary<Vector2Int, int> stepMap = GGridManager.Instance._stepMap;
        List<GHexCoordinate> validCells = new List<GHexCoordinate>();

        foreach (var step in stepMap)
        {
            GHexCoordinate coordinate = GHexCoordinate.FrommOffsetCoordinate(step.Key.x, step.Key.y);
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            
            if (!cell || !cell.IsWalkable() || step.Value > _maxMoveDistance) continue;
            
            validCells.Add(coordinate);
        }
        
        return validCells.ToArray();
    }
    
    public override GAction CloneAction()
    {
        GAction action = base.CloneAction();
        ((GMoveAction)action)._maxMoveDistance = _maxMoveDistance;
        return action;
    }

    private void LootEquipment(GEquipment equipment)
    {
        
    }
}