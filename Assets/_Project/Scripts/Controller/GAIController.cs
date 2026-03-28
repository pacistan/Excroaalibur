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
    

    public override void StartTurn()
    {
        bool isStunned = currentPawn.stunTurns > 0;
        base.StartTurn();
        if (isStunned)
        {
            StopTurn();
            return;
        }
        currentPawn.remainingActionToken = (int)currentPawn.AttributesController.GetFinal(EAttributeType.MaxAction);
        _aiBehavior.OnTurnStart();
        StartAction();
    }

    public void ResetTurn()
    {
        currentPawn.remainingActionToken = (int)currentPawn.AttributesController.GetFinal(EAttributeType.MaxAction);
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
        currentPawn.remainingActionToken--;
        action.OnActionFinished += OnActionOver;
        bool isValid = currentPawn.RequestAction(action);
    }

    public override void OnActionOver()
    {
        base.OnActionOver();
        if (currentPawn.remainingActionToken == 0)
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

    void Start()
    {
        RegisterPawn(GetComponent<GPawn>());
        currentPawn = pawns.Count > 0 ? pawns[0] : null;
        _aiBehavior = ScriptableObject.Instantiate(_aiBehavior);
        _aiBehavior.Init(this);
        currentPawn.data.actionList = _aiBehavior.actions;
        currentPawn.OnKill += OnKilled;
    }
    
}

