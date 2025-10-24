using DG.Tweening;
using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenu : MonoBehaviour
{
    CanvasGroup _canvasGroup;

    public bool IsOpen =>  _canvasGroup.interactable;
    
    public void Open()
    {
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.DOFade(1, .5f).SetEase(Ease.OutCirc);
    }

    public void Close()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.DOFade(0, .5f).SetEase(Ease.OutCirc);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void Awake()
    {
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }
}