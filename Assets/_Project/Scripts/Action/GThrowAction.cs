using DG.Tweening;
using FMODUnity;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GThrowAction : GAction
{
    [SerializeField, Min(0), Tooltip("Maximum distance the Crown can be thrown")]
    private int _maxThrowDistance = 10;
    
    [SerializeField, Tooltip("Speed of the Crown when thrown")]
    private float _crownSpeed = 10f;
    
    GCrown _crown;
    GAction _impactReaction;
    
    Sequence _seq;
    
    bool _playerCatch;
    bool _killTarget = false;
    
    // TODO : check because not the good way
    GPawn _targetPawn;
    
    private Vector3 _startPos;
    private Vector3 _hitPos;
    private Vector3 _returnPos;
    private Vector3 _landingPos;
    
    private float _distance;
    private float _progress;
    
    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess(context);
        if (!linkedPawn.equipment || linkedPawn.equipment is not GCrown) return;
        _crown = (GCrown)linkedPawn.equipment;
        
        EHexDirection direction = linkedPawn.coordinate.GetLineDirection(base.targetCell.hexCoordinates);
        _distance = linkedPawn.coordinate.DistanceTo(base.targetCell.hexCoordinates);
        
        _targetPawn = targetCell.GetGridObject<GPawn>();
        
        if (_targetPawn && _targetPawn.isPlayer)
        {
            _playerCatch = true;
            
            linkedPawn.ReleaseEquipement(false);
            _targetPawn.GiveEquipement(_crown, false, false);
        } 
        else if (_targetPawn && !_targetPawn.isPlayer)
        {
            // TODO : Take damage here or in reaction ? 
            _targetPawn.TakeDamage(_crown._currentDamage);
            _impactReaction = _targetPawn.GetReaction(this);
            if (_impactReaction != null)
            {
                var reactionContext = new GActionContext();
                reactionContext.Set("direction", direction);
                reactionContext.Set("damage", _crown._baseDamage);
                _impactReaction.linkedPawn = _targetPawn;
                GTurnBaseManager.Instance.PreProcessReaction(_impactReaction, reactionContext);
            }
            if (_targetPawn.hp == 0) _killTarget = true;
        }

        if (_killTarget)
        {
            _crown.ResetCrown();
            return;
        }
        
        if (!_playerCatch)
        {
            linkedPawn.ReleaseEquipement(false, true);
            if (_targetPawn && targetCell.GetNeighbor(direction.Opposite()).IsWalkable(true))
            {
                GPawn neighborPawn = targetCell.GetNeighbor(direction.Opposite()).GetGridObject<GPawn>();
                if (neighborPawn)
                {
                    neighborPawn.GiveEquipement(_crown, false, false);
                }
                else
                {
                    targetCell.GetNeighbor(direction.Opposite()).gridObject = _crown;
                }
            }
            else if (_targetPawn) // can give to non-player target if cell in front is not Available
                _targetPawn.GiveEquipement(_crown, false, false);
            else
                targetCell.gridObject = _crown;
        }
        else
        {
            _crown.OnPass();
        }
    }

    public override void Start_Action()
    {
        base.Start_Action();

        GPawn targetPawn = targetCell.GetGridObject<GPawn>();
        
        _startPos  = linkedPawn.equipmentParentTr.position;
        _hitPos    =  targetPawn ? targetPawn.equipmentParentTr.position : targetCell.transform.position;
        _returnPos = _startPos;
        
        var direction = linkedPawn.coordinate.GetLineDirection(targetCell.hexCoordinates);
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

        float outDur   = Vector3.Distance(_startPos, _hitPos)   / Mathf.Max(0.01f, _crownSpeed);
        float backDur  = Vector3.Distance(_hitPos, _returnPos)  / Mathf.Max(0.01f, _crownSpeed);
        float landDur  = Vector3.Distance(_hitPos, _landingPos) / Mathf.Max(0.01f, _crownSpeed);

        _seq = DOTween.Sequence()
            .SetUpdate(UpdateType.Manual, false); // Manual update mode

        
        RuntimeManager.PlayOneShotAttached("event:/Pawn/Throw", linkedPawn.gameObject);
        
        // Sequence to Target 
        _seq.Append(_crown.transform.DOMove(_hitPos, outDur).SetEase(Ease.OutQuint));

        // Impact callback: Fire the reaction of the target pawn
        _seq.AppendCallback(() =>
        {
            if (_impactReaction != null)
            {
                GTurnBaseManager.Instance.TryStartReaction(_impactReaction);
                _impactReaction = null;
            }
            _targetPawn.UpdateHpNumber();
            if (_targetPawn && !_targetPawn.isPlayer)
            {
                RuntimeManager.PlayOneShotAttached("event:/Crown/Hit", _crown.gameObject);
            }
        });

        if (_killTarget)
        {
            // Sequence Return to Owner
            _seq.AppendCallback(() =>
            {
                _targetPawn.Kill();
                RuntimeManager.PlayOneShotAttached("event:/Crown/Catch", _crown.gameObject);
            }); 
            _seq.Append(_crown.transform.DOMove(_returnPos, backDur).SetEase(Ease.InQuint));
        }
        else if (_playerCatch)
        {
           // Player Catch - stay at hit position 
           _seq.AppendCallback(() =>
           {
               _targetPawn.GiveEquipement(_crown, true, true);
           }); 
        }
        else if (canLandInFrontOf && targetPawn)
        {
            // Sequence Landing front of Target
            GPawn landPawn = frontCell.GetGridObject<GPawn>();
            _seq.AppendCallback(() =>
            {
                RuntimeManager.PlayOneShotAttached("event:/Crown/Fall", _crown.gameObject);
            }); 
            _seq.Append(_crown.transform.DOMove(_landingPos, landDur).SetEase(Ease.OutCubic));
            if (landPawn)
            {
                _seq.AppendCallback(() =>
                {
                    landPawn.GiveEquipement(_crown, true, true);
                });
            }
        }
        else if (targetPawn)
        {
            _seq.AppendCallback(() =>
            {
                _targetPawn.GiveEquipement(_crown, true, true);
            }); 
        }
        
        _seq.OnComplete(() =>
        {
            if (_killTarget)          _crown.transform.position = _returnPos;
            else if (_playerCatch)    _crown.transform.position = _hitPos;
            else if (targetPawn)      _crown.transform.position = _landingPos;
            else                      _crown.transform.position = _hitPos;

            End_Action();
        });
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        
        // Manually update the DOTween sequence
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying())
        {
            DOTween.ManualUpdate(delta, delta);
        }

        // Keep your old progress/reaction guard (safe if something changes mid-flight)
        _progress += delta * _crownSpeed;
        int id = Mathf.FloorToInt(_progress);
        if (id >= _distance && _impactReaction != null)
        {
            GTurnBaseManager.Instance.TryStartReaction(_impactReaction);
            _impactReaction = null;
        }
    }

    public override void End_Action()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();
        _seq = null;
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
                    
                newValidCells.Add(cell.hexCoordinates);
            }
        }

        return validCells = newValidCells.ToArray();
    }

    public override GAction CloneAction()
    {
        GThrowAction clonedAction = base.CloneAction() as GThrowAction;
        clonedAction._maxThrowDistance = _maxThrowDistance;
        clonedAction._crownSpeed = _crownSpeed;
        return clonedAction;
    }
}