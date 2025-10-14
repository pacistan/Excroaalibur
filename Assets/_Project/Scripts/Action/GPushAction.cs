using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class GPushAction : GAction
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
        
        //TODO drop the crown
        for (int i = 0; i < _pushDistance; i++)
        {
            GCell neighbor = pathCell.GetNeighbor(_direction);
            if (!neighbor || neighbor.GetTileType == GCellData.ETileType.Wall || neighbor.GetPawn())
            {
                _inflictDamage = true;
                break;
            }
            pathCells.Add(pathCell);
            pathCell = neighbor;
            if (neighbor.GetTileType == GCellData.ETileType.Hole)
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
        _targetPawn.SetCell(_targetEndCell);
        linkedPawn.SetCell(_linkedEndCell);
        if(_inflictDamage)
            _targetPawn.TakeDamage(_damage, _stun);
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
        if (_kill && !_targetPawn.isPlayer)
            _targetPawn.Kill();
        else if (_kill && _targetPawn.isPlayer)
            _targetPawn.TakeDamage(0, 1);
    }

    public override GHexCoordinate[] GetValidCells()
    {
        List<GHexCoordinate> validCells = new List<GHexCoordinate>();

        foreach (GCell cell in linkedPawn.currentCell._neighbors)
        {
            if (!cell || !cell.GetPawn() || cell.GetPawn() == linkedPawn) continue;
            
            validCells.Add(cell._hexCoordinates);
        }
        
        return validCells.ToArray();
    }
}