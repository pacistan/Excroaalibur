using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMapSelectionMenu : MonoBehaviour
{
    [SerializeField]
    List<GMapCard> _mapCards;
    
    [SerializeField]
    Button _exitBtn;
    RectTransform _exitTransform;
    
    
    [SerializeField]
    RectTransform _background;
    
    Sequence _sequence;
    
    CanvasGroup _canvasGroup;
    
    void Start()
    {
        _exitBtn.onClick.AddListener(() => GGameManager.Instance.ChangeState(GGameManager.Instance.previousState));
        _exitTransform = _exitBtn.GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    public void Show()
    {
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();

        _background.pivot = new Vector2(1, 0.5f);
        _exitTransform.anchoredPosition = new Vector2(0, -100);
        
        _canvasGroup.alpha = 1;
        _sequence.Append(_background.DOScaleX(1, .25f).From(0).SetEase(Ease.OutCirc));
        

        float delay = 0;
        foreach (var card in _mapCards)
        {
            _sequence.Join(card.GetComponent<RectTransform>().DOScale(1, .15f).From(0).SetEase(Ease.OutBack).SetDelay(delay));
            delay += .025f;
        }
        
        _sequence.Join(_exitTransform.DOAnchorPosY(0, .25f).SetEase(Ease.OutCirc).SetDelay(delay));
    }

    public void Hide()
    {
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        _background.pivot = new Vector2(0, 0.5f);
        _sequence.Append(_exitTransform.DOAnchorPosY(-100, .25f).SetEase(Ease.OutQuad));

        float delay = 0;
        foreach (var card in _mapCards)
        {
            _sequence.Join(card.GetComponent<RectTransform>().DOScale(0, .15f).From(1).SetEase(Ease.InCirc).SetDelay(delay));
            delay += .025f;
        }
        
        _sequence.Join(_background.DOScaleX(0, .25f).From(1).SetEase(Ease.InCirc).SetDelay(delay - .25f));
        _sequence.AppendCallback(() => _canvasGroup.alpha = 0);
    }
}
