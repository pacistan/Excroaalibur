using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GUpgradeCardSelectMenu : MonoBehaviour
{
    [SerializeField]
    List<GUpgradeCard> _upgrades = new List<GUpgradeCard>();

    CanvasGroup _canvasGroup;
    
    Sequence _showSequence;
    
    void Start()
    {
        for (var i = 0; i < _upgrades.Count; i++)
        {
            _upgrades[i].Initialize(i);
        }
        
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void BuildUpgradesUI(ref List<GSOUpgrade> upgradesData)
    {
        for(int i = 0; i < upgradesData.Count; i++)
        {
            _upgrades[i].Build(upgradesData[i]);
        }
    }

    public void ShowScreen()
    {
        if (_showSequence.IsActive()) _showSequence.Kill();
        _showSequence = DOTween.Sequence();
        
        _showSequence.Append(_canvasGroup.DOFade(1f, .5f).From(0f).SetEase(Ease.OutSine));
        _showSequence.AppendInterval(.15f);
        foreach (var upgrade in _upgrades)
        {
            _showSequence.JoinCallback(upgrade.Show);
        }
    }

    public void HideScreen()
    {
        if (_showSequence.IsActive()) _showSequence.Kill();
        _showSequence = DOTween.Sequence();
        
        foreach (var upgrade in _upgrades)
        {
            _showSequence.JoinCallback(upgrade.Hide);
        }
        _showSequence.Append(_canvasGroup.DOFade(0f, .5f).SetEase(Ease.OutSine));
    }
}