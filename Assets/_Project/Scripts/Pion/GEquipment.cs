using Sirenix.OdinInspector;
using UnityEngine;

public class GEquipment : GGridObject
{
    [SerializeField, ReadOnly]
    public GPawn owner;

    public GCell GetCell()
    {
        return owner ? owner.currentCell : currentCell;
    }
    
    public void ForceRelease()
    {
        owner.ReleaseEquipement(false);
    }

    public override void SetCell(GHexCoordinate coordinate)
    {
        base.SetCell(coordinate);
    }
}
