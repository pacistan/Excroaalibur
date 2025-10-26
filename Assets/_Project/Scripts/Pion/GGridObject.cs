using Sirenix.OdinInspector;
using System;
using UnityEngine;

[SelectionBase]
public class GGridObject : SerializedMonoBehaviour
{
    [SerializeField, ReadOnly]
    protected GCell _currentCell;
    
    [SerializeField, ReadOnly]
    public GHexCoordinate coordinate;
    
    [field: SerializeField, FoldoutGroup("Persistant Data")]
    public Sprite headerSprite { get; private set; }
    
    [field: SerializeField, FoldoutGroup("Persistant Data")]
    public Sprite cadreSprite { get; private set; }
    
    [field: SerializeField, FoldoutGroup("Persistant Data")]
    public Sprite headerObjectIconSprite { get; private set; }
    
    [field: SerializeField, FoldoutGroup("Persistant Data")]
    public Color pawnColor { get; private set; }
    
    [field: SerializeField, FoldoutGroup("Persistant Data")]
    public string headerName { get; private set; }
    
    [field: SerializeField, FoldoutGroup("Persistant Data")]
    public string className { get; private set; }
    
    
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
