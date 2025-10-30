using UnityEngine;
using UnityEngine.UI;

public class GStartMenu : MonoBehaviour
{
    [SerializeField]
    private Button _startButton, _tutorialButton, _optionsButton, _quitMenuOpenButton, _quitButton, _cancelQuitMenuButtton;

    [SerializeField]
    private GameObject _gameOverValidationPanel;

    private void Start()
    {
        _startButton.onClick.AddListener(() =>
        {
            GGameManager.Instance.isLoadingTutorial = false;
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

    public void ActivateQuitValidationMenu(bool toActivate)
    {
        _gameOverValidationPanel.SetActive(toActivate);
    }
}

