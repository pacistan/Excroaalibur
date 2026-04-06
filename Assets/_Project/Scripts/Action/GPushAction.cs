using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GPushAction : GAction
{
    public static Action<GPawn, GPawn> OnPushEvent;
    
    [SerializeField, Min(0), Tooltip("Number of cells the pushed pawn will be moved away")]
    public int _pushForce = 2;

    [SerializeField, Min(0), Tooltip("Distance Of the pawn following the pushed pawn, if possible")]
    private int _followDistance = 1;

    [SerializeField, Min(0), Tooltip("Damage taken by the pushed pawn if pushed into a wall or pawn")]
    int _pushDamage = 1;

    [SerializeField, Min(0), Tooltip("Stun turns taken by the pushed pawn if pushed into a wall or pawn")]
    int _pushStunAmount = 1;

    [SerializeField]
    GActionPrevisualisationCurveData _previsuCurveData;

    EHexDirection _direction = EHexDirection.NE;
    GPawn _targetPawn;

    GAction _reaction;
    GMoveAction _followAction = null;

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
            previsuContext.Set(GActionContext.FORCE_STRING, GetAttributeOrBaseValue(_pushForce, EAttributeType.PushStrength));
            previsuContext.Set(GActionContext.DAMAGE_STRING, GetAttributeOrBaseValue(_pushDamage, EAttributeType.PushDamage));
            previsuContext.Set(GActionContext.STUN_STRING, GetAttributeOrBaseValue(_pushStunAmount, EAttributeType.PushStunAmount));
            _reaction.InitAction(_targetPawn);
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

            int actionPointAddNum = linkedPawn.data.isPlayer && !_targetPawn.data.isPlayer ? 1 : 0;
            previsuContext.Set(GActionContext.ACTION_GAIN_STRING, actionPointAddNum);
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
        bool isPushable = true;
        if (_reaction != null)
        {
            GActionContext pushContext = new GActionContext();
            pushContext.Set(GActionContext.DIRECTION_STRING, _direction);
            pushContext.Set(GActionContext.FORCE_STRING, GetAttributeOrBaseValue(_pushForce, EAttributeType.PushStrength));
            pushContext.Set(GActionContext.DAMAGE_STRING, GetAttributeOrBaseValue(_pushDamage, EAttributeType.PushDamage));
            pushContext.Set(GActionContext.STUN_STRING, GetAttributeOrBaseValue(_pushStunAmount, EAttributeType.PushStunAmount));
            if (_reaction is GPushedReaction)
                isPushable = ((GPushedReaction)_reaction).IsPushable;
            _reaction.InitAction(_targetPawn);
            GTurnBaseManager.Instance.PreProcessReaction(_reaction, pushContext);
        }

        GCell pathCell = linkedPawn.GetCell();

        if (!(_targetPawn is GAltar) && isPushable)
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

        RuntimeManager.PlayOneShotAttached("event:/Pawn/Push Prepare", linkedPawn.gameObject);
        linkedPawn.OnAnimationPush += OnAnimationPushCallback;
        linkedPawn.OnAnimationPushEnd += OnAnimationPushEndCallback;
        linkedPawn.visuals.SetAnimationState(GPawn.PushAnimationName, 0.0f);
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
        linkedPawn.visuals.OnUpdateActionsToken();
        
        OnPushEvent?.Invoke(linkedPawn, _targetPawn);
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

    public override EZoneActionType GetHighlightActionType() => EZoneActionType.Push;

    public override GAction CloneAction()
    {
        GPushAction pushAction =  base.CloneAction() as GPushAction;
        pushAction._pushForce = _pushForce;
        pushAction._followDistance = _followDistance;
        return pushAction;
    }

    private void OnAnimationPushCallback()
    {
        RuntimeManager.PlayOneShotAttached("event:/Pawn/Push", linkedPawn.gameObject);
        linkedPawn.OnAnimationPush -= OnAnimationPushCallback;
        GTurnBaseManager.Instance.TryStartReaction(_reaction);
    }
    
    private void OnAnimationPushEndCallback()
    {
        linkedPawn.OnAnimationPushEnd -= OnAnimationPushEndCallback;
        GTurnBaseManager.Instance.TryStartReaction(_followAction);
    }
    
    
}
