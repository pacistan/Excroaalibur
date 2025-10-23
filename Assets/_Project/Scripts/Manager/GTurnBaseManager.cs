using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GTurnBaseManager : GSingleton<GTurnBaseManager>
{
    public enum ETurnState
    {
        NotStarted,
        InProgress,
        Finished
    }
    
    public event Action<GAction, GController> actionPlayed; 
    public event Action<GController> startControllerTurn;
    
    /** Manager The Turn Order */
    [field: SerializeField, ReadOnly, HideInEditorMode, BoxGroup("Turn")]
    public GController currentTurnController { get; private set; }

    public bool isActionPlaying
    {
        get => _actionsInProgress.Count > 0;
    }
    
    /** Queue Order of The Entity currently in fight */ 
    [SerializeField, ReadOnly, HideInEditorMode, BoxGroup("Actions")]
    private List<GAction> _actionsInProgress = new List<GAction>();
    
    /** Queue Order of The Entity currently in fight */ 
    [SerializeField, ReadOnly, HideInEditorMode, BoxGroup("Turn")]
    private List<GController> _turnOrderControllerQueue = new List<GController>();
    
    /** All the Entity in the Scene */
    private List<GController> _controllerList = new List<GController>();
    
    [SerializeField, BoxGroup("Dev Settings"), Tooltip("Time to wait before forcing the end of the turn when action is playing")]
    private float _safeTimeHandle = 5f;

    [SerializeField, BoxGroup("Dev Settings"), Tooltip("Speed Multiplier of the Action")]
    private float _actionSpeed = 1f;
    
    [SerializeField, HideInEditorMode, ReadOnly, Tooltip("Number of Turn elapsed since the start of the Fight"), BoxGroup("Turn")]
    private int _turnCount = 0;
    
    private WaitUntil _waitForTurn = null;
    
    public ETurnState _currentTurnState { get; private set; }

    /** Register an Controller to the Turn Base Manager */
    public void RegisterController(GController Controller)
    {
        if (enabled && !_turnOrderControllerQueue.Contains(Controller))
        {
            _controllerList.Add(Controller);
            _turnOrderControllerQueue.Add(Controller);
        } 
        else if (!_controllerList.Contains(Controller))
        {
            _controllerList.Add(Controller);
        }
    }
    
    /** Unregister a Controller from the Turn Base Manager */
    public void UnregisterController(GController Controller)
    {
        if (!_turnOrderControllerQueue.Contains(Controller))
        {
            Debug.LogWarning("Trying to Unregister a Controller that is not longer in the Turn Order Queue");
            return;
        }
        
        _turnOrderControllerQueue.Remove(Controller);
        _controllerList.Remove(Controller);
    }
    
    /** Request to End the Turn of the Current Controller,
     *  Set controller to null to Force
     */
    public void RequestEndTurn(GController Controller, bool reenterQueue = true)
    {
        Debug.Log("RequestEndTurn of " + currentTurnController + " by " + Controller?.ToString());
        
        if (Controller != null && Controller != currentTurnController)
        {
            Debug.LogWarning($" {Controller} Trying to End Turn of {currentTurnController}, but it's not his turn.");
            return;
        }

        Controller.EndTurn();
        // isTurnActive = false;
        if (reenterQueue)
        {
            _turnOrderControllerQueue.Add(currentTurnController);
        }
        StartCoroutine(ProcessEndTurn());
    }
    
    public bool TryPlayAction(GAction ActionToPlay) 
    { 
        if (isActionPlaying) return false;
        
        GAction ActionInstance = ActionToPlay.CloneAction();
        
        ActionInstance.PreProcess();

        ActionInstance.Start_Action();
        actionPlayed?.Invoke(ActionInstance, currentTurnController);
        
        _actionsInProgress.Add(ActionInstance);
        return true;
    }

    /// Preprocess a reaction without starting it with the associated context
    /// <param name="ActionContext">collection of parameters of generic type to pass to the reaction from the action</param>
    public bool PreProcessReaction(GAction ReactionToPlay, GActionContext ActionContext)
    {
        ReactionToPlay.PreProcess(ActionContext);
        return true;
    }

    /// <summary>
    /// Start the reaction if it's preprocess is over, to use only for visuals synced with the corresponding instigator action
    /// </summary>
    /// <remarks>all reaction should be start by the action.</remarks>
    public void TryStartReaction(GAction ReactionToStart)
    {
        if (ReactionToStart == null) return;
        if (ReactionToStart.CurrentState != GAction.EActionState.PreProcessing) return;
        ReactionToStart.Start_Action();
        _actionsInProgress.Add(ReactionToStart);
        actionPlayed?.Invoke(ReactionToStart, currentTurnController);
    }
    
    /** Create the Queue based on Rule (Actually : player is first, then IA) */
    private void CreateQueue(bool playerLast = false)
    {
        _turnOrderControllerQueue.Clear();
        var orderedEntities = _controllerList.OrderBy(entity =>
        {
            if (entity is GPlayerController) return playerLast ? int.MaxValue : int.MinValue;
            int closestCrownDistance;
            GGridObjectRegistry.GetClosestObjectOfType<GCrown>(entity.GetComponent<GPawn>().GetCell(), out closestCrownDistance, true);
            return closestCrownDistance;
        }).ToList();
        
        foreach (var entity in orderedEntities) 
        {
            _turnOrderControllerQueue.Add(entity);
        }
    }
    
    /** Start the Turn of the first Entity in the Queue */
    private void StartTurn()
    {
        if (_turnOrderControllerQueue.Count == 0)
        {
            Debug.LogWarning("Turn Order Queue is empty. Force Disable the Turn Base Manager.");
            enabled = false;
            return;
        }
        
        currentTurnController = _turnOrderControllerQueue.First();
        _turnOrderControllerQueue.RemoveAt(0);
        
        currentTurnController.StartTurn();
        _currentTurnState = ETurnState.InProgress;
        startControllerTurn?.Invoke(currentTurnController);
    }
    
    protected override void Awake()
    {
        base.Awake();
        _waitForTurn = new WaitUntil(() => !isActionPlaying);
        _turnOrderControllerQueue.Clear();
        _actionsInProgress.Clear();
        // enabled = false;
        _currentTurnState = ETurnState.NotStarted;
    }

    private void Update()
    {
        if (_currentTurnState != ETurnState.InProgress) return;
        
        for (int i = _actionsInProgress.Count - 1; i >= 0; i--)
        {
            GAction action = _actionsInProgress[i];
            if (action.CurrentState == GAction.EActionState.Finished)
            {
                _actionsInProgress.RemoveAt(i);
                continue;
            }
            
            action.Update_Action(Time.deltaTime * _actionSpeed);
        }
    }

    private void OnEnable()
    {
        _controllerList = FindObjectsByType<GController>(FindObjectsSortMode.None).ToList();
        if (_controllerList.Count > 0)
        {
            StartCoroutine(StartFight());
        } else Debug.LogWarning("No Controller to start the Turn Base Manager");
    }

    private void OnDisable()
    {
        _currentTurnState = ETurnState.NotStarted;
        _turnOrderControllerQueue.Clear();
        _actionsInProgress.Clear();
    }
    
    // TODO : Check 
    private IEnumerator StartFight()
    {
        yield return new WaitForEndOfFrame(); // Wait for all Awake / Start to be called
        
        _turnCount = 1; // Reset Turn Count ! 
        CreateQueue();
        WaveManager.Instance.CheckNextWave(_turnCount);
        StartTurn();
        yield return null;
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator ProcessEndTurn()
    {
        float startTime = Time.time;
        /*while (isActionPlaying && Time.time <= startTime + _safeTimeHandle)
        {
            yield return null;
        }*/
        yield return _waitForTurn; /*=> !isActionPlaying || Time.time > startTime + _safeTimeHandle);*/
        
        _currentTurnState = ETurnState.Finished;
        
        if (_turnOrderControllerQueue.First() is GPlayerController) // Before Player Turn
        {
            CreateQueue();
            _turnCount++;
            WaveManager.Instance.CheckNextWave(_turnCount);
        }
        else if (currentTurnController is GPlayerController) // After Player Turn
        {
            if (WaveManager.Instance.HasNextWave())
            {
                WaveManager.Instance.SpawnNextWave();
            }
            
            CreateQueue(true);
        }
        
        StartTurn();
        yield return null;
    }
}

