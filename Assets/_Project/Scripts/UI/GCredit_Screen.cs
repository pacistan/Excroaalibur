using DG.Tweening;
using FMOD.Studio;
using Stanpac.Utilities;
using UnityEngine;

public class GCredit_Screen : MonoBehaviour
{
    [SerializeField] RectTransform _buttonRect;
    GCredit_Slot[] _credits;
    CanvasGroup _canvasGroup;
    
    Sequence _sequence;

    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        _credits = GetComponentsInChildren<GCredit_Slot>();
    }
    
    public void Show()
    {
        if(_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        _buttonRect.position = new Vector3(_buttonRect.position.x, -_buttonRect.sizeDelta.y, 0);

        _sequence.Append(_canvasGroup.DOFade(1, 0.25f).From(0f).SetEase(Ease.OutQuad));
        
        GCredit_Slot[] slots = _credits;
        slots.Shuffle();
        foreach (GCredit_Slot slot in slots)
        {
            _sequence.AppendCallback(() => slot.Show());
            _sequence.AppendInterval(1f / slots.Length);
        }

        _sequence.Append(_buttonRect.DOAnchorPosY(10f, .25f).SetEase(Ease.OutQuad));
    }

    public void Hide()
    {
        if(_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        foreach (GCredit_Slot slot in _credits)
        {
            _sequence.JoinCallback(() => slot.Hide());
        }
        _sequence.Join(_buttonRect.DOAnchorPosY(-_buttonRect.sizeDelta.y, .25f).SetEase(Ease.InCirc));
        _sequence.Append(_canvasGroup.DOFade(0, 0.25f).SetEase(Ease.InQuad).SetDelay(.25f));
    }
}
