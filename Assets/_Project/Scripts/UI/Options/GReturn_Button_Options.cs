using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GReturn_Button_Options : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Button _button;

    RectTransform _boxTransform;

    bool _isHover = false;

    void Start()
    {
        _boxTransform = _button.GetComponent<RectTransform>();
        _button.onClick.AddListener(OnClick);
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

    public void OnClick()
    {
        _boxTransform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        _boxTransform.DOPunchScale(Vector3.one * -.15f, .25f);
    }
}
