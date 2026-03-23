using DG.Tweening;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class GGeneralButton : Button
{
    [SerializeField]
    private Image _buttonBack;
    
    private RectTransform _buttonTransform;

    [SerializeField]
    Sprite _baseButton;

    [SerializeField]
    Sprite _hoverButton;
    
    [SerializeField]
    Sprite _pressedButton;
    
    [SerializeField]
    private TextMeshProUGUI _text;
    
    private RectTransform _textTransform;
    
    [SerializeField, ColorUsage(true, true)]
    private Color _baseColor;
    
    [SerializeField, ColorUsage(true, true)]
    private Color _hoverColor;

    bool _isHover = false;
    
    Sequence _sequence;

    protected override void Start()
    {
        base.Start();
        _buttonTransform = _buttonBack.GetComponent<RectTransform>();
        _textTransform = _text.GetComponent<RectTransform>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        _isHover = true;
        
        if (!IsInteractable()) return;
        
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        _sequence.JoinCallback(() =>
        {
            _buttonBack.sprite = _hoverButton;
            _buttonTransform.pivot = new Vector2(.5f, 0f);
            _text.color = _hoverColor;
        });
        _sequence.Join(_buttonTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutQuad).From(new Vector3(.95f, 1.05f, 1f)));
        _sequence.Join(_textTransform.DOPivotY(.75f,.025f).SetEase(Ease.OutBack));
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        _isHover = false;
        
        if (!IsInteractable())  return;
        
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();

        _sequence.JoinCallback(() =>
        {
            _buttonBack.sprite = _baseButton;
            _text.color = _baseColor;
        });
        _sequence.Join(_buttonTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutQuad).From(new Vector3(1.05f, .95f, 1f)));
        _sequence.Join(_textTransform.DOPivotY(.5f,.025f).SetEase(Ease.OutBack));
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (!interactable)  return;
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();

        _sequence.JoinCallback(() =>
        {
            _buttonBack.sprite = _pressedButton;
            _text.color = _hoverColor;
        });
        _sequence.Join(_buttonTransform.DOScale(Vector3.one, .35f).SetEase(Ease.OutQuad).From(new Vector3(1.15f, .85f, 1f)));
        _sequence.Join(_textTransform.DOPivotY(.30f,.025f).SetEase(Ease.OutBack));
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (!_isHover) return;
        
        if (!IsInteractable())  return;
        
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        _sequence.JoinCallback(() =>
        {
            _buttonBack.sprite = _isHover ? _hoverButton : _baseButton;
            _text.color = _isHover ? _hoverColor : _baseColor;
        });
        _sequence.Join(_buttonTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutQuad).From(new Vector3(.95f, 1.05f, 1f)));
        _sequence.Join(_textTransform.DOPivotY(_isHover ? .75f : .5f,.025f).SetEase(Ease.OutBack));
    }
    
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (state == SelectionState.Disabled)
        {
            if (_isHover)
            {
                if (_sequence.IsActive()) _sequence.Kill(true);
                _sequence = DOTween.Sequence();

                _sequence.JoinCallback(() =>
                {
                    _buttonBack.sprite = _baseButton;
                    _text.color = _baseColor;
                });
                _sequence.Join(_buttonTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutQuad).From(new Vector3(1.05f, .95f, 1f)));
                _sequence.Join(_textTransform.DOPivotY(.5f,.025f).SetEase(Ease.OutBack));
            }

            _buttonBack.color = Color.gray;
            _text.color = Color.gray;
        }
        else if (state == SelectionState.Normal)
        {
            _buttonBack.color = Color.white;
            _text.color = Color.white;
            
            if (_isHover)
            {
                if (_sequence.IsActive()) _sequence.Kill(true);
                _sequence = DOTween.Sequence();
        
                _sequence.JoinCallback(() =>
                {
                    _buttonBack.sprite = _hoverButton;
                    _buttonTransform.pivot = new Vector2(.5f, 0f);
                    _text.color = _hoverColor;
                });
                _sequence.Join(_buttonTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutQuad).From(new Vector3(.95f, 1.05f, 1f)));
                _sequence.Join(_textTransform.DOPivotY(.75f,.025f).SetEase(Ease.OutBack));
            }
        }
    }
}
