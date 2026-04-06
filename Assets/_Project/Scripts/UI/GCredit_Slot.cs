using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GCredit_Slot : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] string _creditName;
    [SerializeField] string _role;

    [TextArea(3, 8)]
    [SerializeField] string _skills;
    [SerializeField] Sprite _portrait;

    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _roleText;
    [SerializeField] TMP_Text _skillsText;
    [SerializeField] Image _portraitImage;

    CanvasGroup _canvasGroup;
    RectTransform _rect;
    RectTransform _childRect;
    
    Sequence _sequence;
    Sequence _floatShake;

    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rect = GetComponent<RectTransform>();
        _canvasGroup.alpha = 0;
        _childRect = GetComponentInChildren<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_floatShake.IsActive()) _floatShake.Kill(true);
        _floatShake = DOTween.Sequence();
        _floatShake.Append(_childRect.DOShakePosition(1f, 10f, 3, 50f, false, true, ShakeRandomnessMode.Harmonic));
    }

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    void Apply()
    {
        if (_nameText != null)
            _nameText.text = _creditName;

        if (_roleText != null)
            _roleText.text = _role;

        if (_skillsText != null)
            _skillsText.text = _skills;

        if (_portraitImage != null)
            _portraitImage.sprite = _portrait;
    }
    
    public void Show()
    {
        if(_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();

        _sequence.Append(_canvasGroup.DOFade(1, 0.25f).From(0f).SetEase(Ease.OutQuad));
        _sequence.Join(_rect.DOScale(1,  0.5f).From(0).SetEase(Ease.OutBack));
    }

    public void Hide()
    {
        if(_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        _sequence.Append(_canvasGroup.DOFade(0, 0.25f).SetEase(Ease.InQuad).SetDelay(.25f));
        _sequence.Join(_rect.DOScale(0, 0.5f).SetEase(Ease.InCirc));
    }
}
