using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class GController : MonoBehaviour
{
    [field : SerializeField, Min(1)]
    public int actionTokens { get; set; }

    [SerializeField, ReadOnly]
    public int remainingActionToken;

    public event Action OnStartTurn;
    public event Action OnEndTurn;
    public event Action OnStartAction;
    
    protected List<GPawn> pawns = new List<GPawn>();
    
    public void RegisterPawn(GPawn pawn)
    {
        pawns.Add(pawn);
        OnStartTurn += pawn.OnStartTurn;
        OnEndTurn += pawn.OnEndTurn;
        OnStartAction += pawn.OnStartAction;
    }

    public virtual void StartTurn()
    {
        OnStartTurn?.Invoke();
    }

    public virtual void StartAction() { OnStartAction?.Invoke(); }
    
    public virtual void OnActionOver() {}
    
    public virtual void EndTurn() { OnEndTurn?.Invoke(); }
    
    protected void StopTurn()
    {
        GTurnBaseManager.Instance.RequestEndTurn(this);
    }
}