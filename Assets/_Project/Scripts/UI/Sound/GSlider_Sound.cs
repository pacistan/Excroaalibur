using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class GSlider_Sound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Slider _slider;

    [SerializeField]
    FMOD.Studio.Bus _bus;

    [SerializeField]
    RectTransform _handle;

    bool _isHover = false;
    bool _isDrag = false;
    
    void OnSliderValueChanged(float value)
    {
        if (!_bus.isValid()) return;

        _bus.setVolume(value);
    }

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        _bus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
    
        if (!_bus.isValid()) return;
    
        _bus.getVolume(out float volume);
    
        _slider.SetValueWithoutNotify(volume);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHover = true;
        if (_isDrag) return;
        _handle.DOScale(Vector3.one * 1.15f, .25f).SetEase(Ease.OutElastic);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHover = false;
        if (_isDrag) return;
        _handle.DOScale(Vector3.one, .25f).SetEase(Ease.OutElastic);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDrag = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDrag = false;
        if (_isHover) return;
        _handle.DOScale(Vector3.one, .25f).SetEase(Ease.OutElastic);
    }
}
