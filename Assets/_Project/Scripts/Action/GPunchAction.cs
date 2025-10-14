using System.Collections.Generic;
using UnityEngine;

public class GPunchAction : GAction
{ 
    [SerializeField]
    private int _damage = 1;
    [SerializeField]
    private int _stun = 1;
    float _progress = 0;
    
    
    public override void PreProcess()
    {
        if (!targetCell || targetCell.GetPawn() || targetCell.GetPawn() == linkedPawn) return;
        //validate
    }

    public override void Start_Action()
    {
        base.Start_Action();
        targetCell.GetPawn().TakeDamage(_damage);
        targetCell.GetPawn().Stun(_stun);
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
        List<GHexCoordinate> validCells = new List<GHexCoordinate>();

        foreach (var cell in linkedPawn.currentCell._neighbors)
        {
            if (!cell || !cell.GetPawn() || cell.GetPawn() == linkedPawn) continue;
            
            validCells.Add(cell._hexCoordinates);
        }
        
        return validCells.ToArray();
    }
}