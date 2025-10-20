using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GPawn))]
public class GAIController : GController
{
    [HideInInspector]
    public  GPawn pawn;
    [SerializeReference]
    [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
    GAIBehavior _aiBehavior;
    
    public override void StartTurn()
    {
        bool isStunned = pawn.IsStunned;
        base.StartTurn();
        if (isStunned)
        {
            StopTurn();
            return;
        }
        pawn.remainingActionToken = pawn.actionTokens;
        _aiBehavior.OnTurnStart();
        StartAction();
    }

    public void ResetTurn()
    {
        pawn.remainingActionToken = pawn.actionTokens;
    }

    public override void StartAction()
    {
        base.StartAction();
        pawn.remainingActionToken--;
        var action = _aiBehavior.GetAction();
        if (action == null)
        {
            StopTurn();
        }
        else
        {
            action.OnActionFinished += OnActionOver;
            bool isValid = pawn.RequestAction(action);
        }
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
    
    void Start()
    {
        pawn = GetComponent<GPawn>();
        _aiBehavior = ScriptableObject.Instantiate(_aiBehavior);
        _aiBehavior.Init(this);
        pawn.actions = _aiBehavior.actions;
        pawn.OnKill += OnKilled;
    }
}

