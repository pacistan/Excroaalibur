using DG.Tweening;
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
    LocalizeStringEvent _nameTxt;
        
    [SerializeField]
    Image _mapCoverImage;
    
    [SerializeField]
    Image _lockImage;

    [SerializeField]
    RectTransform _SelectorImage;
    
    [SerializeField]
    TextMeshProUGUI _progressTxt;

    [SerializeField]
    Image _progressFillBar;

    [SerializeField]
    LocalizeStringEvent _conditionMapTxt;

    Sequence _selectorLoop;

    Sequence _clickSeq;
    
    void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);

        _selectorLoop = DOTween.Sequence();
        Vector3 maxSize = new Vector3(1.15f, 1.3f, 1f);
        _selectorLoop.SetUpdate(true);
        _selectorLoop.Append(_SelectorImage.DOScale(maxSize, .5f).From(1f).SetEase(Ease.OutCubic));
        _selectorLoop.Append(_SelectorImage.DOScale(1f, .5f).From(maxSize).SetEase(Ease.InQuint));
        _selectorLoop.SetLoops(-1);
        OnUnselected();
    }

    void OnEnable()
    {
        bool isMapUnlocked = IsMapUnlocked();
        _lockImage.enabled = !isMapUnlocked;
        _nameTxt.StringReference = mapData.MapName;
        _nameTxt.RefreshString();
        if (mapData.ProgressMapToUnlock)
        {
            _progressTxt.text = $"{mapData.ProgressMapToUnlock.NumberOfWavesOnThisMap.ToString()} / {mapData.NumberOfWavesToUnlock.ToString()}";
            _progressFillBar.fillAmount = (float)mapData.ProgressMapToUnlock.NumberOfWavesOnThisMap / (float)mapData.NumberOfWavesToUnlock;
            _conditionMapTxt.StringReference["map-name"] = CreateLocalizedStringInstance(mapData.MapName);
            _conditionMapTxt.RefreshString();
        }
        _mapCoverImage.sprite = mapData.MapCardSprite;
        _progressFillBar.gameObject.SetActive(!isMapUnlocked);
        _conditionMapTxt.gameObject.SetActive(!isMapUnlocked);
    }

    public void OnSelected()
    {
        if (_clickSeq.IsActive() && _clickSeq.IsPlaying()) return;
        _SelectorImage.gameObject.SetActive(true);
       _selectorLoop.Restart();
    }
    
    public void OnUnselected()
    {
        if (_clickSeq.IsActive() && _clickSeq.IsPlaying()) return;
        _SelectorImage.gameObject.SetActive(false);
        _selectorLoop.Pause();
    }

    public void OnClick()
    {
        _clickSeq = DOTween.Sequence();
        _clickSeq.SetUpdate(true);
        _SelectorImage.gameObject.SetActive(true);
        _selectorLoop.Pause();
        _clickSeq.Join(_SelectorImage.DOScale(1f, .25f).SetEase(Ease.OutCubic));
        _clickSeq.Join(GetComponent<RectTransform>().DOScale(.75f, .15f).SetEase(Ease.OutCubic));
        _clickSeq.JoinCallback(() =>
        {
            GGameManager.Instance.SetSceneToLoad(mapData);
            GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen);
        });
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
