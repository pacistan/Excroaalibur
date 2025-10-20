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
    private int _damage = 3;
    [SerializeField]
    private int _damageEnd = 5;
    
    GCrown _crown;
    Dictionary<int, GPawn> _toDamage = new Dictionary<int, GPawn>();
    GPawn _toPush;
    GCell _pushTarget;
    int _distance;
    float _progress;
    GReaction _reaction;
    
    public override void PreProcess(GActionContext context = null)
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
            if (neighbor.ownedPawn && !neighbor.ownedPawn.isPlayer && i < _distance) _toDamage.Add(i, neighbor.ownedPawn);
            pathCell = neighbor;
        }
        
        GPawn targetPawn = targetCell.ownedPawn;
        if (targetPawn)
        {
            targetPawn.TakeDamage(_damageEnd);
            
            if (!targetPawn.isPlayer && targetPawn.IsAlive)
            {
                _reaction = targetPawn.GetReaction(this);
                if (_reaction != null)
                {
                    GActionContext pushContext = new GActionContext();
                    pushContext.Set("direction", direction);
                    pushContext.Set("distance", 1);
                    _reaction.instigatorCell = linkedPawn.currentCell;
                    _reaction.instigatorPawn = linkedPawn;
                    _reaction.linkedPawn = targetPawn;
                    GTurnBaseManager.Instance.TryPlayReaction(_reaction, pushContext);
                }
            }
            
        }
        linkedPawn.ReleaseEquipement(false);
        if (targetCell.ownedPawn)
        {
            targetCell.ownedPawn.GiveEquipement(_crown);
        }
        else
        {
            targetCell.GiveEquipement(_crown);
        }
    }

    public override void Start_Action()
    {
        base.Start_Action();
        _crown.transform.DOMove(_crown.transform.position, 0.5f).From(linkedPawn.currentCell.transform.position).SetEase(Ease.OutQuint);
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
        if (id >= _distance && _reaction != null)
        {
            GTurnBaseManager.Instance.TryStartReaction(_reaction);
            _reaction = null;
        }
    }

    public override void End_Action()
    {
        base.End_Action();
        foreach (KeyValuePair<int, GPawn> pair in _toDamage)
            pair.Value.TakeDamage(_damage);
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

    public override GAction CloneAction()
    {
        GThrowAction clonedAction = base.CloneAction() as GThrowAction;
        clonedAction._damageEnd = _damageEnd;
        clonedAction._damage = _damage;
        clonedAction._maxThrowDistance = _maxThrowDistance;
        return clonedAction;
    }
}