using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public abstract class GController : MonoBehaviour
{
    public event Action OnStartTurn;
    
    [SerializeField, FoldoutGroup("Events"), Tooltip("Event triggered when an action starts")]
    protected UnityEvent OnStartActionEvent;
    
    public event Action OnEndTurn;
    public event Action OnStartAction;
    
    protected List<GPawn> pawns = new List<GPawn>();

    public GPawn currentPawn;
    
    public void RegisterPawn(GPawn pawn)
    {
        pawns.Add(pawn);
        OnStartTurn += pawn.OnStartTurn;
        OnEndTurn += pawn.OnEndTurn;
        OnStartAction += pawn.OnStartAction;
    }

    public void UnregisterPawn(GPawn pawn)
    {
        if (!pawns.Contains(pawn))
        {
            Debug.LogError($"Trying to unregister not registered Pawn", pawn);
            return;
        }
        pawns.Remove(pawn);
        OnStartTurn -= pawn.OnStartTurn;
        OnEndTurn -= pawn.OnEndTurn;
        OnStartAction -= pawn.OnStartAction;
    }

    public virtual void StartTurn()
    {
        OnStartTurn?.Invoke();
    }

    public virtual void StartAction()
    {
        OnStartAction?.Invoke();
        OnStartActionEvent?.Invoke();
    }
    
    public virtual void OnActionOver() {}
    
    public virtual void EndTurn() { OnEndTurn?.Invoke(); }
    
    protected virtual void StopTurn()
    {
        GTurnBaseManager.Instance.RequestEndTurn(this);
    }
}