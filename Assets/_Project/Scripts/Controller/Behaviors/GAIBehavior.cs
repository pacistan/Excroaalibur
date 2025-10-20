using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GAIBehavior : ScriptableObject
{
    
    [HideInInspector]
    public List<GAction> actions;
    protected GAIController _controller;

    public virtual void Init(GAIController controller)
    {
        _controller = controller;
    }
    
    public virtual  void OnTurnStart()
    {
    }

    public abstract GAction GetAction();

    public virtual void OnTurnEnd(){}
    
    public virtual void OnActionOver(){}
    
    public virtual void OnReceivedEquipment(GEquipment equipment)
    {

    }

    public virtual void OnLoseEquipment(GEquipment lostItem)
    {
        if (lostItem is GCrown)
        {
        }
    }
    
    protected virtual GMoveAction CreateMoveAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GetAction<GMoveAction>(out GMoveAction action);
        GMoveAction moveAction = action.CloneAction() as GMoveAction;
        moveAction.targetCell = GetClosestCellToTargetCell(_controller.pawn.currentCell, targetCell, moveAction._maxMoveDistance);
        moveAction.OnActionFinished += OnActionOver;
        moveAction.OnActionFinished += inOnActionFinished;
        return moveAction;
    }

    protected virtual GPushAction CreatePushAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GetAction<GPushAction>(out GPushAction action);
        GPushAction pushAction = action.CloneAction() as GPushAction;
        pushAction.linkedPawn = _controller.pawn;
        pushAction.targetCell = targetCell;
        pushAction.OnActionFinished += OnActionOver;
        pushAction.OnActionFinished += inOnActionFinished;
        return pushAction;
    }
    
    protected virtual GPlaceOnAltarAction CreatePlaceOnAltarAction(GCell targetCell, Action inOnActionFinished = null)
    {
        GetAction<GPlaceOnAltarAction>(out GPlaceOnAltarAction action);
        GPlaceOnAltarAction placeOnAltarAction = action.CloneAction() as GPlaceOnAltarAction;
        placeOnAltarAction.linkedPawn = _controller.pawn;
        placeOnAltarAction.targetCell = targetCell;
        placeOnAltarAction.OnActionFinished += OnActionOver;
        placeOnAltarAction.OnActionFinished += inOnActionFinished;
        return placeOnAltarAction;
    }

    protected GCell GetClosestCellToTargetCell(GCell startCell, GCell endCell, int distance)
    {
        var path = GGridManager.Instance.GetPath(startCell, GGridManager.Instance.GetLowestAdjacentCell(endCell), false, distance);
        GCell cell = startCell;
        foreach (EHexDirection direction in path)
        {
            cell = cell._neighbors[(int)direction];
        }
        return cell;
    }

    protected abstract void GetAction<T>(out T action) where T : GAction;
    
    protected GPawn GetPotentialTargetPlayer(out int distance)
    {
        return GGridObjectRegistry.GetClosestObjectOfTypeWithPredicate<GPawn>
                (startCell: _controller.pawn.currentCell, 
                out distance, 
                predicate: player => player.isPlayer);
    }
}
