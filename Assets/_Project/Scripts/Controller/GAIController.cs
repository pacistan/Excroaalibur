using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GPawn))]
public class GAIController : GController
{
    public enum EAIBehaviorType {Sentry}
    [HideInInspector]
    public  GPawn pawn;
    [SerializeReference]
    [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
    GAIBehavior _aiBehavior;
    
    public override void StartTurn()
    {
        remainingActionToken = actionTokens;
        StartAction();
    }

    public override void StartAction()
    {
        remainingActionToken--;
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
        if (remainingActionToken == 0)
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
    }
    
    void Start()
    {
        pawn = GetComponent<GPawn>();
        _aiBehavior = ScriptableObject.Instantiate(_aiBehavior);
        _aiBehavior.Init(this);
        pawn.actions = _aiBehavior.actions;
    }
}

