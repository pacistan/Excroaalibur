using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class GController : MonoBehaviour
{
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
    
    protected virtual void StopTurn()
    {
        GTurnBaseManager.Instance.RequestEndTurn(this);
    }
}