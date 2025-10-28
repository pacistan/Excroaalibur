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

    [SerializeField, Tooltip("Visual GameObject to copy for caching purposes")]
    public GameObject visualToCopy;
    
    public bool isMarkedForDestruction { get; set; } = false;
    
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
    
    
    public virtual void SetCell(GHexCoordinate newCoordinate)
    {
        SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public virtual void SetCell(GCell newCell)
    {
        if (_currentCell && _currentCell != newCell && _currentCell.gridObject == this) _currentCell.SetGridObject(null);
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
