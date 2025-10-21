using Sirenix.OdinInspector;
using UnityEngine;

public class GEquipment : GGridObject
{
    [SerializeField, ReadOnly]
    public GPawn owner;

    public override GCell GetCell()
    {
        return owner ? owner.GetCell() : _currentCell;
    }
    
    public void SetOwner(GPawn newOwner)
    {
        if (GetCell() && GetCell().GetGridObject<GEquipment>())
        {
            GetCell().gridObject = null;
        }
        owner = newOwner;
    }

    public void OnReleased()
    {
        owner = null;
    }

    public override void SetCell(GCell newCell)
    {
        base.SetCell(newCell);
        owner.ReleaseEquipement(false);
    }

    public override void SetCell(GHexCoordinate coordinate)
    {
        base.SetCell(coordinate);
    }
}
