using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// Data container for passing parameters between actions
// This is an equivalent to a String/Untyped dictionary
[Serializable]
public class GActionContext
{
    public const string DAMAGE_STRING = "Damage";
    public const string STUN_STRING = "Stun";
    public const string DIRECTION_STRING = "Direction";
    public const string FORCE_STRING = "Force";
    public const string PREVISU_POS_STRING = "previsuPositions";
    
    private Dictionary<string, object> _data = new Dictionary<string, object>();
    
    // Set a property with a generic type value
    public void Set<T>(string key, T value) => _data[key] = value;
    
    // Remove a property by key
    public void Remove(string key) =>  _data.Remove(key);
    
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
    
    [field: SerializeField, FoldoutGroup("Visuals")]
    public string actionLabel { get; private set; }
    [field: SerializeField, FoldoutGroup("Visuals")]
    public Sprite actionIcon { get; private set; }

    [FormerlySerializedAs("actionCursorIcon")]
    [field: SerializeField, FoldoutGroup("Visuals")]
    private Texture2D[] _actionCursorIcon;

    [field : SerializeField, FoldoutGroup("Visuals")]
    public bool centerCursorOffset { get; private set; }
    
    [ReadOnly, HideInEditorMode] 
    public GPawn linkedPawn;
    [ReadOnly, HideInEditorMode] 
    public GCell targetCell;
    [ReadOnly, HideInEditorMode] 
    public GHexCoordinate[] validCells = Array.Empty<GHexCoordinate>();
    [ReadOnly] public EActionState CurrentState { get; protected set; } = EActionState.None;
    
    // Safe handle to avoid infinite action state !
    float safehandle = 10f;
    float _elapsedTime = 0f;
    
    public Texture2D GetCursorIcon(int index = 0) =>_actionCursorIcon != null && index <= _actionCursorIcon.Length - 1 ? _actionCursorIcon[index] : null;
    
    public GAction(){}
    public GAction(GPawn inLinkedPawn, GCell inTargetCell, Action inOnActionStarted = null,
        Action inOnActionFinished = null)
    {
        linkedPawn = inLinkedPawn;
        targetCell = inTargetCell;
        OnActionStarted += inOnActionStarted;
        OnActionFinished += inOnActionFinished;
    }

    public abstract EZoneActionType GetHighlightActionType();
    
    /// <summary>
    /// Create a new instance of the action with the same parameters, Override this for Add Params
    /// </summary>
    /// <returns>Cloned action</returns>
    public virtual GAction CloneAction()
    {
        GAction clone = (GAction)Activator.CreateInstance(this.GetType());
        clone.linkedPawn = linkedPawn;
        clone.targetCell = targetCell;
        clone.CurrentState = CurrentState;
        clone.OnActionStarted = OnActionStarted;
        clone.OnActionFinished = OnActionFinished;
        clone.actionIcon = actionIcon;
        clone.actionLabel = actionLabel;
        clone.centerCursorOffset = centerCursorOffset;
        clone._actionCursorIcon = _actionCursorIcon;
        return clone;
    }
    
    public virtual GAction SetTargetCell(GCell inTargetCell)
    {
        targetCell = inTargetCell;
        return this;
    }

    /** Call When the Player Selects the Action */
    public virtual void OnSelectedAction()
    {
    }
    
    /** Call When the Player Unselects the Action */
    public virtual void OnUnselectedAction()
    {
    }
    
    public virtual void InitAction(GPawn inLinkedPawn)
    {
        linkedPawn = inLinkedPawn;
    }
    
    public abstract List<GCell> Previsualisation(in GActionContext previsuContext);
    
    /// <summary>
    /// pre-process all the logic of the action. This will update the grid before any visuals. This will also trigger the pre-process of any reaction if any are needed.
    /// </summary>
    /// <param name="context">Collection of parameters of generic type. Useful for reaction but not used for actions</param>
    public virtual void PreProcess(GActionContext context = null)
    {
        CurrentState = EActionState.PreProcessing;
    }
    
    /// <summary>
    /// Visual impact of the action. May trigger reactions start.
    /// </summary>
    public virtual void Start_Action()
    {
        _elapsedTime = 0f;
        CurrentState = EActionState.InProgress;
        OnActionStarted?.Invoke();
    }

    /// <summary>
    /// Visual impact of the action. May trigger reactions start.
    /// </summary>
    /// <param name="delta"></param>
    public virtual void Update_Action(float delta)
    {
        _elapsedTime += delta;
        if (_elapsedTime > safehandle)
        {
            Debug.LogWarning($"Action {this.GetType().Name} exceeded safe handle time limit. Forcing end of action.");
            End_Action();
        }
    }

    /// <summary>
    /// Visual impact of the action. May trigger reactions start.
    /// </summary>
    public virtual void End_Action()
    {
        CurrentState = EActionState.Finished;
        OnActionFinished?.Invoke();
    }

    /// <summary>
    /// Get all cells onto which this action can be played.
    /// </summary>
    /// <returns>Array of <c>Gcell</c> of all valid cells</returns>
    public virtual GHexCoordinate[] GetValidCells() { return validCells; }
    
    /// <summary>
    /// Check if a given cell is part of the valid cells of this action (Valid cells must be cached beforehand for any grid changes with <see cref="GetValidCells"/>)
    /// </summary>
    /// <param name="cell">Cell to check the validity of</param>
    /// <returns>True if cell is valid for this action</returns>
    public bool IsValidCell(GHexCoordinate cell) { return validCells.Contains(cell); }

    /// <summary>
    /// Add Previsualisation curve to <see cref="context"/>
    /// </summary>
    protected void AddPrevisualisationCurve(in GActionContext context, Vector3 startPosition, Vector3 endPosition,
        GActionPrevisualisationCurveData data)
    {
        int resolution = data.previsuCurveResolution;
        Vector3[] curve = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float progress = (float)i / resolution;
            Vector3 position = Vector3.Lerp(startPosition + data.startHeightOffset * Vector3.up, endPosition + data.endHeightOffset * Vector3.up, data.previsuPositionsCurve.Evaluate(progress));
            float height = Mathf.Lerp(position.y, data.previsuCurveMaxHeight, data.previsuHeightCurve.Evaluate(progress));
            position.y = height;
            curve[i] = position;
        }
        
        if (context.Has(GActionContext.PREVISU_POS_STRING))
        {
            List<Vector3[]> curves = context.Get<List<Vector3[]>>(GActionContext.PREVISU_POS_STRING);
            curves.Add(curve);
        }
        else
        {
            var curves = new List<Vector3[]>(1) { curve };
            context.Set(GActionContext.PREVISU_POS_STRING, curves);
        }
    }
}