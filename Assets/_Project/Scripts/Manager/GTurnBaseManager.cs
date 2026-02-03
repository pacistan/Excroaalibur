using _Project.Scripts.Wave;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GWaveComponent))]
public class GTurnBaseManager : GSingleton<GTurnBaseManager>
{
    public enum ETurnState
    {
        NotStarted,
        InProgress,
        Finished
    }
    
    public event Action<GAction, GController> OnActionPlayed; 
    public event Action<GController> OnStartControllerTurn;
    public event Action<int> OnPrePlayerTurn;
    public event Action<int> OnPostPlayerTurn;
    public event Action<GController> OnUnregisterController;
    
    /** Manager The Turn Order */
    [field: SerializeField, ReadOnly, HideInEditorMode, BoxGroup("Turn")]
    public GController currentTurnController { get; private set; }

    public bool isActionPlaying
    {
        get => _actionsInProgress.Count > 0;
    }
    
    public ETurnState _currentTurnState { get; private set; }

    public int EnemiesCount => _turnOrderControllerQueue.Count(entity => entity is GAIController);
    
    /** Queue Order of The Entity currently in fight */ 
    [SerializeReference, ReadOnly, HideInEditorMode, BoxGroup("Actions")]
    private List<GAction> _actionsInProgress = new List<GAction>();
    
    /** Queue Order of The Entity currently in fight */ 
    [SerializeField, ReadOnly, HideInEditorMode, BoxGroup("Turn")]
    private List<GController> _turnOrderControllerQueue = new List<GController>();
    
    /** All the Entity in the Scene */
    private List<GController> _controllerList = new List<GController>();

    [SerializeField, BoxGroup("Dev Settings"), Tooltip("Speed Multiplier of the Action")]
    private float _actionSpeed = 1f;
    
    [SerializeField, HideInEditorMode, ReadOnly, Tooltip("Number of Turn elapsed since the start of the Fight"), BoxGroup("Turn")]
    private int _turnCount = 0;
    
    // Wave Management !
    [SerializeField, BoxGroup("WaveManagement"), Tooltip("Data of the Waves to use in the Turn Base Manager")]
    private bool _hasWaves = true;
    
    [SerializeField, BoxGroup("WaveManagement"), ShowIf("_hasWaves"), Tooltip("Current Wave Data to use for this Scene")]
    private WaveData _currentWaveData;
    
    private GWaveComponent _waveComponent;
    
    /** Get the Current Score (Number of completed waves) */
    public int GetScore() => _waveComponent.GetWaveCount();
    
    public void SetWaveCount(int waveCount) => _waveComponent.SetWaveCount(waveCount);
    
    public WaveData GetCurrentWaveData() => _hasWaves ? _currentWaveData : null;

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
        if (_turnOrderControllerQueue.Contains(Controller) )
            _turnOrderControllerQueue.Remove(Controller);

        if (_controllerList.Contains(Controller))
        {
            _controllerList.Remove(Controller);
            OnUnregisterController?.Invoke(Controller);
        }

