using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GHudManager : GSingleton<GHudManager>
{
    public GTargetHud TargetHud { get; private set; }
    public GPlayMenu playMenu { get; private set; }
    public GPauseMenu pauseMenu { get; private set; }
    public GStartMenu startMenu { get; private set; } 
    public GGameOverMenu gameOverMenu { get; private set; }
    public GLoadingScreenMenu loadingScreenMenu { get; private set; }

    [SerializeField]
    Image _quickTransitionImage;
    
    Material _quickTransitionMaterial;
    
    protected override void Awake()
    {
        base.Awake();
        startMenu = GetComponentInChildren<GStartMenu>(true);
        TargetHud = GetComponentInChildren<GTargetHud>(true);
        playMenu = GetComponentInChildren<GPlayMenu>(true);
        pauseMenu = GetComponentInChildren<GPauseMenu>(true);
        gameOverMenu = GetComponentInChildren<GGameOverMenu>(true);
        loadingScreenMenu = GetComponentInChildren<GLoadingScreenMenu>(true);
        
        if (!_quickTransitionImage) return;
        _quickTransitionMaterial = new Material(_quickTransitionImage.material);
        _quickTransitionImage.material = _quickTransitionMaterial;
    }
    

    public void QuickTransition(Action onTransitions = null)
    {
        Sequence transition = DOTween.Sequence();
        transition.SetUpdate(true);
        
        _quickTransitionMaterial.SetInt("_Invert", 0);
        transition.Append(_quickTransitionMaterial.DOFloat(1f, "_Progress", .75f).SetEase(Ease.InSine));
        transition.AppendCallback(() =>
        {
            _quickTransitionMaterial.SetInt("_Invert", 1);
            onTransitions?.Invoke();
        });
        transition.Append(_quickTransitionMaterial.DOFloat(0f, "_Progress", .75f).SetEase(Ease.OutSine));
    }
}
