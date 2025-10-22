using Sirenix.OdinInspector;
using System;
using UnityEngine;

[SelectionBase]
public class GGridObject : SerializedMonoBehaviour
{
    [ReadOnly, FoldoutGroup("Persistant Data")]
    public GHexCoordinate coordinate;
    
    [SerializeField, ReadOnly, FoldoutGroup("Persistant Data")]
    protected GCell _currentCell;
    
    public bool isMarkedForDestruction { get; set; } = false;
    
    public virtual void SetCell(GHexCoordinate newCoordinate)
    {
        SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public virtual void SetCell(GCell newCell)
    {
        if (_currentCell && _currentCell != newCell && _currentCell.gridObject == this) _currentCell.gridObject = null;
        _currentCell = newCell;
        if (!newCell) return;
        coordinate = newCell.hexCoordinates;
    }

    public virtual GCell GetCell()
    {
        return _currentCell;
    }
    
    void OnEnable()
    {
        GGridObjectRegistry.Instance.Register(this);
    }

    void OnDisable()
    {
        GGridObjectRegistry.Instance.Unregister(this);
    }
}
