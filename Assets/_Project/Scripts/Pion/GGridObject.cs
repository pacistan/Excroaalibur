using Sirenix.OdinInspector;
using System;
using UnityEngine;

[SelectionBase]
public class GGridObject : MonoBehaviour
{
    [ReadOnly, FoldoutGroup("Persistant Data")]
    public GHexCoordinate coordinate;
    
    [ReadOnly, FoldoutGroup("Persistant Data")]
    protected GCell _currentCell;
    
    public virtual void SetCell(GHexCoordinate newCoordinate)
    {
        SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public virtual void SetCell(GCell newCell)
    {
        if (_currentCell && this is GPawn) _currentCell.ownedPawn = null;
        _currentCell = newCell;
        if (!newCell) return;
        coordinate = newCell._hexCoordinates;
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
