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
    [ReadOnly] public GHexCoordinate[] validCells = Array.Empty<GHexCoordinate>();
    
    [ReadOnly] public EActionState CurrentState { get; private set; } = EActionState.None;

    public GAction(){}
    public GAction(GPawn inLinkedPawn, GCell inTargetCell, Action inOnActionStarted = null,
        Action inOnActionFinished = null)
    {
        linkedPawn = inLinkedPawn;
        targetCell = inTargetCell;
        OnActionStarted += inOnActionStarted;
        OnActionFinished += inOnActionFinished;
    }
    
    /* Create a new instance of the action with the same parameters, Override this for Add Params */
    public virtual GAction CloneAction()
    {
        GAction clone = (GAction)Activator.CreateInstance(this.GetType());
        clone.linkedPawn = linkedPawn;
        clone.targetCell = targetCell;
        clone.CurrentState = CurrentState;
        clone.OnActionStarted = OnActionStarted;
        clone.OnActionFinished = OnActionFinished;
        return clone;
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
    
    public virtual GHexCoordinate[] GetValidCells() { return validCells; }
    
    public bool IsValidCell(GHexCoordinate cell) { return GetValidCells().Contains(cell); }
    
}