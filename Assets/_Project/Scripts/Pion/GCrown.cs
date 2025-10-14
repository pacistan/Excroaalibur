using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class GCrown : GEquipment
{

}

public class GEquipment : GGridObject
{
    [field : SerializeField, ReadOnly]
    public GPawn owner { get; private set; }

    public void SetOwner(GPawn newOwner)
    {
        owner = newOwner;
    }

    public void OnReleased()
    {
        owner = null;
    }

    public override void SetCell(GCell newCell)
    {
        base.SetCell(newCell);
        newCell.SetEquipment(this);
    }

    public override void SetCell(GHexCoordinate coordinate)
    {
        base.SetCell(coordinate);
    }
}