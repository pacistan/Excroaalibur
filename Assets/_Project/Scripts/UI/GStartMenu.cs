using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
public class GStartMenu : MonoBehaviour
{
    [SerializeField]
    private Button _startButton, _continueButton;

    [FormerlySerializedAs("_tutorialButton")]
    [SerializeField]
    private Button _creditsButton;

    [SerializeField]
    private Button _optionsButton, _quitMenuOpenButton, _quitButton, _cancelQuitMenuButtton;

    private RectTransform _creditsRect, _startRect, _continueRect, _optionsRect, _quitRect;

    float _baseButtonY, _hideButtonY;
    
    [SerializeField]
    RectTransform _logoRect;
    
    [SerializeField]
    private GameObject _gameOverValidationPanel;

    CanvasGroup _canvasGroup;
    
    Sequence _sequence;
    
    private void Start()
    {
        _creditsRect = _creditsButton.GetComponent<RectTransform>();
        _startRect = _startButton.GetComponent<RectTransform>();
        _continueRect = _continueButton.GetComponent<RectTransform>();
        _optionsRect = _optionsButton.GetComponent<RectTransform>();
        _quitRect = _quitButton.GetComponent<RectTransform>();
        
        _baseButtonY = -57.5f;
        _hideButtonY = _baseButtonY - 170;
        
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        
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

        _creditsButton.onClick.AddListener(() =>
        {
            GGameManager.Instance.isLoadingTutorial = true;
            GGameManager.Instance.ChangeState(EMacroStates.Credits);
        });

        _optionsButton.onClick.AddListener(()=>GGameManager.Instance.ChangeState(EMacroStates.Options));
        _quitMenuOpenButton.onClick.AddListener(()=>ActivateQuitValidationMenu(true));
        _quitButton.onClick.AddListener(()=> Application.Quit());
        _cancelQuitMenuButtton.onClick.AddListener(()=>ActivateQuitValidationMenu(false));
    }

    void OnEnable()
    {
        // TODO : Reenable the continue button once the save system is updated with all the new features
        _continueButton.interactable = GSaveManager.Instance.HasValidSaveFile();
    }

    public void ActivateQuitValidationMenu(bool toActivate)
    {
        _gameOverValidationPanel.SetActive(toActivate);
    }

    public void Show()
    {
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        
        _creditsRect.anchoredPosition = new Vector2(_creditsRect.anchoredPosition.x, _hideButtonY);
        _startRect.anchoredPosition = new Vector2(_startRect.anchoredPosition.x, _hideButtonY);
        _continueRect.anchoredPosition = new Vector2(_continueRect.anchoredPosition.x, _hideButtonY);
        _optionsRect.anchoredPosition = new Vector2(_optionsRect.anchoredPosition.x, _hideButtonY);
        _quitRect.anchoredPosition = new Vector2(_quitRect.anchoredPosition.x, _hideButtonY);
        _logoRect.anchoredPosition = new Vector2(_logoRect.anchoredPosition.x, 200);

        _canvasGroup.alpha = 1;
        _sequence.Join(_logoRect.DOAnchorPosY(-20, .5f).SetEase(Ease.OutCirc));
        _sequence.Join(_continueRect.DOAnchorPosY(_baseButtonY, .25f).SetEase(Ease.OutCirc));
        _sequence.Join(_optionsRect.DOAnchorPosY(_baseButtonY, .25f).SetEase(Ease.OutCirc).SetDelay(.1f));
        _sequence.Join(_quitRect.DOAnchorPosY(_baseButtonY, .25f).SetEase(Ease.OutCirc));
        _sequence.Join(_startRect.DOAnchorPosY(_baseButtonY, .25f).SetEase(Ease.OutCirc).SetDelay(.1f));
        _sequence.Join(_creditsRect.DOAnchorPosY(_baseButtonY, .25f).SetEase(Ease.OutCirc));
        
    }

    public void Hide()
    {
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        _sequence.Join(_logoRect.DOAnchorPosY(200, .5f).SetEase(Ease.InCirc));
        _sequence.Join(_continueRect.DOAnchorPosY(_hideButtonY, .25f).SetEase(Ease.InCirc));
        _sequence.Join(_optionsRect.DOAnchorPosY(_hideButtonY, .25f).SetEase(Ease.InCirc).SetDelay(.1f));
        _sequence.Join(_quitRect.DOAnchorPosY(_hideButtonY, .25f).SetEase(Ease.InCirc));
        _sequence.Join(_creditsRect.DOAnchorPosY(_hideButtonY, .25f).SetEase(Ease.InCirc).SetDelay(.1f));
        _sequence.Join(_startRect.DOAnchorPosY(_hideButtonY, .25f).SetEase(Ease.InCirc));
        _sequence.AppendCallback(() => _canvasGroup.alpha = 0);
    }
}

