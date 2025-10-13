using System;
using UnityEngine;

[RequireComponent(typeof(GPion))]
public class GAIController : MonoBehaviour, GIController
{
    [SerializeField]
    private int _startingActionTokensNumber;
    private GPion _pawn;

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
        _pawn = GetComponent<GPion>();
    }
}