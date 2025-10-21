using DG.Tweening;
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
    bool _KillTarget = false;
    
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
        
        GPawn targetPawn = targetCell.GetGridObject<GPawn>();
        
        if (targetPawn && targetPawn.isPlayer)
        {
            _playerCatch = true;
            
            linkedPawn.ReleaseEquipement(false);
            targetPawn.GiveEquipement(_crown, false, false);
        } 
        else if (targetPawn && !targetPawn.isPlayer)
        {
            _impactReaction = targetPawn.GetReaction(this);
            if (_impactReaction != null)
            {
                var reactionContext = new GActionContext();
                reactionContext.Set("direction", direction);
                reactionContext.Set("damage", _crown.damage);
                _impactReaction.linkedPawn = targetPawn;
                GTurnBaseManager.Instance.PreProcessReaction(_impactReaction, reactionContext);
            }
            if (targetPawn.hp <= 0) _KillTarget = true;
        }
        
        if (_KillTarget) return;
        
        if (!_playerCatch)
        {
            linkedPawn.ReleaseEquipement(false);
            if (targetCell.GetNeighbor(direction.Opposite()).IsWalkable())
                targetCell.GetNeighbor(direction.Opposite()).gridObject = _crown;
            else if (targetPawn) // can give to non-player target if cell in front is not Available
                targetPawn.GiveEquipement(_crown, false, false);
        }
    }

    public override void Start_Action()
    {
        base.Start_Action();
        
        _startPos  = linkedPawn.currentCell.transform.position;
        _hitPos    = targetCell.transform.position;
        _returnPos = _startPos;
        
        var direction = linkedPawn.coordinate.GetLineDirection(targetCell.hexCoordinates);
        var backCell  = targetCell.GetNeighbor(direction.Opposite());
        bool canLandBehind = (backCell && backCell.IsWalkable());
        _landingPos = (_playerCatch || _KillTarget || !canLandBehind) ? _hitPos : backCell.transform.position;

        float outDur   = Vector3.Distance(_startPos, _hitPos)   / Mathf.Max(0.01f, _crownSpeed);
        float backDur  = Vector3.Distance(_hitPos, _returnPos)  / Mathf.Max(0.01f, _crownSpeed);
        float landDur  = Vector3.Distance(_hitPos, _landingPos) / Mathf.Max(0.01f, _crownSpeed);

        _seq = DOTween.Sequence()
            .SetUpdate(UpdateType.Manual, false); // Manual update mode

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
        });

        if (_KillTarget)
        {
            // Sequence Return to Owner
            _seq.Append(_crown.transform.DOMove(_returnPos, backDur).SetEase(Ease.InQuint));
        }
        else if (_playerCatch)
        {
           // Player Catch - stay at hit position 
        }
        else if (_landingPos != _hitPos)
        {
            // Sequence Landing front of Target
            _seq.Append(_crown.transform.DOMove(_landingPos, landDur).SetEase(Ease.InSine));
        }
        
        _seq.OnComplete(() =>
        {
            if (_KillTarget)          _crown.transform.position = _returnPos;
            else if (_playerCatch)    _crown.transform.position = _hitPos;
            else                      _crown.transform.position = _landingPos;

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
        GCell startCell = linkedPawn.currentCell;
        
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
}