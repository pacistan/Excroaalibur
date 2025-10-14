using Sirenix.OdinInspector;
using System;
using UnityEngine;

[SelectionBase]
public class GGridObject : MonoBehaviour
{
    [ReadOnly]
    public GHexCoordinate coordinate;
    
    [ReadOnly]
    public GCell currentCell;
    
    public virtual void SetCell(GHexCoordinate newCoordinate)
    {
        SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public virtual void SetCell(GCell newCell)
    {
        if (!newCell) return;
        if (currentCell) currentCell.SetPawn(null); 
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
