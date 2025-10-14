
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ETurnState
{
    NotStarted,
    InProgress,
    Finished
}

public class GTurnBaseManager : GSingleton<GTurnBaseManager>
{
    /** Manager The Turn Order */
    public GController _currentTurnController { get; private set; }

    public bool isActionPlaying
    {
        get => _actionsInProgress.Count > 0;
    }
    
    private List<GAction> _actionsInProgress = new List<GAction>();
    
    /** Queue Order of The Entity currently in fight */ 
    private List<GController> _turnOrderControllerQueue = new List<GController>();
    
    /** All the Entity in the Scene */
    private List<GController> _controllerList = new List<GController>();
    
    [SerializeField, BoxGroup("Dev Settings"), Tooltip("Time to wait before forcing the end of the turn when action is playing")]
    private float _safeTimeHandle = 5f;

    [SerializeField, BoxGroup("Dev Settings"), Tooltip("Speed Multiplier of the Action")]
    private float _actionSpeed = 1f;

    public ETurnState _currentTurnState { get; private set; }

    /** Register an Controller to the Turn Base Manager */
    public void RegisterController(GController Controller)
    {
        if (enabled && !_turnOrderControllerQueue.Contains(Controller))
        {
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
    public void RequestEndTurn(GController Controller)
    {
        Debug.Log("RequestEndTurn of " + _currentTurnController + " by " + Controller?.ToString());
        
        if (Controller != null && Controller != _currentTurnController)
        {
            Debug.LogWarning($" {Controller} Trying to End Turn of {_currentTurnController}, but it's not his turn.");
            return;
        }
        
        // isTurnActive = false;
        _turnOrderControllerQueue.Add(_currentTurnController);
        StartCoroutine(ProcessEndTurn());
    }
    
    public bool TryPlayAction(GAction ActionToPlay, bool IsReaction) 
    { 
        if (!IsReaction && isActionPlaying) return false;
        
        // TODO : Check if it's the good Method ! 
        GAction ActionInstance = ActionToPlay.Duplicate();
        
        // TODO A check modifs en fonction du return ! 
        ActionInstance.PreProcess();
        
        ActionInstance.Start_Action();
        _actionsInProgress.Add(ActionInstance);
        return true;
    }
    
    private void StartFight() 
    {
        CreateQueue();
        StartTurn();
    }
    
    /** Create the Queue based on Rule (Actually : player is first, then IA) */
    private void CreateQueue()
    {
        var orderedEntities = _controllerList.OrderBy(entity => entity is GPlayerController ? 1 : 0).ToList();
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
        
        _currentTurnController = _turnOrderControllerQueue.First();
        _turnOrderControllerQueue.RemoveAt(0);
        
        _currentTurnController.StartTurn();
        _currentTurnState = ETurnState.InProgress;
    }
    
    protected override void Awake()
    {
        base.Awake();
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
            if (action.CurrentState == EActionState.Finished)
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
            StartFight();
        } else Debug.LogWarning("No Controller to start the Turn Base Manager");
    }

    private void OnDisable()
    {
        _currentTurnState = ETurnState.NotStarted;
        _turnOrderControllerQueue.Clear();
        _actionsInProgress.Clear();
    }
    
    IEnumerator ProcessEndTurn()
    {
        float startTime = Time.time;
        yield return new WaitUntil(() => !isActionPlaying || Time.time > startTime + _safeTimeHandle);
        
        _currentTurnState = ETurnState.Finished;
        StartTurn();
    }
}

