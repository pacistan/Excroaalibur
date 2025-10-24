using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class GSlider_Sound : MonoBehaviour
{
    private Slider _slider;

    [SerializeField]
    FMOD.Studio.Bus _bus;
    
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
}
