using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Linq;
using UnityEngine;

public class GTab_Switcher : MonoBehaviour
{
    [ReadOnly]
    CanvasGroup[] _tabs;
    
    [ReadOnly]
    int _currentTab = 0;

    Sequence _sequence;
    
    void Start()
    {
        _tabs = GetComponentsInChildren<CanvasGroup>();

        if (_tabs.IsNullOrEmpty())
        {
            enabled = false;
            return;
        }

        foreach (CanvasGroup c in _tabs)
        {
            c.alpha = 0;
            c.gameObject.SetActive(false);
        }
        
        _tabs[_currentTab].alpha = 1;
        _tabs[_currentTab].gameObject.SetActive(true);
    }

    public void SelectTab(int tabIndex)
    {
        if (tabIndex == _currentTab) return;
        if (tabIndex < 0 || tabIndex >= _tabs.Length) return;

        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        CanvasGroup oldTab = _tabs[_currentTab];
        _currentTab = tabIndex;
        CanvasGroup newTab = _tabs[_currentTab];
        
        _sequence.Append(oldTab.DOFade(0, .25f).SetEase(Ease.InCirc).OnComplete(() =>
            oldTab.gameObject.SetActive(false)));
        _sequence.JoinCallback(() => newTab.gameObject.SetActive(true));
        _sequence.Join(newTab.DOFade(1, .25f).SetEase(Ease.OutCirc).SetDelay(.25f));
    }

    public void SelectTab(CanvasGroup tab)
    {
        if (tab == null) return;
        
        if (!_tabs.Contains(tab)) return;
        
        SelectTab(Array.IndexOf(_tabs, tab));
    }
}
