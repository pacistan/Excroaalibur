using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "AI Behavior/Sentry", fileName = "Sentry Behaviour")]
public class GSentryBehavior : GAIBehavior
{
    [SerializeReference]
    GMoveAction _moveAction;

    [SerializeReference]
    GPushAction _pushAction;
    
    [SerializeReference]
    GPlaceOnAltarAction _placeOnAltarAction;

    protected bool _hasMoved = false;
    protected bool _isTurnOver = false;
    
    public override void Init(GAIController controller)
    {
        base.Init(controller);
        actions = new List<GAction>()        {
            _moveAction,
            _pushAction,
            _placeOnAltarAction
        };

        controller.pawn.OnEquip += OnReceivedEquipment;
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        _hasMoved = false;
        _isTurnOver = false;
    }

    public override GAction GetAction()
    {
        if (_isTurnOver)
            return null;
        
        GGridManager.Instance.GenerateStepMap(_controller.pawn.currentCell);
        bool hasCrown = _controller.pawn.equipment && _controller.pawn.equipment is GCrown;

        if (hasCrown)
        {
            GAltar altar = GetPotentialTargetAltar(out int altarDistance);
            if (altarDistance == 1)
            {
                _isTurnOver = true;
                return CreatePlaceOnAltarAction(altar.currentCell);
            }   
            else if (altarDistance > 0 && !_hasMoved)
            {
                _hasMoved = true;
                return  CreateMoveAction(altar.currentCell);
            }
        }
        else
        {
            GCrown crown = GetPotentialTargetCrown(out int crownDistance);
            if (crownDistance == 1 && crown.owner)
            {
                _isTurnOver = true;
                return CreatePushAction(crown.GetCell());
            }   
            else if (crownDistance > 0 && !_hasMoved)
            {
                _hasMoved = true;
                return CreateMoveAction(crown.GetCell());
            }    
        }
        return null;
    }


    public override void OnReceivedEquipment(GEquipment equipment)
    {
        base.OnReceivedEquipment(equipment);
        if (equipment is GCrown)
        {
            _hasMoved = false;
            _isTurnOver = false;
            _controller.ResetTurn();
        }
    }

    protected override void GetAction<T>(out T action)
    {
        action = null;
        string typeName = typeof(T).Name;
        switch (typeName)
        {
            case "GMoveAction" : 
                action = _moveAction as T; 
                break;
            case "GPushAction" : 
                action = _pushAction as T; 
                break;
            case "GPlaceOnAltarAction" :
                action = _placeOnAltarAction as T;
                break;
        }
    }
}