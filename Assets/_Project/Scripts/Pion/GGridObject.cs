using Sirenix.OdinInspector;
using System;
using UnityEngine;

[SelectionBase]
public class GGridObject : SerializedMonoBehaviour
{
    [SerializeField, ReadOnly, BoxGroup("Important Info")]
    protected GCell _currentCell;

    [FoldoutGroup("Other", false)]
    [SerializeField, ReadOnly]
    public GHexCoordinate coordinate;
    
    public bool isMarkedForDestruction { get; set; } = false;

    [FoldoutGroup("Other", false)]
    [field: SerializeField, FoldoutGroup("Other/Persistant Data")]
    public Sprite headerSprite { get; private set; }

    [FoldoutGroup("Other", false)]
    [field: SerializeField, FoldoutGroup("Other/Persistant Data")]
    public Sprite cadreSprite { get; private set; }

    [FoldoutGroup("Other", false)] 
    [field: SerializeField, FoldoutGroup("Other/Persistant Data")]
    public Sprite headerObjectIconSprite { get; private set; }

    [FoldoutGroup("Other", false)]
    [field: SerializeField, FoldoutGroup("Other/Persistant Data")]
    public Color pawnColor { get; private set; }

    [FoldoutGroup("Other", false)]
    [field: SerializeField, FoldoutGroup("Other/Persistant Data")]
    public string headerName { get; private set; }

    [FoldoutGroup("Other", false)]
    [field: SerializeField, FoldoutGroup("Other/Persistant Data")]
    public string className { get; private set; }
    
    public virtual void TrySetCell(GHexCoordinate newCoordinate)
    {
        TrySetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    /** Need to be Call by the new Owner (Do Not Call this directly if you are not the new Owner */
    public virtual bool TrySetCell(GCell newCell)
    {
        if (_currentCell == newCell)  return true; // Already assigned to this cell
        
        ClearCell();
        if (_currentCell != null)
        {
             Debug.LogError("Trying to set cell on a GridObject that already has a cell assigned. Clear the current cell first.");
             return false;
        }
        
        _currentCell = newCell;
        if (_currentCell != null) 
            coordinate = _currentCell.hexCoordinates;
        
        return true;
    }
    
    /** Clear the Current Cell reference from both sides **/
    public virtual void ClearCell()
    {
        if (_currentCell && _currentCell.gridObject == this) 
            _currentCell.SetGridObject(null);
        
        _currentCell = null;
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
