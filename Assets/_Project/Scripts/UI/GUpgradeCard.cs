using DG.Tweening;
using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Vector3 = UnityEngine.Vector3;

public class GUpgradeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    Image _iconImg;

    [SerializeField]
    LocalizeStringEvent _nameTxt;

    [SerializeField]
    LocalizeStringEvent _descriptionTxt;

    [SerializeField]
    RectTransform _selectorIcon;
    
    [SerializeField]
    RectTransform _selectorFrame;
    Image _selectorFrameImage;

    [SerializeField]
    Image _header;
    
    [SerializeField]
    Image _footer;
    
    Material _headerMaterial;
    Material _footerMaterial;
    
    Button _upgradeBtn;
    CanvasGroup _group;

    Sequence _sequence;
    Sequence _Pointersequence;
    
    Sequence _selectorLoop;
    
    public void Initialize(int upgradeIndex)
    {
        _upgradeBtn = GetComponent<Button>();
        _upgradeBtn.onClick.AddListener(() => GUpgradeManager.Instance.OnUpgradeSelected(upgradeIndex));
    }

    void Start()
    {
        _selectorFrameImage = _selectorFrame.GetComponent<Image>();
        _group = GetComponent<CanvasGroup>();
        _headerMaterial = new Material(_header.material);
        _footerMaterial = new Material(_footer.material);
        _header.material = _headerMaterial;
        _footer.material = _footerMaterial;
        
        _selectorLoop = DOTween.Sequence();
        _selectorLoop.Append(_selectorIcon.DORotate(new Vector3(0, 0, -45f), .75f).From(new Vector3(0, 0, 45f)).SetEase(Ease.InOutCirc));
        _selectorLoop.Join(_selectorFrame.DOScale(Vector3.one * 1.05f, .75f).SetEase(Ease.InOutSine).SetDelay(.15f));
        _selectorLoop.Append(_selectorIcon.DORotate(new Vector3(0, 0, 45f), .75f).SetEase(Ease.InOutCirc));
        _selectorLoop.Join(_selectorFrame.DOScale(Vector3.one, .75f).SetEase(Ease.InOutSine).SetDelay(.15f));
        _selectorLoop.SetLoops(-1);
        _selectorLoop.Pause();

        _group.alpha = 0;
    }

    public void Build(GSOUpgrade upgrade)
    {
        _nameTxt.StringReference.SetReference(upgrade.Name.TableReference, upgrade.Name.TableEntryReference);
        _descriptionTxt.StringReference.SetReference(upgrade.Description.TableReference, upgrade.Description.TableEntryReference);
        _iconImg.sprite = upgrade.Icon;
        // TODO : Localized Name and Description
    }

    public void Show()
    {
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();

        _headerMaterial.SetFloat("_Progress", 0f);
        _footerMaterial.SetFloat("_Progress", 0f);
        _sequence.Append(_group.DOFade(1f, .25f).From(0f).SetEase(Ease.OutSine));
        _sequence.Append(_headerMaterial.DOFloat(1f, "_Progress", .75f).From(0).SetEase(Ease.OutSine));
        _sequence.Join(_footerMaterial.DOFloat(1f, "_Progress", .75f).From(0).SetEase(Ease.OutSine).SetDelay(.15f));
    }

    public void Hide()
    {
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();

        _sequence.Append(_headerMaterial.DOFloat(0f, "_Progress", .75f).From(1f).SetEase(Ease.InSine));
        _sequence.Join(_footerMaterial.DOFloat(0f, "_Progress", .75f).From(1f).SetEase(Ease.InSine).SetDelay(.15f));
        _sequence.Append(_group.DOFade(0f, .25f).From(1f).SetEase(Ease.InSine));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_Pointersequence.IsActive()) _Pointersequence.Kill(true);
        _Pointersequence = DOTween.Sequence();

        _Pointersequence.AppendCallback(() =>
        {
            _selectorIcon.gameObject.SetActive(true);
            _selectorFrame.gameObject.SetActive(true);
        });
        _Pointersequence.Append(_selectorIcon.DOScale(Vector3.one, .25f).From(Vector3.zero).SetEase(Ease.OutBack));
        _Pointersequence.Join(_selectorIcon.DORotate(new Vector3(0, 0, 45f), .75f).SetEase(Ease.OutExpo));
        _Pointersequence.Join(_selectorFrameImage.DOFade(1, .25f).From(0).SetEase(Ease.OutExpo));
        _Pointersequence.Join(_selectorFrame.DOScale(Vector3.one, .25f).From(Vector3.one * .95f).SetEase(Ease.OutExpo));
        _Pointersequence.AppendCallback(() =>
        {
            _selectorLoop.Restart();
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_Pointersequence.IsActive()) _Pointersequence.Kill(true);
        _Pointersequence = DOTween.Sequence();

        _Pointersequence.AppendCallback(() =>
        {
            _selectorLoop.Pause();
        });
        _Pointersequence.Append(_selectorIcon.DOScale(Vector3.zero, .25f).From(Vector3.one).SetEase(Ease.InQuart));
        _Pointersequence.Join(_selectorFrameImage.DOFade(0, .25f).SetEase(Ease.InExpo));
        _Pointersequence.Join(_selectorFrame.DOScale(Vector3.one * .95f, .25f).SetEase(Ease.InExpo));
        _Pointersequence.AppendCallback(() =>
        {
            _selectorIcon.gameObject.SetActive(false);
            _selectorFrame.gameObject.SetActive(false);
        });
    }
}
