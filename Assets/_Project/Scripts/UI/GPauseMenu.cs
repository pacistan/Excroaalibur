using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GPauseMenu : MonoBehaviour
{
    [SerializeField]
    Button _continueBtn, _optionsBtn, _restartBtn, _mainMenuBtn, _tutoBtn;

    [SerializeField]
    CanvasGroup _tutoCanvas;
    
    void Start()
    {
        _continueBtn.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Play));
        _optionsBtn.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Options));
        _restartBtn.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen));
        _mainMenuBtn.onClick.AddListener(()=>
        {
            GHudManager.Instance.QuickTransition(() => GGameManager.Instance.ChangeState(EMacroStates.Start));
        });
        _tutoBtn.onClick.AddListener(()=> ShowTuto(true));
    }

    public void ShowTuto(bool show)
    {
        if (show)
        {
            _tutoCanvas.gameObject.SetActive(true);
            _tutoCanvas.DOFade(1, .25f).SetEase(Ease.OutCubic).From(0);
        }
        else
        {
            _tutoCanvas.DOFade(0, .25f).SetEase(Ease.InCubic).OnComplete(() => _tutoCanvas.gameObject.SetActive(false));
        }
    }
}