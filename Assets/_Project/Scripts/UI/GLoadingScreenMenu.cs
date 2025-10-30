using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GLoadingScreenMenu : MonoBehaviour
{
    [SerializeField]
    Image _progressFill;
    
    [SerializeField]
    TextMeshProUGUI _progressText;

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
}
