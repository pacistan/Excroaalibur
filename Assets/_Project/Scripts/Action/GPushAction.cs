using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class 
    GPushAction : GAction
{
    [SerializeField, Min(0)]
    private int _pushDistance = 2;
    [SerializeField, Min(0)]
    private int _followDistance = 1;
    [SerializeField]
    private int _stun = 2;
    [SerializeField]
    private int _damage = 1;
    
    EHexDirection _direction = EHexDirection.NE;
    float _progress = 0;
    GCell _linkedEndCell;
    Vector3 _linkedStartPosition;
    GPawn _targetPawn;
    GReaction _reaction;
    
    bool _inflictDamage = false;
    bool _fall = false;

    
    public override void PreProcess(GActionContext context = null)
    {
        if (linkedPawn.equipment || linkedPawn.equipment is GCrown) return;
        _direction = linkedPawn.coordinate.GetLineDirection(targetCell._hexCoordinates);
        _targetPawn = targetCell.GetPawn();
        if (!_targetPawn) return;

        _reaction = _targetPawn.GetReaction(this);
        if (_reaction != null)
        {
            GActionContext pushContext = new GActionContext();
            pushContext.Set("direction", _direction);
            pushContext.Set("distance", _pushDistance);
            pushContext.Set("damage", _damage);
            pushContext.Set("stun" , _stun);
            _reaction.instigatorCell = linkedPawn.currentCell;
            _reaction.instigatorPawn = linkedPawn;
            _reaction.linkedPawn = _targetPawn;
            GTurnBaseManager.Instance.TryPlayReaction(_reaction, pushContext);
        }

        GCell pathCell = linkedPawn.currentCell;
        
        for (int i = 0; i < _pushDistance; i++)
        {
            if (_targetPawn is GAltar) break;
            
            GCell neighbor = pathCell.GetNeighbor(_direction);
            
            if (!neighbor || neighbor.GetPawn() || neighbor.GetTileType == ETileType.Wall) break;

            pathCell = neighbor;
            if (neighbor.GetTileType == ETileType.Hole) break;
        }
        
        _linkedEndCell = pathCell;
        linkedPawn.SetCell(_linkedEndCell);
        _linkedStartPosition = linkedPawn.transform.position;
    }

    public override void Start_Action()
    {
        base.Start_Action();
        
        //TODO Check the need to replace the tryStartReaction with an event queue for all preprocessed callbacks
        GTurnBaseManager.Instance.TryStartReaction(_reaction);
        _progress = 0;
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        _progress += delta;
        if (_progress >= 1) 
        {
            End_Action();
            return;
        }
        linkedPawn.transform.position = Vector3.Lerp(_linkedStartPosition, _linkedEndCell.transform.position, _progress);
    }

    public override void End_Action()
    {
        base.End_Action();
        linkedPawn.transform.position = _linkedEndCell.transform.position;
        
        GEquipment equipment = _linkedEndCell._equipment;
        if (equipment &&  !(_targetPawn is GAltar))
        {
            _linkedEndCell.ReleaseEquipement();
            linkedPawn.Possess(equipment);
        }
        
    }

    public override GHexCoordinate[] GetValidCells()
    {
        if (linkedPawn.equipment || linkedPawn.equipment is GCrown)
            return validCells = new GHexCoordinate[]{};
        
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();
        
        foreach (GCell cell in linkedPawn.currentCell._neighbors)
        {
            if (!cell || !cell.GetPawn() || cell.GetPawn() == linkedPawn) continue;
            
            newValidCells.Add(cell._hexCoordinates);
        }
        
        return validCells = newValidCells.ToArray();
    }
}