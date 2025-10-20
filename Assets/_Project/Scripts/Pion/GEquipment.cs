using Sirenix.OdinInspector;
using UnityEngine;

public class GEquipment : GGridObject
{
    [field : SerializeField, ReadOnly]
    public GPawn owner { get; private set; }

    public GCell GetCell()
    {
        return owner ? owner.currentCell : currentCell;
    }
    
    public void ForceRelease()
    {
        owner.ReleaseEquipement();
    }
    
    public void SetOwner(GPawn newOwner)
    {
        if (currentCell && currentCell._equipment)
        {
            currentCell.ReleaseEquipement();
        }
        owner = newOwner;
        currentCell = owner.currentCell;
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
