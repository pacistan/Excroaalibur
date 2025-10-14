using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GController : MonoBehaviour
{
    [field: SerializeField, Min(1)]
    public int actionTokens;

    public event Action OnStartTurn;
    public event Action OnEndTurn;
    
    protected List<GPawn> pawns = new List<GPawn>();
    
    public void RegisterPawn(GPawn pawn)
    {
        pawns.Add(pawn);
        OnStartTurn += pawn.OnStartTurn;
        OnEndTurn += pawn.OnEndTurn;
    }
    
    public virtual void StartTurn() {}

    public virtual void StartAction() { OnStartTurn?.Invoke(); }
    
    public virtual void OnActionOver() {}
    
    public virtual void EndTurn() { OnEndTurn?.Invoke(); }
    
    protected void StopTurn()
    {
        GTurnBaseManager.Instance.RequestEndTurn(this);
    }
}