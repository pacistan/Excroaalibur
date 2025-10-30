using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GPauseMenu : MonoBehaviour
{
    [SerializeField]
    Button _continueBtn, _optionsBtn, _restartBtn, _mainMenuBtn;

    void Start()
    {
        _continueBtn.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Play));
        _optionsBtn.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Options));
        _restartBtn.onClick.AddListener(()=> GGameManager.Instance.ReloadScene());
        _mainMenuBtn.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Start));
    }
}