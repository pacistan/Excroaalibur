using UnityEngine;
using UnityEngine.UI;

public class GOptionsMenu : MonoBehaviour
{
    [SerializeField]
    private Slider _slider;

    [SerializeField]
    Toggle _skipTurnAutoToggle;

    [SerializeField]
    Button _exitOptionMenuBtn;
    
    FMOD.Studio.Bus _bus;
    
    void OnSliderValueChanged(float value)
    {
        if (!_bus.isValid()) return;

        _bus.setVolume(value);
    }

    void Awake()
    {
        GPlayerController playerController = GameObject.FindFirstObjectByType<GPlayerController>();
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        _bus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
    
        if (!_bus.isValid()) return;
    
        _bus.getVolume(out float volume);
    
        _slider.SetValueWithoutNotify(volume);
        _skipTurnAutoToggle.onValueChanged.AddListener(playerController.SetEndTurnWhenNoActionsLeft);
        _exitOptionMenuBtn.onClick.AddListener(() => GGameManager.Instance.ChangeState(GGameManager.Instance.previousState));
    }
}
