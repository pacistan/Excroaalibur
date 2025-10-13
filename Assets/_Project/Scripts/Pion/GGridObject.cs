using Sirenix.OdinInspector;
using UnityEngine;

public class GGridObject : MonoBehaviour
{
    [ReadOnly]
    public GHexCoordinate coordinate;
    
    [ReadOnly]
    public GCell currentCell;
    
    //[SerializeField]
    //private int _hp = 3;
    
    public void SetCell(GHexCoordinate newCoordinate)
    {
        SetCell(GGridManager.Instance.GetCell(newCoordinate));
    }
    
    public void SetCell(GCell newCell)
    {
        if (!newCell) return;
        if (currentCell) currentCell.SetPawn(null); 
        currentCell = newCell;
        coordinate = newCell._hexCoordinates;
        currentCell.SetGridObject(this);
    }
}
