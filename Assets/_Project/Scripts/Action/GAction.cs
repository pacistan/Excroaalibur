using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;

// Data container for passing parameters between actions
// This is an equivalent to a String/Untyped dictionary
[Serializable]
public class GActionContext
{
    private Dictionary<string, object> _data = new Dictionary<string, object>();
    
    public void Set<T>(string key, T value) => _data[key] = value;
    
    public T Get<T>(string key, T defaultValue = default)
    {
        if (_data.TryGetValue(key, out object value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }
    
    public bool TryGet<T>(string key, out T value)
    {
        if (_data.TryGetValue(key, out object obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }
        value = default;
        return false;
    }
    
    public bool Has(string key) => _data.ContainsKey(key);
}

[Serializable]
public abstract class GAction
{
    public enum EActionState
    {
        None,
        PreProcessing,
        InProgress,
        Finished
    }
    
    public event Action OnActionStarted; 
    public event Action OnActionFinished;

    [ReadOnly] public GPawn linkedPawn;
    [ReadOnly] public GCell targetCell;
    [ReadOnly] public GHexCoordinate[] validCells = Array.Empty<GHexCoordinate>();
    
    [ReadOnly] public EActionState CurrentState { get; protected set; } = EActionState.None;

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
    
    public virtual void PreProcess(GActionContext context = null)
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