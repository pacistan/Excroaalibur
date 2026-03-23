using DG.Tweening;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public enum EMovementMode
{
    Lerp,
    TPBefore,
    TPAfter
}

public class GMoveAction : GAction
{
    public static Action<GPawn, bool> OnMoveEvent;
    
    [Tooltip("Maximum number of cells the pawn can move")]
    [SerializeField, Min(0), HideIf("_useAttribute")]
    public int _maxMoveDistance = 2;
    
    [Tooltip("Speed of the movement")]
    [SerializeField]
    public float _speed = 4f;
    
    [SerializeField]
    protected AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [FormerlySerializedAs("_previsualisationCurveData")]
    [SerializeField]
    GActionPrevisualisationCurveData _previsuCurveData;
    
    [FormerlySerializedAs("animationName")]
    public string moveAnimationName = GPawn.MoveAnimationName;
    
    [SerializeField]
    public bool IsAnimationDriven = false;

    [SerializeField]
    public float OutAnimBlendTime = .01f;
    
    [SerializeField]
    public EMovementMode MovementMode = EMovementMode.Lerp;
    
    private ETileType[] _walkingTileType;
    private ETileType[] _endMovementTileType;
    
    private EHexDirection[] _path = new EHexDirection[] { };
    private Vector3[] _wayPoints = new Vector3[] { };
    float _progress = 0;
    int _currentWayPoint = 0;
    
    private bool _animationComplete = false;

    private bool _isWaitingForAnimation = false;
    private int _animationStateHash = 0;
    
    private Animator _animator;
    bool _isReaction = false;
    
    int _EquipementPickUpIndex = -1; // No Equipement to Picked Up
    
    public GMoveAction(){}
    
    public GMoveAction(GPawn inLinkedPawn, GCell inTargetCell, Action inOnActionStarted = null, Action inOnActionFinished = null) : base(inLinkedPawn, inTargetCell, inOnActionStarted, inOnActionFinished)
    {
        _isReaction = true;
    }

    public void OverrideTileType(ETileType[] inWalkingTileType, ETileType[] inEndMovementTileType)
    {
        _walkingTileType = inWalkingTileType;
        _endMovementTileType = inEndMovementTileType;
    }

    public override void InitAction(GPawn inLinkedPawn)
    {
        base.InitAction(inLinkedPawn);
        _walkingTileType = inLinkedPawn.data._walkingTileType;
        _endMovementTileType = inLinkedPawn.data._endMovementTileType;
    }

    public override List<GCell> Previsualisation(in GActionContext previsuContext)
    {
        GGridManager.Instance.GenerateStepMap(linkedPawn.GetCell(), _walkingTileType);
        _path = GGridManager.Instance.GetPath(linkedPawn.GetCell() ,targetCell, _endMovementTileType,false);
        if (_path == null || _path.Length == 0 || _path.Length > GetAttributeOrBaseValue(_maxMoveDistance, EAttributeType.MoveDistance)) return null;
        
        GCell cell = linkedPawn.GetCell();
        
        List<GCell> previewCells = new List<GCell>();
        previewCells.Add(linkedPawn.GetCell());
        GCell previousCell = linkedPawn.GetCell();
        foreach (var direction in _path)
        {
            GCell currentCell = previousCell.GetNeighbor(direction);
            previewCells.Add(currentCell);
            previousCell = currentCell;
        }
        
        AddPrevisualisationCurve(previsuContext, previewCells.Select(cell => cell.transform.position).ToArray(), _previsuCurveData);
        return previewCells;
    }

    public override void PreProcess(GActionContext context = null)
    {
        // TODO check player and do not generate step map if player ?
        base.PreProcess(context);
        GGridManager.Instance.GenerateStepMap(linkedPawn.GetCell(), _walkingTileType);
        _path = GGridManager.Instance.GetPath(linkedPawn.GetCell() ,targetCell, _endMovementTileType,false);
        if (_path == null || _path.Length == 0 || _path.Length > GetAttributeOrBaseValue(_maxMoveDistance, EAttributeType.MoveDistance)) return;
        
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
        targetCell.SetGridObject(linkedPawn, false);
    }

