using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public enum EMacroStates { Start, Options, Upgrade_Select_Card, Upgrade_Select_Character, Pause, Play, End, LoadingScreen, Map_Select, None }

/* Responsable de la gestion globale du Jeu, de l'activation de potentiel Manager etc...*/
public class GGameManager: GSingleton<GGameManager>
{
    /// <summary>
    /// 1 - current 2 - previous
    /// </summary>
    public event Action<EMacroStates, EMacroStates> OnChangeMacroStateEvent;
    public event Action<bool> OnPauseEvent;
    
    public event Action OnNewSceneLoaded;
    
    [SerializeField, HideInPlayMode]
    private EMacroStates _startState;

    [field : HideInEditorMode, ReadOnly, SerializeField]
    public EMacroStates currentState { get; private set; } = EMacroStates.None;

    [field : HideInEditorMode, ReadOnly, SerializeField]
    public EMacroStates previousState { get; private set; } = EMacroStates.None;
    
    [ReadOnly]
    public bool isLoadingTutorial;

    [ReadOnly]
    public bool isLoadingNewSave = true;
    
    [SerializeField]
    public int _localizationId;

    [field : SerializeField, FoldoutGroup("SceneToLoad"), ReadOnly, HideInEditorMode]
    public GGameStateSaveData gameStateSaveData { get; private set; }

#if UNITY_EDITOR
    [SerializeField, FoldoutGroup("SceneToLoad")]
    private UnityEditor.SceneAsset[] _tutorialSceneAssets;
#endif

    [SerializeField]
    GCommonInstantiationData _commonInstantiationData;

    [field : SerializeField, FoldoutGroup("SceneToLoad"), ReadOnly, HideInEditorMode]
    public  GSOMapData loadableMapData { get; private set; }
    
    [Space(10), LabelText("Tutorial")]
    [SerializeField, FoldoutGroup("SceneToLoad"), ReadOnly]
    private string[] _loadableTutorialScenes;
    
    [SerializeField, FoldoutGroup("SceneToLoad"), ReadOnly]
    public int _currentTutorialSceneToLoadIndex = 0 ;
    
    // Debug For Vincent 
    [FoldoutGroup("Debug")]
    [SerializeField, FoldoutGroup("Debug/SlowMotion")]
    private float duration = 0.5f;
    
    [SerializeField, FoldoutGroup("Debug/SlowMotion")]
    private float timeScale = 0.1f;
    // End Debug For Vincent 

    [SerializeField]
    EventReference _musicEvent;
    EventInstance _musicInstance;
    EventInstance _pauseSnapshot;

    InputAction _pauseAction;
    InputAction _menuInput;

    [HideInInspector]
    public bool isGamePaused;
    
    private int _intensity = 0;
    private int _maxIntensity = 10;
    
    [SerializeField, Tooltip("Number of waves for intensity increases")]
    private int _intensityWaveFactor = 1;

    [SerializeField]
    float _loadSceneForceDuration = 2f;
    
    public GPlayerController playerController;
    
    Coroutine _SlongMoCoroutine;
    
    public void SetSceneToLoad(GSOMapData mapToLoad)
    {
        loadableMapData = mapToLoad;
    }    
    
    public bool IsMenuActive(EMacroStates menu) => menu == currentState;

    public void ChangeState(EMacroStates newState)
    {
        if (newState == currentState)
        {
            Debug.LogWarning("Called to change to already active menu");
            return;
        }
        //GDebug.Log(ELogType.Andre_Channel_2,newState.ToString());
        previousState = currentState;
        currentState = newState;
        OnMenuExit();
        OnMenuEnter();
        OnChangeMacroStateEvent?.Invoke(currentState, previousState);
    }

