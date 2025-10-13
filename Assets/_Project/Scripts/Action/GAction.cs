using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine.Serialization;

[Serializable]
public abstract class GAction
{
    public event Action OnActionStarted; 
    public event Action OnActionFinished;

    [ReadOnly] public GPawn linkedPion;
    [ReadOnly] public GCell targetCell;
    
    public abstract void PreProcess();
    
    public virtual void Start_Action() { OnActionStarted?.Invoke(); }

    public virtual void Update_Action(float delta) {}
    
    public virtual void End_Action() { OnActionFinished?.Invoke(); }
    
    public virtual GHexCoordinate[] GetValidCells() { return Array.Empty<GHexCoordinate>(); }
    
    public virtual bool IsValid() { return false; }
    public bool IsValidCell(GHexCoordinate cell) { return GetValidCells().Contains(cell); }
    
}