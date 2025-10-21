using Sirenix.OdinInspector;
using System;
using UnityEngine;

[SelectionBase]
public class GGridObject : SerializedMonoBehaviour
{
    [ReadOnly, FoldoutGroup("Persistant Data")]
    public GHexCoordinate coordinate;
    
    [ReadOnly, FoldoutGroup("Persistant Data")]
    public GCell currentCell;
    
    public virtual void SetCell(GHexCoordinate newCoordinate)
    {
        SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public virtual void SetCell(GCell newCell)
    {
        if (!newCell) return;
        if (currentCell && currentCell != newCell) currentCell.gridObject = null;
        currentCell = newCell;
        coordinate = newCell.hexCoordinates;
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