    public override void Start_Action()
    {
        base.Start_Action();
        _progress = 0;
        _currentWayPoint = 0;
        _animationComplete = false;
        
        if (targetCell.GetTileType == ETileType.Hole)
        {
            RuntimeManager.PlayOneShotAttached("event:/Pawn/Fall", linkedPawn.gameObject);
            moveAnimationName = GPawn.PushedIntoHoleAnimationName;
            
            MovementMode = EMovementMode.TPAfter;
            IsAnimationDriven = true;
            OutAnimBlendTime = 0;
        }

        if (moveAnimationName == GPawn.PushedStartAnimationName || moveAnimationName == GPawn.PushedIntoHoleAnimationName)
        {
            Vector3 lookAtPosition = targetCell.transform.position - (2 * (targetCell.transform.position - linkedPawn.transform.position));
            lookAtPosition.y = linkedPawn.transform.position.y;
            linkedPawn.transform.DOLookAt(lookAtPosition, 0.1f).SetEase(Ease.OutBack);
            //linkedPawn.transform.LookAt(lookAtPosition);
        }
        
        if (MovementMode == EMovementMode.TPBefore)
        {
            linkedPawn.transform.position = targetCell.transform.position;
        }

        
        linkedPawn.visuals.SetAnimationState(moveAnimationName, 0.01f);
        _animator = linkedPawn.visuals.GetAnimator();
        
        if (IsAnimationDriven)
        {
            _isWaitingForAnimation = true;
            _animationStateHash = Animator.StringToHash(moveAnimationName);
        }
        
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        _progress += delta * _speed;

        if (IsAnimationDriven && _animator)
        {
            if (_isWaitingForAnimation)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.shortNameHash == _animationStateHash && stateInfo.normalizedTime >= .95)
                {
                    _animationComplete = true;
                    _isWaitingForAnimation = false;
                }
            }
        }
        
        float animatedProgress = _speedCurve.Evaluate(_progress / _wayPoints.Length) * _wayPoints.Length;


        if (MovementMode == EMovementMode.Lerp)
        {
            if (animatedProgress > _currentWayPoint)
            {
                if (_EquipementPickUpIndex == _currentWayPoint)
                {
                    linkedPawn.GiveEquipement(linkedPawn.equipment, true, true);
                }
                _currentWayPoint++;

                if (_wayPoints.Length > 0 && moveAnimationName != GPawn.PushedStartAnimationName &&
                    moveAnimationName != GPawn.PushedIntoHoleAnimationName)
                {
                    Vector3 lookAtPosition = _wayPoints[Mathf.Min(_currentWayPoint + 1, _wayPoints.Length - 1)];
                    lookAtPosition.y = linkedPawn.transform.position.y;
                    linkedPawn.transform.LookAt(lookAtPosition);
                }
            }

            if (!IsAnimationDriven && animatedProgress > _wayPoints.Length - 1 || IsAnimationDriven && _animationComplete) 
            {
                End_Action();
                return;
            }
            
            int id = Mathf.FloorToInt(animatedProgress);
            linkedPawn.transform.position = Vector3.Lerp(_wayPoints[id], _wayPoints[id + 1], Mathf.Repeat(animatedProgress, 1));
        }
        else if (!IsAnimationDriven && animatedProgress > _wayPoints.Length - 1 || IsAnimationDriven && _animationComplete) 
        {
            End_Action();
            return;
        }
    }

    public override void End_Action()
    {
        linkedPawn.transform.position = targetCell.transform.position;
        if (!linkedPawn.IsAlive)
        {
            if (targetCell.GetTileType == ETileType.Hole)
            {
                RuntimeManager.PlayOneShotAttached("event:/Pawn/Enemy/Drown", linkedPawn.gameObject);
            }
            else
            {
                linkedPawn.Kill(GPawn.EDeathType.Pushed);
            }
        }
        else if(targetCell.GetTileType == ETileType.Hole)
        {
            linkedPawn.visuals.SetAnimationParameter(GPawn.AnimParam_IsInHole, true);
        }

        if (moveAnimationName == GPawn.PushedStartAnimationName && linkedPawn.IsAlive)
        {
            linkedPawn.visuals.SetAnimationState(GPawn.PushedEndAnimationName);
        }
        else if (moveAnimationName == GPawn.PushedStartAnimationName && !linkedPawn.IsAlive)
        {
            
        }
        else if(linkedPawn.IsAlive)
        {
            if (targetCell.GetTileType == ETileType.Hole)
            {
                linkedPawn.visuals.SetAnimationState("Idle_InHole", OutAnimBlendTime);
            }
            else
            {
                linkedPawn.visuals.SetAnimationState(GPawn.IdleAnimationName, OutAnimBlendTime);
            }
        }
        OnMoveEvent?.Invoke(linkedPawn, _isReaction);
        
        base.End_Action();
    }

    public override GHexCoordinate[] GetValidCells()
    {
        GGridManager.Instance.GenerateStepMap(linkedPawn.GetCell(), !linkedPawn.data.isPlayer);
        Dictionary<Vector2Int, int> stepMap = GGridManager.Instance._stepMap;
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();

        foreach (var step in stepMap)
        {
            GHexCoordinate coordinate = GHexCoordinate.FrommOffsetCoordinate(step.Key.x, step.Key.y);
            GCell cell = GGridManager.Instance.GetCell(coordinate);
            if (!cell || !cell.IsWalkable(false, !linkedPawn.data.isPlayer) || step.Value > GetAttributeOrBaseValue(_maxMoveDistance, EAttributeType.MoveDistance)) continue;
            if (GGridManager.Instance.GetPath(linkedPawn.GetCell(), cell).Length <= 0) continue;
            newValidCells.Add(coordinate);
        }
        
        return validCells = newValidCells.ToArray();
    }

    public override EZoneActionType GetHighlightActionType() => EZoneActionType.Move;

    public override GAction CloneAction()
    {
        GMoveAction moveAction = base.CloneAction() as GMoveAction;
        moveAction._maxMoveDistance = _maxMoveDistance;
        moveAction._speed = _speed;
        moveAction._speedCurve = _speedCurve;
        moveAction._walkingTileType = _walkingTileType;
        moveAction._endMovementTileType = _endMovementTileType;
        moveAction._isReaction = _isReaction;
        return moveAction;
    }
}