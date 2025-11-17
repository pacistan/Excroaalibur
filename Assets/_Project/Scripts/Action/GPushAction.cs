using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GPushAction : GAction
{
    [SerializeField, Min(0), Tooltip("Number of cells the pushed pawn will be moved away")]
    private int _pushForce = 2;
    
    [SerializeField, Min(0), Tooltip("Distance Of the pawn following the pushed pawn, if possible")]
    private int _followDistance = 1;
    
    [SerializeField]
    GActionPrevisualisationCurveData _previsuCurveData;
    
    EHexDirection _direction = EHexDirection.NE;
    GPawn _targetPawn;
    
    GAction _reaction;
    GMoveAction _followAction = null;

    bool _isPushAnimationOver = false;
    
    // TODO : Add What tile types we can push ! (Like walls, holes, Spawner)
    
    public override List<GCell> Previsualisation(in GActionContext previsuContext)
    {
        _direction = linkedPawn.GetHexCoordinate().GetLineDirection(targetCell.hexCoordinates);
        _targetPawn = targetCell.GetGridObject<GPawn>();
        if (!_targetPawn) return null; 

        List<GCell> previewCells = new List<GCell>();
        _reaction = _targetPawn.GetReaction(this);
        if (_reaction != null)
        {
            previsuContext.Set(GActionContext.DIRECTION_STRING, _direction);
            previsuContext.Set(GActionContext.FORCE_STRING, _pushForce);
            _reaction.linkedPawn = _targetPawn;
            previewCells.AddRange(_reaction.Previsualisation(in previsuContext));
        }

        GCell pathCell = linkedPawn.GetCell();

        if (previewCells.Contains(_targetPawn.GetCell())) return previewCells; // No valid cell to follow !
        
        if (!(_targetPawn is GAltar))
        {
            for (int i = 0; i < _followDistance; i++)
            {
                GCell neighbor = pathCell.GetNeighbor(_direction);
                
                if (!neighbor || previewCells.Contains(neighbor)) break;

                pathCell = neighbor;
            }
            
            previewCells.Add(pathCell);
            
            AddPrevisualisationCurve(previsuContext, linkedPawn.GetPrevisuPosition(), 
                pathCell.transform.position, _previsuCurveData);
        }
        
        
        
        return previewCells;
    }
   
    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess(context);
        _direction = linkedPawn.GetCell().hexCoordinates.GetLineDirection(targetCell.hexCoordinates);
        _targetPawn = targetCell.GetGridObject<GPawn>();
        if (!_targetPawn) return;

        _reaction = _targetPawn.GetReaction(this);
        if (_reaction != null)
        {
            GActionContext pushContext = new GActionContext();
            pushContext.Set(GActionContext.DIRECTION_STRING, _direction);
            pushContext.Set(GActionContext.FORCE_STRING, _pushForce);
            _reaction.linkedPawn = _targetPawn;
            GTurnBaseManager.Instance.PreProcessReaction(_reaction, pushContext);
        }

        GCell pathCell = linkedPawn.GetCell();

        if (!(_targetPawn is GAltar))
        {
            for (int i = 0; i < _followDistance; i++)
            {
                GCell neighbor = pathCell.GetNeighbor(_direction);
                
                if (!neighbor || neighbor.GetGridObject<GPawn>() || !linkedPawn.data._walkingTileType.Contains(neighbor.GetTileType)) break;

                pathCell = neighbor;
                if (neighbor.GetTileType == ETileType.Hole) break;
            }
        }
        
        if (pathCell == linkedPawn.GetCell())  return; // No valid cell to follow
        
        _followAction = new GMoveAction();
        _followAction.InitAction(linkedPawn);
        _followAction.targetCell = pathCell;
        _followAction._maxMoveDistance = _followDistance;
        GTurnBaseManager.Instance.PreProcessReaction(_followAction, new GActionContext());
    }

    public override void Start_Action()
    {
        base.Start_Action();

        Vector3 lookAtPosition = targetCell.transform.position;
        lookAtPosition.y = linkedPawn.transform.position.y;
        linkedPawn.transform.LookAt(lookAtPosition);
        
        linkedPawn.OnAnimationPush += OnAnimationPushCallback;
        linkedPawn.visuals.SetAnimationState(GPawn.PushAnimationName, 0.0f);
        linkedPawn.StartCoroutine(StartReactionsCoroutine());
        
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
        if ((_followAction == null || _followAction.CurrentState == GAction.EActionState.Finished) 
             && (_reaction == null ||_reaction.CurrentState == GAction.EActionState.Finished))
        {
            End_Action();
        }
    }

    public override void End_Action()
    {
        base.End_Action();
        linkedPawn.visuals.SetAnimationState(GPawn.IdleAnimationName);
    }
    
    public override GHexCoordinate[] GetValidCells()
    { 
        if (linkedPawn.equipment || linkedPawn.equipment is GCrown)
            return validCells = new GHexCoordinate[]{};
        
        List<GHexCoordinate> newValidCells = new List<GHexCoordinate>();
        
        foreach (GCell cell in linkedPawn.GetCell().neighbors)
        {
            if (!cell
                || !cell.GetGridObject<GPawn>()
                || (cell.GetGridObject<GPawn>() is GAltar && cell.GetGridObject<GPawn>().equipment == null)
                || (linkedPawn.data.isPlayer && cell.GetTileType == ETileType.Spawner)
                || cell.GetTileType == ETileType.Hole)
            {
                continue;
            }
            
            newValidCells.Add(cell.hexCoordinates);
        }
        
        return validCells = newValidCells.ToArray();
    }

    public override ETileHighlightActionType GetHighlightActionType() => ETileHighlightActionType.Push;

    public override GAction CloneAction()
    {
        GPushAction pushAction =  base.CloneAction() as GPushAction;
        pushAction._pushForce = _pushForce;
        pushAction._followDistance = _followDistance;
        return pushAction;
    }

    IEnumerator StartReactionsCoroutine()
    {
        yield return new WaitUntil(() => _isPushAnimationOver);
        linkedPawn.OnAnimationPush -= OnAnimationPushCallback;
        GTurnBaseManager.Instance.TryStartReaction(_reaction);
        GTurnBaseManager.Instance.TryStartReaction(_followAction);
        RuntimeManager.PlayOneShotAttached("event:/Pawn/Push", linkedPawn.gameObject);

    }
    
    private void OnAnimationPushCallback() 
        => _isPushAnimationOver = true;
    
}
