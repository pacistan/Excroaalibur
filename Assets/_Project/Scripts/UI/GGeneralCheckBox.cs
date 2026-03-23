using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GGeneralCHeckBox : MonoBehaviour
{
    private Toggle _toggle;
    
    private RectTransform _checkTransform;
    private RectTransform _boxTransform;

    Sequence _sequence;
    
    void Start()
    {
        _toggle = GetComponent<Toggle>();
        _checkTransform = _toggle.graphic.GetComponent<RectTransform>();
        _boxTransform = _toggle.targetGraphic.GetComponent<RectTransform>();
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnToggleValueChanged(bool value)
    {
        if (value)
        {
            if (_sequence.IsActive()) _sequence.Kill(true);
            _sequence = DOTween.Sequence();

            _sequence.Append(_toggle.graphic.DOFade(1, .25f).From(0).SetEase(Ease.OutExpo));
            _sequence.Join(_checkTransform.DOScale(Vector3.one, .25f).From(Vector3.one*1.25f).SetEase(Ease.InCirc));
            _sequence.Append(_boxTransform.DOPunchScale(Vector3.one * -.15f, .25f));
        }
        else
        {
            if (_sequence.IsActive()) _sequence.Kill(true);
            _sequence = DOTween.Sequence();

            _sequence.Append(_boxTransform.DOShakePosition(.25f, 2f).SetEase(Ease.OutExpo));
            _sequence.Join(_boxTransform.DOPunchScale(Vector3.one * -.15f, .25f));
        }
    }
}