    public void LoadScene()
    {
        string sceneName = "";
        if (isLoadingTutorial)
        {
            if(_currentTutorialSceneToLoadIndex > _loadableTutorialScenes.Length - 1)
            {
                ChangeState(EMacroStates.Start);
                return;
            }
            sceneName = _loadableTutorialScenes[Mathf.Min(_currentTutorialSceneToLoadIndex, _loadableTutorialScenes.Length - 1)];
        }
        else
        {
            sceneName = loadableMapData.SceneName;
        }
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        float forcedTimer = 0;
        while (!asyncOperation.isDone || forcedTimer < 1f)
        {
            GHudManager.Instance.loadingScreenMenu.UpdateProgress(asyncOperation.progress);
            forcedTimer += Time.unscaledDeltaTime / _loadSceneForceDuration;
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        yield return null;
        if (!isLoadingNewSave)
        {
            UpdateGameStateWithSavedData();
        }
        yield return null;
        
        OnNewSceneLoaded?.Invoke();

        GTurnBaseManager.Instance.enabled = true;
        
        ChangeState(EMacroStates.Play);
    }

    private void UpdateGameStateWithSavedData()
    {
        var data = gameStateSaveData;
        GCrown crown = GGridObjectRegistry.GetItems<GCrown>()[0];
        GPawn[] players = GGridObjectRegistry.GetItemsByPredicate<GPawn>(pawn => pawn.data.isPlayer).ToArray();

        GTurnBaseManager.Instance.SetWaveCount(data.waveNumber);
        GTurnBaseManager.Instance.SetCurrentWaveEnemyCapReached(data.waveEnemyCapReach);

        // Player Stuff
        {
            for (var i = 0; i < players.Length; i++)
            {
                GPawn player = players.ElementAt(i);
                var playerData = data.players.ElementAt(i);

                if (playerData.stunTurnNumber > 0) player.Stun(playerData.stunTurnNumber);

                GCell playerCell = GGridManager.Instance.GetCell(new Vector2Int(playerData.xCoordinate, playerData.yCoordinate));
                if (playerCell != player.GetCell())
                {
                    playerCell.SetGridObject(player, true);
                }

                if (playerData.upgradeGuids != null)
                {
                    foreach (var upgradeGuid in playerData.upgradeGuids)
                    {
                        GSOUpgrade upgrade = GUpgradeManager.Instance.GetUpgradeWithGuid(upgradeGuid);
                        if (upgrade != null)
                        {
                            player.AddUpgrade(upgrade);
                        }
                    }
                }

            }
        }

        // Ennemy Stuff
        {
            for (int i = 0; i < data.ennemies.Length; i++)
            {
                var enemyData = data.ennemies.ElementAt(i);
                EGridObjectType objectType = (EGridObjectType)Enum.Parse(typeof(EGridObjectType), enemyData.ennemyType);
                GGridObject gridObjectPrefab = _commonInstantiationData.objectTypeData[objectType];
                GPawn pawnPrefab = gridObjectPrefab as GPawn;
                if (pawnPrefab == null)
                {
                    Debug.LogError($"Could not find pawn for type {enemyData.ennemyType}");
                    continue;
                }

                GPawn pawn = GameObject.Instantiate(pawnPrefab);
                GCell ennemyCell = GGridManager.Instance.GetCell(new Vector2Int(enemyData.xCoordinate, enemyData.yCoordinate));
                ennemyCell.SetGridObject(pawn, true);
                if (enemyData.stunTurnNumber > 0) pawn.Stun(enemyData.stunTurnNumber);

                if(enemyData.upgradeGuids != null) 
                { 
                    foreach (var upgradeGuid in enemyData.upgradeGuids) 
                    { 
                        GSOUpgrade upgrade = GTurnBaseManager.Instance.GetCurrentWaveData().GetUpgradeWithGuid(upgradeGuid); 
                        if (upgrade != null) 
                        { 
                            pawn.AddUpgrade(upgrade);
                        } 
                    } 
                }

                pawn.SetHp(enemyData.hp);
            }
        }

        // Crown Stuff
        {
            crown.SetCurrentDamage(data.crownDamage);
            GHudManager.Instance.playMenu.SetCrownDamageText(crown.currentDamage);

            GCell crownCell =
                GGridManager.Instance.GetCell(new Vector2Int(data.xCrownCoordinate, data.yCrownCoordinate));
            GPawn crownPawn = crownCell.gridObject as GPawn;

            if (crown.owner != null)
                crown.owner.ReleaseEquipement(false, false);

            if (crownPawn)
                crownPawn.GiveEquipement(crown, true, false);
            else
                crownCell.SetGridObject(crown);
        }

        GPlayerController playerController = GameObject.FindFirstObjectByType<GPlayerController>();
        playerController.isFirstAction = data.isFirstAction;
        GHudManager.Instance.playMenu.SetWaveNumberText(data.waveNumber);
    }

    private void OnMenuEnter()
    {
        switch (currentState)
        {
            case EMacroStates.Start:
                isLoadingTutorial = false;
                isLoadingTutorial = true;
                _currentTutorialSceneToLoadIndex = 0;
                PauseGameTime(true);
                break;
            case EMacroStates.Options:
                PauseGameTime(true);
                break;
            case EMacroStates.Pause:
                PauseGameTime(true);
                break;
            case EMacroStates.Play:
                PauseGameTime(false);
                break;
            case EMacroStates.End:
                StartGameOver();
                PauseGameTime(true, true, 2);
                break;
            case EMacroStates.LoadingScreen:
                GHudManager.Instance.loadingScreenMenu.ResetProgress();
                break;
        }
    }

    private void OnMenuExit()
    {
        switch (currentState)
        {
            case EMacroStates.Start:
                break;
            case EMacroStates.Pause:
                break;
            case EMacroStates.Play:
                break;
            case EMacroStates.End:
                break;
        }
    }

    public void PauseGameTime(bool toPause, bool isLerp = false, float time = 1)
    {
        isGamePaused = toPause;
        OnPauseEvent?.Invoke(toPause);
        // TODO Clean if necessary
        // if (isLerp)
        // {
        //     if (toPause)
        //         DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, time).SetEase(Ease.InQuad).SetUpdate(true);
        //     else
        //         DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, time).SetEase(Ease.InQuad).SetUpdate(true);
        // }
        // else
        // {
        //     if (toPause)
        //         Time.timeScale = 0f;
        //     else
        //         Time.timeScale = 1f;
        // }
    }

    void StartMusic()
    {
        if (_musicEvent.IsNull) return;
        _pauseSnapshot = RuntimeManager.CreateInstance("snapshot:/Pause");
        _musicInstance = RuntimeManager.CreateInstance(_musicEvent);
        _musicInstance.start();
    }

    private void HandlePauseInput()
    {
        if (_pauseAction != null && _pauseAction.WasPressedThisFrame())
        {
            switch (currentState)
            {
                case EMacroStates.Play:
                    ChangeState(EMacroStates.Pause);
                    break;
                case EMacroStates.Options:
                    if(previousState == EMacroStates.Pause)
                        ChangeState(EMacroStates.Pause);
                    else ChangeState(EMacroStates.Start);
                    break;
                case EMacroStates.Pause:
                    ChangeState(EMacroStates.Play);
                    break;
            }
        }
    }
    
    public void UpdateIntensity(int newIntensity)
    {
        if (newIntensity < 1) return; 
        _intensity = Mathf.Min(newIntensity / _intensityWaveFactor, _maxIntensity);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Intensity", _intensity);
    }
    
    /** Start the game over sequence */
    public void StartGameOver()
    {
        Debug.Log("Game Over !");
        GTurnBaseManager.Instance.enabled = false; // Disable turn ! 
    }
    
    public void TriggerSlowMotion()
    {
        if (_SlongMoCoroutine != null)
            StopCoroutine(_SlongMoCoroutine);
        
        // TODO : add Cam Movement  
        _SlongMoCoroutine = StartCoroutine(SlowMoCoroutine(duration, timeScale));
    }
    
    public void SetGameStateData(GGameStateSaveData gameStateSaveData)
    {
        this.gameStateSaveData = gameStateSaveData;
        GSOMapData mapData = GSaveManager.Instance.GetMapData(gameStateSaveData.mapIndex);
        if (mapData != null)
        {
            loadableMapData = mapData;
        }
    }

    IEnumerator SlowMoCoroutine(float duration, float timeScale)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTimeScale;
    }
    
    protected override void Awake()
    {
        currentState = EMacroStates.None;
        _pauseAction = InputSystem.actions.FindAction("Pause");
        base.Awake();
    }
    
    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
        yield return null;
        
        Locale currentLocal = LocalizationSettings.AvailableLocales.Locales[_localizationId];
        LocalizationSettings.SelectedLocale = currentLocal;
        
        _menuInput = InputSystem.actions.FindAction("Menu");
        StartMusic();
        ChangeState(_startState); 
    }

    void Update()
    {
        HandlePauseInput();
        if (Input.GetKeyDown(KeyCode.V))
        {
            ChangeState(EMacroStates.End);
        }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if(_tutorialSceneAssets != null && _tutorialSceneAssets.Length > 0)
        {
            _loadableTutorialScenes = new string[_tutorialSceneAssets.Length];
            for (int i = 0; i < _tutorialSceneAssets.Length; i++)
            {
                _loadableTutorialScenes[i] = _tutorialSceneAssets[i].name;
            }
        }
        else
            _loadableTutorialScenes = null;
       
    }
#endif
}
