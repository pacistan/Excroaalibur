using System.Collections.Generic;
using UnityEngine;

public class GPunchAction : GAction
{ 
    [SerializeField]
    private int _damage = 1;
    [SerializeField]
    private int _stun = 1;
    float _progress = 0;
    
    
    public override void PreProcess(GActionContext context = null)
    {
        if (linkedPawn.equipment || linkedPawn.equipment is GCrown) return;
        if (!targetCell || targetCell.GetGridObject<GPawn>() || targetCell.GetGridObject<GPawn>() == linkedPawn) return;
        //validate
    }

    public override void Start_Action()
    {
        base.Start_Action();
        targetCell.GetGridObject<GPawn>().TakeDamage(_damage);
        targetCell.GetGridObject<GPawn>().Stun(_stun);
        _progress = 0;
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        _progress += delta;
        if (_progress >= .5) 
        {
            End_Action();
            return;
        }
    }

    public override void End_Action()
    {
        base.End_Action();
    }

    public override GHexCoordinate[] GetValidCells()
    {
        if (linkedPawn.equipment || linkedPawn.equipment is not GCrown)
            return validCells = new GHexCoordinate[]{};
        
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();

        foreach (var cell in linkedPawn.currentCell.neighbors)
        {
            if (!cell || !cell.GetGridObject<GPawn>() || cell.GetGridObject<GPawn>() == linkedPawn) continue;
            
            newValidCells.Add(cell.hexCoordinates);
        }
        
        return validCells = newValidCells.ToArray();
    }
}