using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GPawn))]
public class GAIController : GController
{
    [SerializeReference]
    [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
    public GAIBehavior _aiBehavior;
    
    public GPawn pawn => pawns.Count > 0 ? pawns[0] : null;

    public override void StartTurn()
    {
        bool isStunned = pawn.stunTurns > 0;
        base.StartTurn();
        if (isStunned)
        {
            StopTurn();
            return;
        }
        pawn.remainingActionToken = (int)pawn.AttributesController.GetFinal(EAttributeType.MaxAction);
        _aiBehavior.OnTurnStart();
        StartAction();
    }

    public void ResetTurn()
    {
        pawn.remainingActionToken = (int)pawn.AttributesController.GetFinal(EAttributeType.MaxAction);
    }

    public override void StartAction()
    {
        var action = _aiBehavior.GetAction();
        if (action == null)
        {
            StopTurn();
            return;
        }
        base.StartAction();
        pawn.remainingActionToken--;
        action.OnActionFinished += OnActionOver;
        bool isValid = pawn.RequestAction(action);
    }

    public override void OnActionOver()
    {
        base.OnActionOver();
        if (pawn.remainingActionToken == 0)
        {
            StopTurn();
        }
        else
        {
            StartCoroutine(RestartAction());
        }
    }

    IEnumerator RestartAction()
    {
        yield return null;
        StartAction();
    }

    public override void EndTurn()
    {
        base.EndTurn();
        _aiBehavior.OnTurnEnd();
    }

    public void OnKilled()
    {
        GTurnBaseManager.Instance.UnregisterController(this);
        GTurnBaseManager.Instance.RequestEndTurn(this, false);
    }

    void OnDestroy()
    {
        GTurnBaseManager.Instance.UnregisterController(this);
    }

    void Awake()
    {
        RegisterPawn(GetComponent<GPawn>());
        _aiBehavior = ScriptableObject.Instantiate(_aiBehavior);
        _aiBehavior.Init(this);
        pawn.data.actionList = _aiBehavior.actions;
        pawn.OnKill += OnKilled;
    }
    
}

