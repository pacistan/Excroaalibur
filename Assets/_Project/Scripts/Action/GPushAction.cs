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
    GPawn _targetPawn;
    GCell _linkedEndCell;
    Vector3 _linkedStartPosition;
    GCell _targetEndCell;
    Vector3 _targetStartPosition;
    
    bool _inflictDamage = false;
    bool _kill = false;

    
    public override void PreProcess()
    {
        _direction = linkedPawn.coordinate.GetLineDirection(targetCell._hexCoordinates);
        _targetPawn = targetCell.GetPawn();
        if (!_targetPawn) return;

        GCell pathCell = _targetPawn.currentCell;
        List<GCell> pathCells = new List<GCell>();
        
        
        for (int i = 0; i < _pushDistance; i++)
        {
            if (_targetPawn is GAltar) break;
            
            GCell neighbor = pathCell.GetNeighbor(_direction);
            
            if (!neighbor || neighbor.GetTileType == ETileType.Wall || neighbor.GetPawn())
            {
                _inflictDamage = true;
                break;
            }

            pathCells.Add(pathCell);
            pathCell = neighbor;
            if (neighbor.GetTileType == ETileType.Hole)
            {
                _kill = true;
                break;
            }
        }
        int id = Mathf.Min(pathCells.Count, _followDistance) - 1;
        if (id < 0 || id >= pathCells.Count) id = 0;
        if (pathCells.IsNullOrEmpty()) id = -1;
        _linkedEndCell = id < 0 ? linkedPawn.currentCell : pathCells[id];
        _linkedStartPosition = linkedPawn.transform.position;
        _targetEndCell = pathCell;
        _targetStartPosition = _targetPawn.transform.position;
    }

    public override void Start_Action()
    {
        base.Start_Action();
        
        // Release Equipment held onto cell if the pawn is leaving the cell
        if (_linkedEndCell != linkedPawn.currentCell && _targetPawn && _targetPawn.equipment && !(_targetPawn is GAltar))
        {
            GEquipment equipment = _targetPawn.equipment;
            _targetPawn.Release();
            _targetPawn.currentCell.Posess(equipment);
        }
        // Give Item to the unit that is pushing if there is a wall behind pawn that is pushed.
        /*else if (_linkedEndCell == linkedPawn.currentCell && _targetPawn && _targetPawn.GetEquipment() &&
                 !(_targetPawn is GAltar))
        {
            GEquipment equipment = _targetPawn.equipment;
            _targetPawn.Release();
            linkedPawn.Posess(equipment);
        }*/
        
        if (_inflictDamage)
        {
            _targetPawn.TakeDamage(_damage);
            _targetPawn.Stun(_stun);
        }
        
        
        if (_targetPawn is GAltar && _targetPawn.equipment && _targetPawn.equipment is GCrown)
        {
            GEquipment equipment = _targetPawn.equipment;
            _targetPawn.Release(); 
            linkedPawn.Posess(equipment);
        }
        
        _targetPawn.SetCell(_targetEndCell);
        linkedPawn.SetCell(_linkedEndCell);

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
        _targetPawn.transform.position = Vector3.Lerp(_targetStartPosition, _targetEndCell.transform.position, _progress);
    }

    public override void End_Action()
    {
        base.End_Action();
        linkedPawn.transform.position = _linkedEndCell.transform.position;
        _targetPawn.transform.position = _targetEndCell.transform.position;
        
        GEquipment equipment = _linkedEndCell._equipment;
        if (equipment &&  !(_targetPawn is GAltar))
        {
            _linkedEndCell.ReleaseEquipement();
            linkedPawn.Posess(equipment);
        }
        
        if (_kill && !_targetPawn.isPlayer)
            _targetPawn.Kill();
        else if (_kill && _targetPawn.isPlayer)
        {
            _targetPawn.Stun(1);
            
        }
    }

    public override GHexCoordinate[] GetValidCells()
    {
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();

        
        
        foreach (GCell cell in linkedPawn.currentCell._neighbors)
        {
            if (!cell || !cell.GetPawn() || cell.GetPawn() == linkedPawn) continue;
            
            newValidCells.Add(cell._hexCoordinates);
        }
        
        return validCells = newValidCells.ToArray();
    }
}