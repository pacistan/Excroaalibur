using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GUpgradeCharacterSelectMenu : MonoBehaviour
{
    [SerializeField] Button _btnReturn;
    RectTransform _btnReturnRect;

    [SerializeField]
    RectTransform _frame;
    Image _frameImage;
    
    [SerializeField] RectTransform _centerTitle;
    [SerializeField] RectTransform _topTitle;
    [SerializeField] RectTransform _title;
    
    CanvasGroup _canvasGroup;
    
    Sequence _sequence;
    
    void Start()
    {
        _btnReturn.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Upgrade_Select_Card));
        _canvasGroup = GetComponent<CanvasGroup>();
        _frameImage = _frame.GetComponent<Image>();
        _btnReturnRect = _btnReturn.GetComponent<RectTransform>();
    }

    public void Show()
    {
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        _title.position = _centerTitle.position;
        _sequence.Append(_canvasGroup.DOFade(1f, .25f).SetEase(Ease.OutSine));
        _sequence.Join(_frame.DOScale(Vector3.one, .5f).From(Vector3.zero).SetEase(Ease.OutExpo));
        _sequence.Join(_frameImage.DOFade(1f, .5f).SetEase(Ease.OutSine));
        _sequence.Join(_btnReturnRect.DOAnchorPosY(30f, .5f).SetEase(Ease.OutSine));

        _sequence.AppendInterval(1f);
        _sequence.Append(_title.DOMove(_topTitle.position, .5f).SetEase(Ease.InOutSine));
    }

    public void Hide()
    {
        if (_sequence.IsActive()) _sequence.Kill(true);
        _sequence = DOTween.Sequence();
        
        _sequence.Append(_frame.DOScale(Vector3.zero, .5f).SetEase(Ease.OutExpo));
        _sequence.Join(_frameImage.DOFade(0f, .5f).SetEase(Ease.InSine));
        _sequence.Join(_canvasGroup.DOFade(0f, .25f).SetEase(Ease.InSine));
        _sequence.Join(_btnReturnRect.DOAnchorPosY(-100, .5f).SetEase(Ease.OutSine));
    }
}
