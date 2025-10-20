using Sirenix.OdinInspector;
using UnityEngine;

public class GEquipment : GGridObject
{
    [field : SerializeField, ReadOnly]
    public GPawn owner { get; private set; }

    public override GCell GetCell()
    {
        return owner ? owner.GetCell() : _currentCell;
    }
    
    public void ForceRelease()
    {
        owner.ReleaseEquipement();
    }
    
    public void SetOwner(GPawn newOwner)
    {
        if (GetCell() && GetCell()._equipment)
        {
            GetCell().ReleaseEquipement();
        }
        owner = newOwner;
        //GetCell() = owner.GetCell();
    }

    public void OnReleased()
    {
        owner = null;
    }

    public override void SetCell(GCell newCell)
    {
        base.SetCell(newCell);
        // newCell.GiveEquipement(this);
    }

    public override void SetCell(GHexCoordinate coordinate)
    {
        base.SetCell(coordinate);
    }
}
