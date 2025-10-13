using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GPawn))]
public class GAIController : GController
{
    [SerializeField]
    private int _startingActionTokensNumber;
    [HideInInspector]
    public  GPawn pawn;

    public int actionTokens { get; set; }
    
    public void StartTurn()
    {
        actionTokens = _startingActionTokensNumber;
        StartAction();
    }

    public void StartAction()
    {
        actionTokens--;
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
    }
}

