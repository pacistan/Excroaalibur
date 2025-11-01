using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class GThrowAction : GAction
{
    [SerializeField, Min(0), Tooltip("Maximum distance the Crown can be thrown")]
    private int _maxThrowDistance = 10;
    
    [BoxGroup("Animation")]
    [SerializeField, Min(0), BoxGroup("Animation/Throw"), Tooltip("Height of the mid point of the curve when the crown is thrown")]
    private float _throwMidPointHeight = 0f;
    
    [SerializeField, BoxGroup("Animation/Throw"), Tooltip("Speed of the Crown when thrown")]
    float _throwCrownSpeed = 20f;
    
    [SerializeField, BoxGroup("Animation/Throw"), Tooltip("Animation curve, When the crown is thrown")]
    private AnimationCurve _throwSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [SerializeField, Min(0), BoxGroup("Animation/Pass"), Tooltip("Height of the mid point of the curve when the crown is passed to another pawn")]
    private float _passMidPointHeight = 0f;
    
    [SerializeField, BoxGroup("Animation/Pass"), Tooltip("Speed of the Crown when passed")]
    float _passCrownSpeed = 25f;
    
    [SerializeField, BoxGroup("Animation/Pass"), Tooltip("Animation curve, When the crown is thrown")]
    private AnimationCurve _passSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [SerializeField, Min(0), BoxGroup("Animation/Return"), Tooltip("Height of the mid point of the curve when the crown return")]
    private float _returnMidPointHeight = 2f;

    [SerializeField, BoxGroup("Animation/Return"), Tooltip("Speed of the Crown when returning")]
    float _impactDelayOnKill = 0f;

    [SerializeField, BoxGroup("Animation/Return"), Tooltip("Speed of the Crown when returning")]
    float _returnCrownSpeed = 10f;
    
    [SerializeField, BoxGroup("Animation/Return"), Tooltip("Animation curve, When the crown return to the owner")]
    private AnimationCurve _returnSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [SerializeField, Min(0), BoxGroup("Animation/Land"), Tooltip("Height of the mid point of the curve when the crown land")]
    private  float _landMidPointHeight = 2f;
    
    [SerializeField, BoxGroup("Animation/Land"), Tooltip("Speed of the Crown when land")]
    float _landCrownSpeed = 10f;
    
    [SerializeField, BoxGroup("Animation/Land"), Tooltip("Animation curve When the crown fall on the cell in front of the target")]
    private AnimationCurve _landSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField, BoxGroup("Animation/Throw"), Tooltip("Rotation Amount by each tile crossed while moving towards its initial target")]
    float _crownRotationAmountByTileOnThrowToTarget;

    [SerializeField, BoxGroup("Animation/Throw"), Tooltip("Rotation AnimationCruve while moving towards its initial target")]
    AnimationCurve _crownRotationAnimationCurveOnThrowToTarget;
    
    [SerializeField, BoxGroup("Animation/Throw"), Tooltip("Rotation Amount by each tile crossed while moving back toward the thrower")]
    float _crownRotationAmountByTileOnReturnToOrigin;

    [SerializeField, BoxGroup("Animation/Throw"), Tooltip("Rotation AnimationCruve while moving back toward the thrower")]
    AnimationCurve _crownRotationAnimationCurveOnThrowToOrigin;
    
    GCrown _crown;
    GAction _impactReaction;
    
    Sequence _seq;
    
    bool _playerCatch;
    bool _killTarget = false;
    
    GPawn _targetPawn;
    
    private Vector3 _startPos;
    private Vector3 _hitPos;
    private Vector3 _returnPos;
    private Vector3 _landingPos;
    
    private float _distance;
    private float _progress;

    EventInstance _throwSoundInstance;
    int _crownPower = 0;
    bool _isThrowAnimationOver;

    public override void OnSelectedAction()
    {
        base.OnSelectedAction();
        
        linkedPawn.visuals.SetAnimationParameter(GPawn.AnimParam_IsPreparedToThrow, true);
        if (validCells.Length == 0) return;
        foreach (var validCell in validCells)
        { 
            GPawn pawn =  GGridManager.Instance.GetCell(validCell).GetGridObject<GPawn>();
            if (!pawn) continue;
            pawn.RotateTowards(linkedPawn.transform.position, 0.5f);
            pawn.visuals.SetAnimationParameter(GPawn.AnimParam_IsPreparedToCatch, true);
        }
    }

    public override void OnUnselectedAction()
    {
        base.OnUnselectedAction();
        linkedPawn.visuals.SetAnimationParameter(GPawn.AnimParam_IsPreparedToThrow, false);
        foreach (var validCell in validCells)
            GGridManager.Instance.GetCell(validCell).GetGridObject<GPawn>()?.visuals.
                SetAnimationParameter(GPawn.AnimParam_IsPreparedToCatch, false);
    }

    public override List<GCell> Previsualisation(in GActionContext previsuContext)
    {
        if (!linkedPawn.equipment || linkedPawn.equipment is not GCrown) return null;
        _crown = (GCrown)linkedPawn.equipment;
        
        EHexDirection direction = linkedPawn.GetHexCoordinate().GetLineDirection(base.targetCell.hexCoordinates);
        _distance = linkedPawn.GetHexCoordinate().DistanceTo(base.targetCell.hexCoordinates);
        
        _targetPawn = targetCell.GetGridObject<GPawn>();
        List<GCell> previewCells = new List<GCell>();
        
        if (_targetPawn && _targetPawn.data.isPlayer)
        {
            previewCells.Add(targetCell);  // Target Cell
        } 
        else if (_targetPawn && !_targetPawn.data.isPlayer)
        {
            GAction impactReaction = _targetPawn.GetReaction(this);
            previsuContext.Set(GActionContext.DAMAGE_STRING, _crown.currentDamage);
            if (impactReaction != null)
            {
                previsuContext.Set(GActionContext.DIRECTION_STRING, direction);
                impactReaction.linkedPawn = _targetPawn;
                impactReaction.Previsualisation(in previsuContext);
            }
            
            if (_targetPawn.hp - previsuContext.Get<int>(GActionContext.DAMAGE_STRING) <= 0) // Kill 
            {
                previewCells.Add(linkedPawn.GetCell()); // Return to owner (Cell of the linked pawn)
            }
            else
            {
                GCell frontCell = targetCell.GetNeighbor(direction.Opposite());
                if (_targetPawn && frontCell.IsWalkable(true))
                {
                    previewCells.Add(frontCell); // Cell in front of target
                }
                else
                {
                    previewCells.Add(targetCell); // Target Cell
                }
            }
            
        }
        else
        {
            previewCells.Add(targetCell); // Target Cell
        }
        
        return previewCells;
    }
    
    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess(context);
        if (!linkedPawn.equipment || linkedPawn.equipment is not GCrown) return;
        _crown = (GCrown)linkedPawn.equipment;
        
        EHexDirection direction = linkedPawn.GetHexCoordinate().GetLineDirection(base.targetCell.hexCoordinates);
        _distance = linkedPawn.GetHexCoordinate().DistanceTo(base.targetCell.hexCoordinates);
        
        _targetPawn = targetCell.GetGridObject<GPawn>();
        
        if (_targetPawn && _targetPawn.data.isPlayer)
        {
            _playerCatch = true;
            
            linkedPawn.ReleaseEquipement(false);
            _targetPawn.GiveEquipement(_crown, false, false);
        } 
        else if (_targetPawn && !_targetPawn.data.isPlayer)
        {
            // TODO : Take damage here or in reaction ? 
            _targetPawn.TakeDamage(_crown.currentDamage);
            _impactReaction = _targetPawn.GetReaction(this);
            if (_impactReaction != null)
            {
                var reactionContext = new GActionContext();
                reactionContext.Set(GActionContext.DIRECTION_STRING, direction);
                reactionContext.Set(GActionContext.DAMAGE_STRING, _crown._baseDamage);
                _impactReaction.linkedPawn = _targetPawn;
                GTurnBaseManager.Instance.PreProcessReaction(_impactReaction, reactionContext);
            }
            if (_targetPawn.hp == 0) _killTarget = true;
        }
        _crownPower = Mathf.Clamp(_crown.currentDamage - 1, 0, 5);
        
        if (_killTarget)
        {
            _crown.ResetCrown();
            return;
        }
        
        if (!_playerCatch)
        {
            linkedPawn.ReleaseEquipement(false, true);
            GCell frontCell = targetCell.GetNeighbor(direction.Opposite());
            if (_targetPawn && frontCell.IsWalkable(true))
            {
                GPawn neighborPawn = frontCell.GetGridObject<GPawn>();
                if (neighborPawn)
                    neighborPawn.GiveEquipement(_crown, false, false);
                else
                    frontCell.SetGridObject(_crown, false);
            }
            else if (_targetPawn) // can give to non-player target if cell in front is not Available
                _targetPawn.GiveEquipement(_crown, false, false);
            else
                targetCell.SetGridObject(_crown, false);;
        }
        else
        {
            _crown.OnPass();
        }
    }

    public override void Start_Action()
    {
        base.Start_Action();
        
        Vector3 lookAtPosition = targetCell.transform.position;
        lookAtPosition.y = linkedPawn.transform.position.y;
        linkedPawn.transform.LookAt(lookAtPosition);

        string animName = GPawn.ThrowAnimationName;
        if(_targetPawn && _targetPawn.data.isPlayer)
            animName = _targetPawn.GetCell().hexCoordinates.DistanceTo(linkedPawn.GetCell().hexCoordinates) == 1
                ? GPawn.PassCloseAnimationName
                : GPawn.PassFarAnimationName;
        
        
        linkedPawn.OnAnimationThrow += OnAnimationThrowCallback;
        linkedPawn.visuals.SetAnimationState(animName);
        linkedPawn.StartCoroutine(StartReactionsCoroutine());
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        
        // Manually update the DOTween sequence
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying())
        {
            DOTween.ManualUpdate(delta, delta);
        }
        
    }

    public override void End_Action()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();
        _crown.transform.localRotation = Quaternion.identity;
        _seq = null;
        linkedPawn.visuals.SetAnimationState(GPawn.IdleAnimationName);
        base.End_Action();
    }

    public override GHexCoordinate[] GetValidCells()
    {
        if (!linkedPawn.equipment || linkedPawn.equipment is not GCrown)
            return validCells = new GHexCoordinate[]{};
        
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();
        GCell startCell = linkedPawn.GetCell();
        
        foreach (EHexDirection direction in Enum.GetValues(typeof(EHexDirection)))
        {
            GCell cell = startCell;
            for (int i = 0; i < _maxThrowDistance; i++)
            {
                cell = cell.GetNeighbor(direction);

                if (!cell || cell.GetTileType == ETileType.Wall) break;
                if (cell.GetTileType == ETileType.Hole) continue;
                if (cell.GetGridObject<GPawn>() is GAltar) continue;
                    
                newValidCells.Add(cell.hexCoordinates);
                if (cell.GetGridObject<GPawn>()) break;
            }
        }

        return validCells = newValidCells.ToArray();
    }

    public override ETileHighlightActionType GetHighlightActionType() => ETileHighlightActionType.Throw;

    public override GAction CloneAction()
    {
        GThrowAction clonedAction = base.CloneAction() as GThrowAction;
        clonedAction._maxThrowDistance = _maxThrowDistance;
        clonedAction._throwCrownSpeed = _throwCrownSpeed;
        clonedAction._throwMidPointHeight = _throwMidPointHeight;
        clonedAction._throwSpeedCurve = _throwSpeedCurve;
        clonedAction._passCrownSpeed = _passCrownSpeed;
        clonedAction._passMidPointHeight = _passMidPointHeight;
        clonedAction._passSpeedCurve = _passSpeedCurve;
        clonedAction._returnCrownSpeed = _returnCrownSpeed;
        clonedAction._returnMidPointHeight = _returnMidPointHeight;
        clonedAction._returnSpeedCurve = _returnSpeedCurve;
        clonedAction._landCrownSpeed = _landCrownSpeed;
        clonedAction._landMidPointHeight = _landMidPointHeight;
        clonedAction._landSpeedCurve = _landSpeedCurve;
        clonedAction._impactDelayOnKill = _impactDelayOnKill;
        clonedAction._crownRotationAmountByTileOnReturnToOrigin = _crownRotationAmountByTileOnReturnToOrigin;
        clonedAction._crownRotationAmountByTileOnThrowToTarget = _crownRotationAmountByTileOnThrowToTarget;
        return clonedAction;
    }
    
    IEnumerator StartReactionsCoroutine()
    {
        EventInstance pawnThrowSoundInstance = RuntimeManager.CreateInstance("event:/Pawn/Throw");
        pawnThrowSoundInstance.set3DAttributes(linkedPawn.gameObject.To3DAttributes());
        pawnThrowSoundInstance.setParameterByName("Power", _crownPower);
        pawnThrowSoundInstance.start();
        pawnThrowSoundInstance.release();
        
        _throwSoundInstance = RuntimeManager.CreateInstance("event:/Crown/Throw");
        _throwSoundInstance.set3DAttributes(_crown.gameObject.To3DAttributes());
        _throwSoundInstance.setParameterByName("Power", _crownPower);
        
        yield return new WaitUntil(() => _isThrowAnimationOver);
        
        linkedPawn.OnAnimationThrow -= OnAnimationThrowCallback;
        _throwSoundInstance.start();
        
        _crown.ResetTransformOwner();
        _crown.transform.DORotateQuaternion(Quaternion.identity, 0.2f);
        _startPos  = linkedPawn.equipmentParentTr.position;
        _hitPos    =  _targetPawn ? _targetPawn.equipmentParentTr.position : targetCell.equipmentSpawnPoint.position;
        _returnPos = _startPos;
        
        var direction = linkedPawn.GetHexCoordinate().GetLineDirection(targetCell.hexCoordinates);
        var frontCell  = targetCell.GetNeighbor(direction.Opposite());
        bool canLandInFrontOf = (frontCell && frontCell.IsWalkable(true));
        if (_playerCatch || _killTarget || !canLandInFrontOf)
        {
            _landingPos = _hitPos;
        }
        else
        {
            GPawn landPawn = frontCell.GetGridObject<GPawn>();
            if (landPawn)
            {
                _landingPos = landPawn.equipmentParentTr.position;
            }
            else
            {
                _landingPos = frontCell.transform.position;
            }
        }

        float outDur   = Vector3.Distance(_startPos, _hitPos)   / Mathf.Max(0.01f, _playerCatch ? _passCrownSpeed : _throwCrownSpeed);
        float backDur  = Vector3.Distance(_hitPos, _returnPos)  / Mathf.Max(0.01f, _returnCrownSpeed);
        float landDur  = Vector3.Distance(_hitPos, _landingPos) / Mathf.Max(0.01f, _landCrownSpeed);

        _seq = DOTween.Sequence()
            .SetUpdate(UpdateType.Manual, false); // Manual update mode
        
        Vector3 throwMidPoint = Vector3.Lerp(_startPos, _hitPos, 0.5f);
        throwMidPoint.y += _playerCatch ? _passMidPointHeight : _throwMidPointHeight;
        
        Vector3[] ThrowPath = new Vector3[] { _startPos, throwMidPoint , _hitPos };
        _seq.Append(_crown.transform.DOPath(ThrowPath, outDur, PathType.CatmullRom)
                .SetEase(_playerCatch ?_passSpeedCurve : _throwSpeedCurve)
                .SetOptions(false)
        );

        Vector3 angleAxisRotation = Vector3.right * _crownRotationAmountByTileOnThrowToTarget * ThrowPath.Length;
        
        _seq.Join(_crown.transform.DORotate(angleAxisRotation, outDur, RotateMode.LocalAxisAdd))
            .SetEase(_playerCatch ?_passSpeedCurve : _throwSpeedCurve);

        // Impact callback: Fire the reaction of the target pawn
        _seq.AppendCallback(() =>
        {
            if (_impactReaction != null)
            {
                GTurnBaseManager.Instance.TryStartReaction(_impactReaction);
                _impactReaction = null;
            }
            GHudManager.Instance.playMenu.SetCrownDamageText(_crown.currentDamage);
            if (_targetPawn)
            {
                _targetPawn.UpdateHpNumber();
                if (!_targetPawn.data.isPlayer)
                {
                    EventInstance hitEvent = RuntimeManager.CreateInstance("event:/Crown/Hit");
                    hitEvent.set3DAttributes(_crown.gameObject.To3DAttributes());
                    hitEvent.setParameterByName("Power", _crownPower);
                    hitEvent.start();
                    hitEvent.release();
                    if (_targetPawn.IsAlive)
                    {
                        _targetPawn.visuals.SetAnimationState(GPawn.HitAnimationName);
                    }
                }
                else
                {
                    _targetPawn.visuals.SetAnimationState(GPawn.CatchAnimationName);
                }
            }
            _throwSoundInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _throwSoundInstance.release();
        });

        if (_killTarget)
        {
            _seq.PrependInterval(_impactDelayOnKill);
            // Sequence Return to Owner
            _seq.AppendCallback(() =>
            {
                _targetPawn.Kill();
                RuntimeManager.PlayOneShotAttached("event:/Crown/Catch", _crown.gameObject);
            }); 
            
            Vector3 returnMidPoint = Vector3.Lerp(_hitPos, _returnPos, 0.5f);
            returnMidPoint.y += _returnMidPointHeight;
        
            Vector3[] returnPath = new Vector3[] { _hitPos, returnMidPoint, _returnPos};
            _seq.Append(_crown.transform.DOPath(returnPath, backDur, PathType.CatmullRom)
                .SetEase(_returnSpeedCurve)
                .SetOptions(false)
            );
            Vector3 angleAxisRotationReturn = Vector3.right * _crownRotationAmountByTileOnThrowToTarget * ThrowPath.Length;
        
            _seq.Join(_crown.transform.DORotate(angleAxisRotation, outDur, RotateMode.LocalAxisAdd))
                .SetEase(_playerCatch ?_passSpeedCurve : _throwSpeedCurve);
            _seq.AppendCallback(() => linkedPawn.GiveEquipement(_crown, true, true));
        }
        else if (_playerCatch)
        {
           // Player Catch - stay at hit position 
           _seq.AppendCallback(() =>
           {
               _targetPawn.GiveEquipement(_crown, true, true);
           }); 
        }
        else if (canLandInFrontOf && _targetPawn)
        {
            // Sequence Landing front of Target
            GPawn landPawn = frontCell.GetGridObject<GPawn>();
            _seq.AppendCallback(() =>
            {
                RuntimeManager.PlayOneShotAttached("event:/Crown/Fall", _crown.gameObject);
            }); 
            
            Vector3 midPoint = Vector3.Lerp(_hitPos, _landingPos, 0.5f);
            midPoint.y += _landMidPointHeight;
        
            Vector3[] path = new Vector3[] { _hitPos, midPoint, _landingPos};
            _seq.Append(_crown.transform.DOPath(path, landDur, PathType.CatmullRom)
                .SetEase(_landSpeedCurve)
                .SetOptions(false)
            );
            
            if (landPawn)
            {
                _seq.AppendCallback(() =>
                {
                    landPawn.GiveEquipement(_crown, true, true);
                });
            }
            else
            {
                _seq.AppendCallback(() =>
                {
                    frontCell.SetGridObject(_crown, true);
                });
            }
        }
        else if (_targetPawn)
        {
            _seq.AppendCallback(() =>
            {
                _targetPawn.GiveEquipement(_crown, true, true);
            }); 
        }
        else if (!_targetPawn)
        {
            _seq.AppendCallback(() =>
            {
                targetCell.SetGridObject(_crown, true);
                RuntimeManager.PlayOneShotAttached("event:/Crown/Fall", _crown.gameObject);
            });
        }
        
        _seq.OnComplete(() =>
        {
            //if (_killTarget)          _crown.transform.position = _returnPos;
            //else if (_playerCatch)    _crown.transform.position = _hitPos;
            //else if (_targetPawn)      _crown.transform.position = _landingPos;
            //else                      _crown.transform.position = _hitPos;

            End_Action();
        });
    }
    
    private void OnAnimationThrowCallback() 
        => _isThrowAnimationOver = true;
}