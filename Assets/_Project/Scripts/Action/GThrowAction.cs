using DG.Tweening;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GThrowAction : GAction
{
    [SerializeField, Min(0)]
    private int _maxThrowDistance = 10;
    [SerializeField]
    private int _damage = 1;
    
    GCrown _crown;
    Dictionary<int, GPawn> _toDamage = new Dictionary<int, GPawn>();
    GPawn _toPush;
    GCell _pushTarget;
    int _distance;
    float _progress;

    public override void PreProcess()
    {
        if (!linkedPawn.equipment || linkedPawn.equipment is not GCrown) return;
        _crown = (GCrown)linkedPawn.equipment;
        
        EHexDirection direction = linkedPawn.coordinate.GetLineDirection(base.targetCell._hexCoordinates);
        _distance = linkedPawn.coordinate.DistanceTo(base.targetCell._hexCoordinates);

        GCell pathCell = linkedPawn.currentCell;
        
        for (int i = 1; i <= _distance; i++)
        {
            GCell neighbor = pathCell.GetNeighbor(direction);
            if (neighbor == null) continue;
            if (neighbor.GetPawn() && !neighbor.GetPawn().isPlayer) _toDamage.Add(i, neighbor.GetPawn());
            pathCell = neighbor;
        }

        GPawn targetPawn = targetCell.GetPawn();
        if (targetPawn && !targetPawn.isPlayer)
        {
            _pushTarget = targetCell.GetNeighbor(direction);
            if (!_pushTarget || _pushTarget.GetPawn() || _pushTarget.GetTileType == ETileType.Wall)
                _pushTarget = null;
            else _toPush = targetPawn;
        }
    }

    public override void Start_Action()
    {
        base.Start_Action();
        linkedPawn.Release();
        if (_toPush && _pushTarget) _toPush.SetCell(_pushTarget);
        _crown.SetCell(targetCell);
        if (_crown.currentCell.GetPawn()) _crown.currentCell.GetPawn().Possess(_crown);
        
        Vector3 targetPos = _crown.transform.position;
        _crown.transform.DOMove(targetPos, 0.5f).From(linkedPawn.currentCell.transform.position).SetEase(Ease.OutQuint);
        DOTween.To(() => _progress, x => _progress = x, _distance, 0.5f).SetEase(Ease.OutQuint).onComplete = End_Action;
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        int id = Mathf.FloorToInt(_progress);
        if (_toDamage.ContainsKey(id))
        {
            _toDamage[id].TakeDamage(_damage);
            _toDamage.Remove(id);
        }
    }

    public override void End_Action()
    {
        base.End_Action();
        foreach (KeyValuePair<int, GPawn> pair in _toDamage)
            pair.Value.TakeDamage(_damage);
        
        if (_toPush && _pushTarget)
            _toPush.transform.DOMove(_pushTarget.transform.position, 0.25f).SetEase(Ease.OutQuint).onComplete = () => {if (_pushTarget.GetTileType == ETileType.Hole) _toPush.Fall();};
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
                    
                newValidCells.Add(cell._hexCoordinates);
            }
        }

        return validCells = newValidCells.ToArray();
    }
}