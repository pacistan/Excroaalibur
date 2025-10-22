using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using STOP_MODE = FMOD.Studio.STOP_MODE;


public class GMoveAction : GAction
{
    [SerializeField]
    public int _maxMoveDistance = 2;

    [SerializeField]
    public ETileType[] _walkingTileType = new[]{ETileType.Normal};

    [SerializeField]
    public ETileType[] _endMovementTileType = new[]{ETileType.Normal};
    
    [SerializeField]
    protected float _speed = 1f;
    
    [SerializeField]
    protected AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private EHexDirection[] _path = new EHexDirection[] { };
    private Vector3[] _wayPoints = new Vector3[] { };
    float _progress = 0;
    int _currentWayPoint = 0;
    
    int _EquipementPickUpIndex = -1; // No Equipement to Picked Up

    String MoveEvent = "event:/Pawn/Move";
    EventInstance MoveEventInstance = new EventInstance();
    
    public GMoveAction(){}
    
    public GMoveAction(GPawn inLinkedPawn, GCell inTargetCell, Action inOnActionStarted = null, Action inOnActionFinished = null) : base(inLinkedPawn, inTargetCell, inOnActionStarted, inOnActionFinished)
    {
    }

    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess(context);
        GGridManager.Instance.GenerateStepMap(linkedPawn.GetCell(), _walkingTileType);
        _path = GGridManager.Instance.GetPath(linkedPawn.GetCell() ,targetCell, _endMovementTileType,false);
        if (_path == null || _path.Length == 0 || _path.Length > _maxMoveDistance) return;
        
        GCell cell = linkedPawn.GetCell();

        List<Vector3> wayPoints = new List<Vector3>();
        wayPoints.Add(cell.transform.position);
        for(int i = 0; i < _path.Length; i++)
        {
            cell = cell.neighbors[(int)_path[i]];
            if (cell == null) break;
            GEquipment equipment = cell.GetGridObject<GEquipment>();
            if (equipment && linkedPawn.GetCell() != cell)
            {
                linkedPawn.GiveEquipement(equipment, false, false);
                _EquipementPickUpIndex = i + 1; // pick up between waypoints
            }
               
            wayPoints.Add(cell.transform.position);
        }
        _wayPoints = wayPoints.ToArray();
        linkedPawn.SetCell(targetCell);
    }

    public override void Start_Action()
    {
        base.Start_Action();
        _progress = 0;
        _currentWayPoint = 0;

        linkedPawn.visuals.SetAnimationState("Walking");
        MoveEventInstance = RuntimeManager.CreateInstance(MoveEvent);
        MoveEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(linkedPawn.gameObject));
        MoveEventInstance.start();
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        _progress += delta * _speed;
        float animatedProgress = _speedCurve.Evaluate(_progress);
        
        if (_progress > _currentWayPoint)
        {
            if (_EquipementPickUpIndex == _currentWayPoint)
            { 
                linkedPawn.GiveEquipement(linkedPawn.equipment, true, true);
            }
            _currentWayPoint++;

            if (_wayPoints.Length > 0)
            {
                Vector3 lookAtPosition = _wayPoints[Mathf.Min(_currentWayPoint + 1, _wayPoints.Length - 1)];
                lookAtPosition.y = linkedPawn.transform.position.y;
                linkedPawn.transform.LookAt(lookAtPosition);
            }
        }

        if (_progress > _wayPoints.Length - 1) 
        {
            End_Action();
            return;
        }
        int id = Mathf.FloorToInt(_progress);
        linkedPawn.transform.position = Vector3.Lerp(_wayPoints[id], _wayPoints[id+1], Mathf.Repeat(_progress, 1));
    }

    public override void End_Action()
    {
        linkedPawn.transform.position = targetCell.transform.position;
        linkedPawn.visuals.SetAnimationState("Idle");
        MoveEventInstance.stop(STOP_MODE.ALLOWFADEOUT);
        if (!linkedPawn.IsAlive) linkedPawn.Kill();
        base.End_Action();
    }

    public override GHexCoordinate[] GetValidCells()
    {
        GGridManager.Instance.GenerateStepMap(linkedPawn.GetCell());
        Dictionary<Vector2Int, int> stepMap = GGridManager.Instance._stepMap;
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();

        foreach (var step in stepMap)
        {
            GHexCoordinate coordinate = GHexCoordinate.FrommOffsetCoordinate(step.Key.x, step.Key.y);
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            
            if (!cell || !cell.IsWalkable() || step.Value > _maxMoveDistance) continue;
            if (GGridManager.Instance.GetPath(linkedPawn.GetCell(), cell).Length <= 0) continue;
            newValidCells.Add(coordinate);
        }
        
        
        return validCells = newValidCells.ToArray();
    }
    
    public override GAction CloneAction()
    {
        GMoveAction moveAction = base.CloneAction() as GMoveAction;
        moveAction._maxMoveDistance = _maxMoveDistance;
        moveAction._speed = _speed;
        moveAction._speedCurve = _speedCurve;
        moveAction._walkingTileType = _walkingTileType;
        moveAction._endMovementTileType = _endMovementTileType;
        
        return moveAction;
    }
}