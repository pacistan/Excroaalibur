using System;
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
    [SerializeField]
    private EAIBehaviorType _aiBehaviorType;

    GAIBehavior _aiBehavior;
    
    public int actionTokens { get; set; }
    
    public void StartTurn()
    {
        actionTokens = _startingActionTokensNumber;
        StartAction();
    }

    public void StartAction()
    {
        actionTokens--;
        var action = _aiBehavior.GetAction();
        // TODO : Give Action to manager
    }

    public void OnActionOver()
    {
        if (actionTokens == 0)
        {
            EndTurn();
        }
        else
        {
            StartAction();
        }
    }

    public void EndTurn()
    {
    }
    
    void Start()
    {
        pawn = GetComponent<GPawn>();
        switch (_aiBehaviorType)
        {
            case EAIBehaviorType.Sentry:
                _aiBehavior = new GSentryBehavior(this);
                break;
        }
    }
}

