using Sirenix.OdinInspector;
using UnityEngine;

public class GEquipment : GGridObject
{
    [SerializeField, ReadOnly, BoxGroup("Important Info")]
    public GPawn owner;

    [FoldoutGroup("Other", false)]
    [field: SerializeField, FoldoutGroup("Other/Components")]
    public GEquipmentVisuals visuals { get; private set; }
    
    [ReadOnly, BoxGroup("Important Info")]
    public GPawn transformOwner;
    
    public override GCell GetCell()
    {
        return owner ? owner.GetCell() : _currentCell;
    }

    public void ResetTransformOwner()
    {
        if (transformOwner != null)
        {
            transformOwner.OnReleaseTransformEquipment();
            transformOwner = null;
            transform.SetParent(null, true);
            transform.localScale = Vector3.one;
        }
    }
    
    public override bool TrySetCell(GCell newCell)
    {
        if (!base.TrySetCell(newCell)) return false;
        
        if(owner != null)
            owner.ReleaseEquipement(false);
        
        return true;
    }

    public override void TrySetCell(GHexCoordinate coordinate)
    {
        base.TrySetCell(coordinate);
    }
}