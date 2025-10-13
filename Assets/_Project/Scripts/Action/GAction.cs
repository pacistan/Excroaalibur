using Sirenix.OdinInspector;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public enum EActionState
{
    None,
    PreProcessing,
    InProgress,
    Finished
}

[Serializable]
public abstract class GAction
{
    public event Action OnActionStarted; 
    public event Action OnActionFinished;

    [ReadOnly] public GPawn linkedPawn;
    [ReadOnly] public GCell targetCell;

    [ReadOnly] public EActionState CurrentState { get; private set; } = EActionState.None;
    
    public GAction Duplicate()
    {
        GAction copy = (GAction)Activator.CreateInstance(this.GetType());
        copy.linkedPawn = linkedPawn;
        copy.targetCell = targetCell;
        copy.CurrentState = CurrentState;
        return copy;
    }
    
    public virtual void PreProcess()
    {
        CurrentState = EActionState.PreProcessing;
    }
    
    public virtual void Start_Action()
    {
        CurrentState = EActionState.InProgress;
        OnActionStarted?.Invoke();
    }

    public virtual void Update_Action(float delta) {}

    public virtual void End_Action()
    {
        CurrentState = EActionState.Finished;
        OnActionFinished?.Invoke();
    }
    
    public virtual GHexCoordinate[] GetValidCells() { return Array.Empty<GHexCoordinate>(); }
    
    public virtual bool IsValid() { return false; }
    public bool IsValidCell(GHexCoordinate cell) { return GetValidCells().Contains(cell); }
    
}