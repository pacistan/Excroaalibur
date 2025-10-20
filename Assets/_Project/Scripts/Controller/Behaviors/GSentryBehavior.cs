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


    [SerializeField, Tooltip("If true the sentry will try to go push player if crown is not an option")]
    protected bool _isFouteurDeMerde;

    [SerializeField, Tooltip("If true when another sentry has the crown, this sentry will attack it to steal it")]
    protected bool _isDirtyStealer;
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

        GPawn linkedPawn = _controller.pawn;
        GCell pawnCell = linkedPawn.GetCell();
        GGridManager.Instance.GenerateStepMap(_controller.pawn.GetCell());
        bool hasCrown = _controller.pawn.equipment && _controller.pawn.equipment is GCrown;

        if (hasCrown)
        {
            GAltar altar = GGridObjectRegistry.GetClosestObjectOfType<GAltar>(pawnCell, out int altarDistance);
            if (altarDistance == 1)
            {
                _isTurnOver = true;
                return CreatePlaceOnAltarAction(altar.GetCell());
            }   
            else if (altarDistance > 0 && !_hasMoved)
            {
                _hasMoved = true;
                if (altarDistance > _moveAction._maxMoveDistance)
                {
                    _isTurnOver = true;
                }
                return  CreateMoveAction(altar.GetCell());
            }
        }
        else
        {
            GCrown crown = GGridObjectRegistry.GetClosestObjectOfTypeWithPredicate<GCrown>(
                pawnCell, out int crownDistance, 
                crown=> !(crown.owner is GAltar) && (_isDirtyStealer || (!crown.owner || crown.owner.isPlayer)));
            
            if (crown && crownDistance == 1 && crown.owner)
            {
                _isTurnOver = true;
                return CreatePushAction(crown.GetCell());
            }   
            else if (crown && crownDistance > 0 && !_hasMoved)
            {
                _hasMoved = true;
                if (crownDistance > _moveAction._maxMoveDistance)
                {
                    _isTurnOver = true;
                }
                return CreateMoveAction(crown.GetCell());
            }    
        }
        
        if(!_isFouteurDeMerde) return null;
        
        GPawn player = GetPotentialTargetPlayer(out int distance);
        if (distance == 1)
        {
            _isTurnOver = true;
            return CreatePushAction(player.GetCell());
        }
        else if (distance > 0 && !_hasMoved)
        {
            GCell cell = player.GetCell();
            _hasMoved = true;
            if (distance > _moveAction._maxMoveDistance)
            {
                _isTurnOver = true;
            }
            return CreateMoveAction(cell);
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