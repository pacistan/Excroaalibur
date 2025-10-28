using Sirenix.OdinInspector;
using UnityEngine;

public class GEquipment : GGridObject
{
    [SerializeField, ReadOnly, BoxGroup("Important Info")]
    public GPawn owner;

    [FoldoutGroup("Other", false)]
    [field: SerializeField, FoldoutGroup("Other/Components")]
    public GEquipmentVisuals visuals { get; private set; }
    
    public override GCell GetCell()
    {
        return owner ? owner.GetCell() : _currentCell;
    }
    
    public void SetOwner(GPawn newOwner)
    {
        if (GetCell() && GetCell().GetGridObject<GEquipment>())
        {
            GetCell().SetGridObject(null);
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
        if(owner)
            owner.ReleaseEquipement(false);
    }

    public override void SetCell(GHexCoordinate coordinate)
    {
        base.SetCell(coordinate);
    }
}