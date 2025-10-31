using Sirenix.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GLoadingScreenMenu : MonoBehaviour
{
    [SerializeField]
    Image _progressFill;
    
    [SerializeField]
    TextMeshProUGUI _progressText;

    [SerializeField]
    RectTransform _loadingDiscParent;

    [SerializeField]
    float[] _loadingDiscRotationSpeed;
    
    RectTransform[]  _loadingDiscs;
    
    float _oldValue;

    public void ResetProgress()
    {
        _oldValue = 0;
        _progressFill.fillAmount = 0;
        _progressText.text = "0%";
    }
    
    public void UpdateProgress(float newProgress)
    {
        float newValue = Mathf.Min(_oldValue + Time.unscaledDeltaTime, newProgress);
        _oldValue = newValue;
        _progressFill.fillAmount = newValue;
        _progressText.text = Mathf.RoundToInt(newValue * 100).ToString() + "%";
    }

    void Update()
    {
        _loadingDiscs.ForEach(disc => disc.Rotate(0, 0, 
            _loadingDiscRotationSpeed[Mathf.Min(disc.GetSiblingIndex(), _loadingDiscRotationSpeed.Length - 1)] * Time.unscaledDeltaTime));
    }

    void Start()
    {
        _loadingDiscs = new RectTransform[_loadingDiscParent.childCount];
        for (int i = 0; i < _loadingDiscParent.childCount; i++)
        {
            _loadingDiscs[i] = _loadingDiscParent.GetChild(i) as RectTransform;
        }
    }
}
