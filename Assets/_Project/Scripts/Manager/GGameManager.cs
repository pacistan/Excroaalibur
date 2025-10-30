using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public enum EMacroStates { Start, Options, Pause, Play, End, None }


/* Responsable de la gestion globale du Jeu, de l'activation de potentiel Manager etc...*/
public class GGameManager: GSingleton<GGameManager>
{
    public event Action<EMacroStates, EMacroStates> OnChangeMacroStateEvent;
    public event Action<bool> OnPauseEvent;
    
    [field: HideInPlayMode, SerializeField]
    private EMacroStates _startState;

    [field : HideInEditorMode, ReadOnly, SerializeField]
    public EMacroStates currentState { get; private set; } = EMacroStates.None;

    [field : HideInEditorMode, ReadOnly, SerializeField]
    public EMacroStates previousState { get; private set; } = EMacroStates.None;
    
    [ReadOnly]
    public bool isLoadingTutorial;

#if UNITY_EDITOR
    [SerializeField, FoldoutGroup("SceneToLoad")]
    private UnityEditor.SceneAsset[] _loadableSceneAssets;

    [SerializeField, FoldoutGroup("SceneToLoad")]
    private UnityEditor.SceneAsset[] _tutorialSceneAssets;
#endif
    [SerializeField, FoldoutGroup("SceneToLoad"), ReadOnly]
    private string[] _loadableScenes;
    [SerializeField, FoldoutGroup("SceneToLoad")] 
    private int _currentSceneToLoadIndex;

    [Space(10), LabelText("Tutorial")]
    [SerializeField, FoldoutGroup("SceneToLoad"), ReadOnly]
    private string[] _loadableTutorialScenes;
    [SerializeField, FoldoutGroup("SceneToLoad"), ReadOnly]
    private int _currentTutorialSceneToLoadIndex = 0;

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
    
    public bool IsMenuActive(EMacroStates menu) => menu == currentState;

    public void ChangeState(EMacroStates newMenuState)
    {
        if (newMenuState == currentState)
        {
            Debug.LogWarning("Called to change to already active menu");
            return;
        }
        StartCoroutine(ChangeStateCoroutine(newMenuState)); 
    }

    public void ReloadScene()
    {
        StartCoroutine(ChangeStateCoroutine(EMacroStates.Play, true)); 
    }

    public IEnumerator ChangeStateCoroutine(EMacroStates newState, bool reloadScene = false)
    {
        previousState = currentState;
        currentState = newState;
        OnMenuExit();
        OnMenuEnter();
        OnChangeMacroStateEvent?.Invoke(currentState, previousState);
        if (((previousState == EMacroStates.End || previousState == EMacroStates.Start) && newState == EMacroStates.Play) || reloadScene)
        {
            string sceneName = reloadScene ? SceneManager.GetActiveScene().name : // Is Reload ?
                isLoadingTutorial ?  // Is Tutorial ?
                    _loadableTutorialScenes[Mathf.Min(_currentTutorialSceneToLoadIndex, _loadableTutorialScenes.Length - 1)] :
                    _loadableScenes[Mathf.Min(_currentSceneToLoadIndex, _loadableScenes.Length - 1)];
            
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;
            while (!asyncOperation.isDone)
            {

                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }
            yield return new WaitForSecondsRealtime(1);
        }

    }

    private void OnMenuEnter()
    {
        switch (currentState)
        {
            case EMacroStates.Start:
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
                PauseGameTime(false, true, 2);
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
        if (isLerp)
        {
            if (toPause)
                DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, time).SetEase(Ease.InQuad).SetUpdate(true);
            else
                DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, time).SetEase(Ease.InQuad).SetUpdate(true);
        }
        else
        {
            if (toPause)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;
        }
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
    
    // TODO : CallBack this ! 
    void UpdateIntensity(int waveNumber)
    {
        if (waveNumber < 1) return; 
        _intensity = Mathf.Min(waveNumber / _intensityWaveFactor, _maxIntensity);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Intensity", _intensity);
    }
    
    /** Start the game over sequence */
    public void StartGameOver()
    {
        Debug.Log("Game Over !");
        GTurnBaseManager.Instance.enabled = false; // Disable turn ! 
        // TODO : Disable Game Controls on grid ? 
    }
    
    protected override void Awake()
    {
        _pauseAction = InputSystem.actions.FindAction("Pause");
        base.Awake();
    }
    void Start()
    {
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
        if(_loadableSceneAssets == null || _loadableSceneAssets.Length == 0)
        {
            _loadableScenes = null;
            return;
        }
        _loadableScenes = new string[_loadableSceneAssets.Length];
        for (int i = 0; i < _loadableSceneAssets.Length; i++)
        {
            _loadableScenes[i] = _loadableSceneAssets[i].name;
        }
    }
#endif
}
