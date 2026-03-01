using System;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
public class GStartMenu : MonoBehaviour
{
    [SerializeField]
    private Button _startButton, _continueButton, _tutorialButton, _optionsButton, _quitMenuOpenButton, _quitButton, _cancelQuitMenuButtton;

    [SerializeField]
    private GameObject _gameOverValidationPanel;

    private void Start()
    {
        _startButton.onClick.AddListener(() =>
        {
            GGameManager.Instance.isLoadingTutorial = false;
            GGameManager.Instance.isLoadingNewSave = true;
            GGameManager.Instance.ChangeState(EMacroStates.Map_Select);
        });
        
        _continueButton.onClick.AddListener(() =>
        {
            GGameManager.Instance.isLoadingTutorial = false;
            GGameManager.Instance.isLoadingNewSave = false;
            GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen);
        });

        _tutorialButton.onClick.AddListener(() =>
        {
            GGameManager.Instance.isLoadingTutorial = true;
            GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen);
        });

        _optionsButton.onClick.AddListener(()=>GGameManager.Instance.ChangeState(EMacroStates.Options));
        _quitMenuOpenButton.onClick.AddListener(()=>ActivateQuitValidationMenu(true));
        _quitButton.onClick.AddListener(()=> Application.Quit());
        _cancelQuitMenuButtton.onClick.AddListener(()=>ActivateQuitValidationMenu(false));
    }

    void OnEnable()
    {
        // TODO : Reenable the continue button once the save system is updated with all the new features
        _continueButton.interactable = GSaveManager.Instance.IsGameStateSaveFileCreated();
    }

    public void ActivateQuitValidationMenu(bool toActivate)
    {
        _gameOverValidationPanel.SetActive(toActivate);
    }
}

