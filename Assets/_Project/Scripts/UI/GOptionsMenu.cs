using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GOptionsMenu : MonoBehaviour
{
    [SerializeField]
    private Slider _volumeSlider;
    
    [SerializeField]
    private Slider _musicSlider;
    
    [SerializeField]
    private Slider _sfxSlider;
    
    [SerializeField]
    Toggle _skipTurnAutoToggle;

    [SerializeField]
    Button _exitOptionMenuBtn;
    
    FMOD.Studio.Bus _musicBus;
    FMOD.Studio.Bus _masterBus;
    FMOD.Studio.Bus _sfxBus;

    
    void OnSliderMusicValueChanged(float value)
    {
        if (!_musicBus.isValid()) return;

        _musicBus.setVolume(value);
    }
    
    void OnSliderVolumeValueChanged(float value)
    {
        if(!_masterBus.isValid()) return;
        _masterBus.setVolume(value);
    }
    
    void OnSliderSfxValueChanged(float value)
    {
        if(!_sfxBus.isValid()) return;
        _sfxBus.setVolume(value);
    }

    void Awake()
    {
        GPlayerController playerController = GameObject.FindFirstObjectByType<GPlayerController>();
        
        _volumeSlider.onValueChanged.AddListener(OnSliderVolumeValueChanged);
        _volumeSlider.minValue = 0f;
        _volumeSlider.maxValue = 1f;
        
        _musicSlider.onValueChanged.AddListener(OnSliderMusicValueChanged);
        _musicSlider.minValue = 0f;
        _musicSlider.maxValue = 1f;
        
        _sfxSlider.onValueChanged.AddListener(OnSliderSfxValueChanged);
        _sfxSlider.minValue = 0f;
        _sfxSlider.maxValue = 1f;
        
        _musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        _masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
        _sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
    
        _skipTurnAutoToggle.onValueChanged.AddListener(playerController.SetEndTurnWhenNoActionsLeft);
        _exitOptionMenuBtn.onClick.AddListener(() => GGameManager.Instance.ChangeState(GGameManager.Instance.previousState));
        
        if (!_musicBus.isValid()) return;
    
        _musicBus.getVolume(out float musicVolume);
        _musicSlider.SetValueWithoutNotify(musicVolume);
        
        if(!_masterBus.isValid()) return;
        
        _masterBus.getVolume(out float volume);
        _volumeSlider.SetValueWithoutNotify(volume);
        
        if(!_sfxBus.isValid()) return;
        
        _sfxBus.getVolume(out float sfxVolume);
        _sfxSlider.SetValueWithoutNotify(sfxVolume);
    }
}
