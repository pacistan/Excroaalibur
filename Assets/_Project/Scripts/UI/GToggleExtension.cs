using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class GToggleExtension : MonoBehaviour
{
    Toggle _toggle;
    [SerializeField]
    Color _offColor;
    [SerializeField]
    Color _onColor;
    
    
    void Start()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(_toggle.isOn);
    }

    private void OnValueChanged(bool isOn)
    {
        _toggle.image.color = isOn ? _onColor : _offColor;
    }
}
