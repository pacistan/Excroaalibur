using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GReturn_Button_Options : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Button _button;

    bool _isHover = false;
    
    RectTransform _boxTransform;

    void Start()
    {
        _boxTransform = _button.GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHover = true;
        _boxTransform.DOScale(Vector3.one * 1.15f, .25f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHover = false;
        _boxTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _boxTransform.DOScale(Vector3.one * .85f, .25f).SetEase(Ease.OutElastic);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isHover) return;
        _boxTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutElastic);
    }
}
