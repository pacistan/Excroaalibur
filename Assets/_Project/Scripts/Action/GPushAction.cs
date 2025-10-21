using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class GPushAction : GAction
{
    [SerializeField, Min(0), Tooltip("Number of cells the pushed pawn will be moved away")]
    private int _pushForce = 2;
    
    [SerializeField, Min(0), Tooltip("Distance Of the pawn following the pushed pawn, if possible")]
    private int _followDistance = 1;
    
    EHexDirection _direction = EHexDirection.NE;
    GPawn _targetPawn;
    
    GAction _reaction;
    GMoveAction _followAction = null;
    
    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess(context);
        if (linkedPawn.equipment || linkedPawn.equipment is GCrown) return;
        _direction = linkedPawn.coordinate.GetLineDirection(targetCell.hexCoordinates);
        _targetPawn = targetCell.GetGridObject<GPawn>();
        if (!_targetPawn) return;

        _reaction = _targetPawn.GetReaction(this);
        if (_reaction != null)
        {
            GActionContext pushContext = new GActionContext();
            pushContext.Set("direction", _direction);
            pushContext.Set("force", _pushForce);
            _reaction.linkedPawn = _targetPawn;
            GTurnBaseManager.Instance.PreProcessReaction(_reaction, pushContext);
        }

        GCell pathCell = linkedPawn.currentCell;

        if (!(_targetPawn is GAltar))
        {
            for (int i = 0; i < _followDistance; i++)
            {
                GCell neighbor = pathCell.GetNeighbor(_direction);
                
                if (!neighbor || neighbor.GetGridObject<GPawn>() || neighbor.GetTileType == ETileType.Wall) break;

                pathCell = neighbor;
                if (neighbor.GetTileType == ETileType.Hole) break;
            }
        }
        
        _followAction = new GMoveAction();
        _followAction.targetCell = pathCell;
        _followAction.linkedPawn = linkedPawn;
        _followAction._maxMoveDistance = _followDistance;
        _followAction._walkingTileType = new ETileType[] { ETileType.Normal, ETileType.Hole };
        _followAction._endMovementTileType = new ETileType[] { ETileType.Normal, ETileType.Hole };
        GTurnBaseManager.Instance.PreProcessReaction(_followAction, new GActionContext());
    }

    public override void Start_Action()
    {
        base.Start_Action();
        
        // TODO Check the need to replace the tryStartReaction with an event queue for all preprocessed callbacks
        GTurnBaseManager.Instance.TryStartReaction(_reaction);
        GTurnBaseManager.Instance.TryStartReaction(_followAction);
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        if (_followAction.CurrentState == GAction.EActionState.Finished 
            && _reaction.CurrentState == GAction.EActionState.Finished)
        {
            End_Action();
        }
    }

    public override void End_Action()
    {
        base.End_Action();
    }

    public override GHexCoordinate[] GetValidCells()
    {
        if (linkedPawn.equipment || linkedPawn.equipment is GCrown)
            return validCells = new GHexCoordinate[]{};
        
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();
        
        foreach (GCell cell in linkedPawn.currentCell.neighbors)
        {
            if (!cell || !cell.GetGridObject<GPawn>() || cell.GetGridObject<GPawn>() == linkedPawn) continue;
            
            newValidCells.Add(cell.hexCoordinates);
        }
        
        return validCells = newValidCells.ToArray();
    }
}