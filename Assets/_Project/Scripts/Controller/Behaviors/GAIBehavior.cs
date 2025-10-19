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
    
    protected GCrown GetPotentialTargetCrown(out int distance)
    {
        distance = -1;
        IEnumerable<GCrown> crowns = GGridObjectRegistry.Instance.GetItemsByPredicate<GCrown>(crown => crown.owner == null || crown.owner.isPlayer);

        if (crowns == null || crowns.Count() == 0) return null;
        
        int shortestDistance = int.MaxValue;
        GCrown targetCrown = null;
        foreach (var crown in crowns)
        {
            GCell crownCell = crown.owner ? crown.owner.currentCell : crown.currentCell;
            int crownStep =  GGridManager.Instance.GetStep(crownCell, true);
            if (crownStep != -1 && crownStep < shortestDistance) 
            {
                shortestDistance = crownStep;
                targetCrown = crown;
            }
        }
        distance = shortestDistance;
        return targetCrown;
    }

    protected GPawn GetPotentialTargetPlayer(out int distance)
    {
        distance = -1;
        IEnumerable<GPawn> players = GGridObjectRegistry.Instance.GetItemsByPredicate<GPawn>(player => player.isPlayer);

        if (players == null || players.Count() == 0) return null;
        
        
        int shortestDistance = int.MaxValue;
        GPawn targetPlayer = null;
        foreach (var player in players)
        {
            GCell playerCell = player.currentCell;
            int playerStep =  GGridManager.Instance.GetStep(playerCell, true);
            if (playerStep != -1 && playerStep < shortestDistance) 
            {
                shortestDistance = playerStep;
                targetPlayer = player;
            }
        }
        distance = shortestDistance;
        return targetPlayer;
    }
    
    protected GAltar GetPotentialTargetAltar(out int distance)
    {
        distance = -1;
        List<GAltar> receptacles = GGridObjectRegistry.Instance.GetItems<GAltar>();

        if (receptacles == null || receptacles.Count() == 0) return null;
        
        int shortestDistance = int.MaxValue;
        GAltar targetAltar = null;
        foreach (var receptacle in receptacles)
        {
            int crownStep = GGridManager.Instance.GetStep(receptacle.currentCell, true);
            if (crownStep != -1 && crownStep < shortestDistance) 
            {
                shortestDistance = crownStep;
                targetAltar = receptacle;
            }
        }
        distance = shortestDistance;
        return targetAltar;
    }
    
}
