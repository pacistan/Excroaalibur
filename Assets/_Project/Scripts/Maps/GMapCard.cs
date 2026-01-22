using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class GMapCard : MonoBehaviour
{
    [field: SerializeField]
    public GSOMapData mapData { get; private set; }

    Button _button;

    [SerializeField]
    Image _mapCoverImage;
    
    [SerializeField]
    Image _lockImage;

    [SerializeField]
    TextMeshProUGUI _progressTxt;

    [SerializeField]
    Image _progressFillBar;
    
    [SerializeField]
    LocalizeStringEvent _conditionMapTxt;
    
    void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            GGameManager.Instance.SetSceneToLoad(mapData.SceneName);
            GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen);
        });
    }

    void OnEnable()
    {
        _lockImage.enabled = IsMapUnlocked();
        _progressTxt.text = $"{mapData.ProgressMapToUnlock.NumberOfWavesOnThisMap.ToString()} / {mapData.NumberOfWavesToUnlock.ToString()}";
        _progressFillBar.fillAmount = (float)mapData.ProgressMapToUnlock.NumberOfWavesOnThisMap / (float)mapData.NumberOfWavesToUnlock;
        _conditionMapTxt.StringReference["map-name"] = CreateLocalizedStringInstance(mapData.MapName);
        _mapCoverImage.sprite = mapData.MapCardSprite;
        _conditionMapTxt.RefreshString();
    }

    LocalizedString CreateLocalizedStringInstance(LocalizedString oldLocalizedString)
    {
        var localizedString = new LocalizedString();
        localizedString.TableReference = oldLocalizedString.TableReference;
        localizedString.TableEntryReference = oldLocalizedString.TableEntryReference;
        return localizedString;
    }
    
    bool IsMapUnlocked() => mapData.ProgressMapToUnlock == null || mapData.ProgressMapToUnlock.NumberOfWavesOnThisMap >= mapData.NumberOfWavesToUnlock;
}
