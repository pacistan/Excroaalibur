using Sirenix.OdinInspector;
using System;
using UnityEngine;

[SelectionBase]
public class GGridObject : MonoBehaviour
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
        if (currentCell) currentCell.ownedPawn = null;
        if (!newCell) return;
        currentCell = newCell;
        coordinate = newCell._hexCoordinates;
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
