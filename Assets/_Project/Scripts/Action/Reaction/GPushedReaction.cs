using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
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
    
    [SerializeField, Tooltip("Tile types on which the pawn can be pushed")]
    ETileType[] _pushedTileType = new ETileType[] { ETileType.Normal, ETileType.Hole };
    

    [SerializeField]
    GActionPrevisualisationCurveData _previsuCurveData;
    
    int _distance;
    
    bool _inflictDamage;
    EHexDirection _direction;
    GMoveAction _moveAction = null;
    
    GEquipment CachedEquipment;

    public override List<GCell> Previsualisation(in GActionContext previsuContext)
    {
        if (previsuContext == null)
        {
            Debug.LogWarning( $"GPushedReaction on {linkedPawn} missing context");
            return null;
        }
        
        List<GCell> PreviewCells = new List<GCell>();
        if (previsuContext.Has(GActionContext.DIRECTION_STRING))
            _direction = previsuContext.Get<EHexDirection>(GActionContext.DIRECTION_STRING);
        if (previsuContext.Has(GActionContext.FORCE_STRING))
            _distance = previsuContext.Get<int>(GActionContext.FORCE_STRING);
        
        if (_isPushable) // Check initial param
        {
            if (!_pushedTileType.Contains(linkedPawn.GetCell().GetNeighbor(_direction).GetTileType)
                || linkedPawn.GetCell().GetNeighbor(_direction).GetGridObject<GPawn>())
            {
                _isPushable = false;
                _inflictDamage = true;
            }
        }

        if (!_isPushable) // do not put in else !
        {
            if (linkedPawn is GAltar) _inflictDamage = false;
            PreviewCells.Add(linkedPawn.GetCell());
        } 
        else 
        {
            
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
            
            PreviewCells.Add(cell);
            
            AddPrevisualisationCurve(previsuContext, linkedPawn.GetPrevisuPosition(), 
                cell.transform.position, _previsuCurveData);
        }
        
        if (_inflictDamage)
        {
            previsuContext.Set(GActionContext.DAMAGE_STRING, _damage);
            previsuContext.Set(GActionContext.STUN_STRING, _stun);
        }
        
        return PreviewCells;
    }
    
    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess(context);

        if (context == null)
        {
            Debug.LogWarning($"GPushedReaction on {linkedPawn} missing context");
            return;
        }
        
        if (context.Has(GActionContext.DIRECTION_STRING))
            _direction = context.Get<EHexDirection>(GActionContext.DIRECTION_STRING);
        if (context.Has(GActionContext.FORCE_STRING))
            _distance = context.Get<int>(GActionContext.FORCE_STRING);
        
        if (_isPushable) // Check initial param
        {
            if (!_pushedTileType.Contains(linkedPawn.GetCell().GetNeighbor(_direction).GetTileType)
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
                linkedPawn.ReleaseEquipement(true, true);
            
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
                {
                    targetCell = neighbor;
                    _inflictDamage = true;
                    _damage = 0;
                    _stun = 1;
                    break;
                }
            }
            
            _moveAction = new GMoveAction();
            _moveAction.InitAction(linkedPawn);
            _moveAction.OverrideTileType(_pushedTileType, _pushedTileType);
            _moveAction.targetCell = cell;
            _moveAction._maxMoveDistance = _distance;
            _moveAction.moveAnimationName = GPawn.PushedStartAnimationName;
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
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        
        if (_moveAction == null || _moveAction.CurrentState == GAction.EActionState.Finished)
        {
            if (linkedPawn && !linkedPawn.IsAlive && !linkedPawn.isMarkedForDestruction &&
                !(targetCell && targetCell.data.tileType == ETileType.Hole))
                linkedPawn.Kill();
            
            End_Action();
        }
    }

    public override void End_Action()
    {
        if (CachedEquipment) // Need For the case where we are pushed into a wall and have to give back the equipment 
            CachedEquipment.owner.GiveEquipement(CachedEquipment, true, true);
        base.End_Action();
    }

    public override ETileHighlightActionType GetHighlightActionType()
    {
        throw new System.NotImplementedException();
    }

    public override GAction CloneAction()
    {
        GPushedReaction reaction = base.CloneAction() as GPushedReaction;
        reaction._isPushable = _isPushable;
        reaction._DamageRelatedToPushForce = _DamageRelatedToPushForce;
        reaction._damage = _damage;
        reaction._stun = _stun;
        reaction._pushedTileType = _pushedTileType;
        reaction._previsuCurveData = _previsuCurveData;
        return reaction;
    }
}