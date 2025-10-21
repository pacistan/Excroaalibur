using DG.Tweening;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GPushedReaction : GAction
{
    [SerializeField]
    bool _isPushable = true;
    
    [SerializeField]
    bool _DamageRelatedToPushForce = false;
    
    [SerializeField, HideIf("_DamageRelatedToPushForce"), Tooltip("Damage inflicted if we hit Something while being pushed")]
    int _damage = 1;
    
    [SerializeField, Tooltip("Stun inflicted if we hit Something while being pushed")]
    int _stun = 1;
    
    
    int _distance;
    
    bool _inflictDamage;
    EHexDirection _direction;
    GMoveAction _moveAction = null;
    
    GEquipment CachedEquipment;
    
    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess(context);

        if (context == null)
        {
            Debug.LogWarning($"GPushedReaction on {linkedPawn} missing context");
            return;
        }
        
        if (context.Has("direction"))
            _direction = context.Get<EHexDirection>("direction");
        if (context.Has("force"))
            _distance = context.Get<int>("force");
        
        if (_isPushable) // Check initial param
        {
            if (linkedPawn.GetCell().GetNeighbor(_direction).GetTileType == ETileType.Wall 
                || linkedPawn.GetCell().GetNeighbor(_direction).GetGridObject<GPawn>())
            {
                _isPushable = false;
                _inflictDamage = true;
            }
        }

        if (!_isPushable) // do not put in else !
        {
            GPawn Instigitator = linkedPawn.GetCell().GetNeighbor(_direction.Opposite()).GetGridObject<GPawn>();
            if (Instigitator) 
            {
                CachedEquipment = linkedPawn.equipment;
                if (CachedEquipment)
                {
                    linkedPawn.ReleaseEquipement(false, true);
                    Instigitator.GiveEquipement(CachedEquipment, false, false);
                }
            }
            
            if (linkedPawn is GAltar) 
                _inflictDamage = false;
        } 
        else 
        {
            if (linkedPawn.equipment)
            {
                linkedPawn.ReleaseEquipement(true, true);
            }
            
            GCell cell = linkedPawn.GetCell();
            for (int i = 0; i < _distance; i++)
            {
                GCell neighbor = cell.GetNeighbor(_direction);
            
                if (!neighbor || neighbor.GetTileType == ETileType.Wall)
                {
                    _inflictDamage = true;
                    _damage = _DamageRelatedToPushForce ? _distance - i: _damage;
                    break;
                }
                
                cell = neighbor;
                if (neighbor.GetTileType == ETileType.Hole)
                    break;
            }
            
            _moveAction = new GMoveAction();
            _moveAction.targetCell = cell;
            _moveAction.linkedPawn = linkedPawn;
            _moveAction._maxMoveDistance = _distance;
            _moveAction._walkingTileType = new ETileType[] { ETileType.Normal, ETileType.Hole };
            _moveAction._endMovementTileType = new ETileType[] { ETileType.Normal, ETileType.Hole };
            GTurnBaseManager.Instance.PreProcessReaction(_moveAction, new GActionContext());
        }
        
        if (_inflictDamage)
        {
            linkedPawn.TakeDamage(_damage);
            linkedPawn.Stun(_stun);
        }
    }

    public override void Start_Action()
    {
        base.Start_Action();

        if (_inflictDamage)
        {
            linkedPawn.UpdateHpNumber();
            linkedPawn.UpdateStunTurn();
        }
        
        if (_moveAction != null)
        {
            GTurnBaseManager.Instance.TryStartReaction(_moveAction);
        }
        else
        {
            //End_Action();
        } 
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        
        if (_moveAction == null || _moveAction.CurrentState == GAction.EActionState.Finished)
        {
            if (linkedPawn && !linkedPawn.IsAlive && !linkedPawn.isMarkedForDestruction)
            {
                linkedPawn.Kill();
            }
            End_Action();
        }
    }

    public override void End_Action()
    {
        // TODO : check error null here !
        if (CachedEquipment)
            CachedEquipment.owner.GiveEquipement(CachedEquipment, true, true);
        base.End_Action();
    }

    public override GAction CloneAction()
    {
        GPushedReaction reaction = base.CloneAction() as GPushedReaction;
        reaction._isPushable = _isPushable;
        reaction._DamageRelatedToPushForce = _DamageRelatedToPushForce;
        reaction._damage = _damage;
        reaction._stun = _stun;
        return reaction;
    }
}