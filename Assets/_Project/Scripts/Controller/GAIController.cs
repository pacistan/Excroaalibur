using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GPawn))]
public class GAIController : GController
{
    public enum EAIBehaviorType {Sentry}
    
    [SerializeField]
    private int _startingActionTokensNumber;
    [HideInInspector]
    public  GPawn pawn;
    [SerializeReference]
    GAIBehavior _aiBehavior;
    
    public override void StartTurn()
    {
        actionTokens = _startingActionTokensNumber;
        StartAction();
    }

    public override void StartAction()
    {
        actionTokens--;
        var action = _aiBehavior.GetAction();
        if (action == null)
        {
            EndTurn();
        }
        else
        {
            action.OnActionFinished += OnActionOver;
            bool isValid = GTurnBaseManager.Instance.TryPlayAction(action, false);
            if (isValid)
            {
            }
        }
    }

    public override void OnActionOver()
    {
        if (actionTokens == 0)
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