        // If No More Enemies, Check Next Wave !
        if (EnemiesCount == 0) {
            _waveComponent.CheckNextWave(_turnCount);
        }
    }
    
    /** Request to End the Turn of the Current Controller,
     *  Set controller to null to Force
     */
    public void RequestEndTurn(GController Controller ,bool reenterQueue = true)
    {
        Debug.Log("RequestEndTurn of " + currentTurnController + " by " + Controller?.ToString());
        
        if (Controller != null && Controller != currentTurnController)
        {
            Debug.LogWarning($" {Controller} Trying to End Turn of {currentTurnController}, but it's not his turn.");
            return;
        }
        
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
        OnActionPlayed?.Invoke(ActionInstance, currentTurnController);
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
        OnActionPlayed?.Invoke(ReactionToStart, currentTurnController);
    }
    
    /** Create the Queue based on Rule (Actually : player is first, then IA) */
    private void CreateQueue(bool playerLast = false)
    {
        _turnOrderControllerQueue.Clear();
        //  _controllerList = _controllerList.Where(controller => controller != null).ToList(); // Clean Null References 
        var orderedEntities = _controllerList.OrderBy(entity =>
        {
            if (entity is GPlayerController) return playerLast ? int.MaxValue : int.MinValue;
            if (entity.GetComponent<GPawn>() == null) return int.MaxValue - 1;
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
        if (_turnOrderControllerQueue.Count == 0 && _waveComponent == null)
        {
            Debug.LogWarning("Turn Order Queue is empty and WaveManager is null. Force Disable the Turn Base Manager.");
            enabled = false;
            return;
        }
        
        if (_turnOrderControllerQueue.First() is GPlayerController) // Before Player Turn
        {
            OnPrePlayerTurn?.Invoke(_turnCount);
            _turnCount++;
            
            if (EnemiesCount == 0) // Needed for First Wave Preview
            {
                _waveComponent.CheckNextWave(_turnCount);
            }
        }

        if (_turnOrderControllerQueue.Count >= 0)  // if Queue Is empty, keep the currentTurnController 
        {
            currentTurnController = _turnOrderControllerQueue.First();
            _turnOrderControllerQueue.RemoveAt(0);
        }
        
        _currentTurnState = ETurnState.InProgress;
        OnStartControllerTurn?.Invoke(currentTurnController);
        currentTurnController.StartTurn();
    }
    
    protected override void Awake()
    {
        base.Awake();
        enabled = false;
        
        _turnOrderControllerQueue.Clear();
        _actionsInProgress.Clear();
        _currentTurnState = ETurnState.NotStarted;
        
        TryGetComponent(out _waveComponent);
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
        
        GGameManager.Instance.OnChangeMacroStateEvent += OnChangeMacroStateCallback;
    }

    private void OnDisable()
    {
        _currentTurnState = ETurnState.NotStarted;
        _turnOrderControllerQueue.Clear();
        _actionsInProgress.Clear();
        GGameManager.Instance.OnChangeMacroStateEvent -= OnChangeMacroStateCallback;
    }
    
    private IEnumerator StartFight()
    {
        yield return new WaitForEndOfFrame(); // Wait for all Awake / Start to be called
        
        _turnCount = 1; // Reset Turn Count ! 
        CreateQueue();
        StartTurn();
        yield return null;
    }
    
    IEnumerator ProcessEndTurn()
    {
        GSaveManager.Instance.SerializeToJson(); // Auto Save at the end of each Turn !
        yield return new WaitUntil(() => !isActionPlaying); 
        
        _currentTurnState = ETurnState.Finished;
        
        currentTurnController?.EndTurn();
        
        if (currentTurnController is GPlayerController) // After Player Turn
        {
            OnPostPlayerTurn?.Invoke(_turnCount);
            
            if (EnemiesCount == 0)  
                _waveComponent.CheckNextWave(_turnCount); // Safe Check, but should not be needed here !

            if (_waveComponent.HasCreateNextWave())
                GUpgradeManager.Instance.StartUpgradeSequence();
            
            yield return new WaitUntil(() => !GUpgradeManager.Instance.IsUpgradeSelectionInProgress());
            
            if (_waveComponent.HasEnemiesToSpawn())
                _waveComponent.SpawnNextWave();
            
            CreateQueue(true);
            yield return new WaitUntil(() => !_waveComponent.IsSpawningInProgress());
        }
        
        StartTurn();
        yield return null;
    }
    
    
    private void OnChangeMacroStateCallback(EMacroStates newState, EMacroStates oldState)
    {
        if (newState == EMacroStates.Play && oldState == EMacroStates.LoadingScreen)
        {
            _actionsInProgress.Clear();
            GHudManager.Instance.playMenu.SetWaveNumberText(GetScore() - 1);
        }
    }
}

