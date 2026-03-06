using DG.Tweening;
using Sirenix.Utilities;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GLoadingScreenMenu : MonoBehaviour
{
    [SerializeField]
    Image _background;

    Material _back_mat;
    
    [SerializeField]
    Image _progressFill;
    
    [SerializeField]
    TextMeshProUGUI _progressText;

    [SerializeField]
    RectTransform _progressBar;
    CanvasGroup _progressGroup;
    
    [SerializeField]
    RectTransform _loadingDiscParent;

    [SerializeField]
    float[] _loadingDiscRotationSpeed;

    [SerializeField]
    Image _sittingFrog;
    
    [SerializeField]
    Image _jumpingFrog;
    
    [SerializeField]
    RectTransform _startJumpPosition;
    
    [SerializeField]
    RectTransform _endJumpPosition;
    
    [SerializeField]
    AnimationCurve _jumpCurve;
    
    RectTransform[]  _loadingDiscs = Array.Empty<RectTransform>();
    
    float _oldValue;

    bool _loaded = false;

    float _baseBarWidth = 500f;
    Sequence seq;

    
    public void ResetProgress()
    {
        if (!_back_mat) Setup();
        
        _oldValue = 0;
        _progressFill.fillAmount = 0;
        _progressText.text = "0%";
        _loaded = false;
    }
    
    public void UpdateProgress(float newProgress)
    {
        float newValue = Mathf.Min(_oldValue + Time.unscaledDeltaTime, newProgress);
        _oldValue = newValue;
        _progressFill.fillAmount = newValue;
        _progressText.text = Mathf.RoundToInt(newValue * 100).ToString() + "%";

        if (newValue >= 1 && !_loaded)
        {
            _loaded = true;
        }
    }

    public void Show()
    {
        _back_mat.SetFloat("_Rotation", -135f);
        _jumpingFrog.rectTransform.anchoredPosition = _startJumpPosition.anchoredPosition;
    
        if (seq is { active: true }) seq.Kill(true);
        seq = DOTween.Sequence();
        seq.SetUpdate(true);
        seq.Append(_back_mat.DOFloat(1f, "_Progress", 1f).SetEase(Ease.OutQuad));
        seq.Insert(.35f, _progressBar.DOSizeDelta(new Vector2(_baseBarWidth, _progressBar.sizeDelta.y), 0.25f).From(new Vector2(0, _progressBar.rect.height)).SetEase(Ease.OutCirc));
        seq.Join(_progressGroup.DOFade(1f, .25f).SetEase(Ease.OutCirc));

        seq.Append(_jumpingFrog.rectTransform.DOJump(_sittingFrog.rectTransform.position, 150f, 1, .85f).SetEase(Ease.Linear));
        seq.AppendCallback(() =>
        {
            _jumpingFrog.enabled = false;
            _sittingFrog.enabled = true;
        });
        
        float delay = .45f;
        foreach (RectTransform disc in _loadingDiscs)
        {
            if (!disc) continue;
            seq.Insert(delay, disc.DOScale(Vector3.one, 0.15f ).SetEase(Ease.OutBack));
            delay += .05f;
        }
    }

    public void Hide()
    {
        _back_mat.SetFloat("_Rotation", -45f);
        
        if (seq is { active: true }) seq.Kill(true);
        seq = DOTween.Sequence();
        seq.SetUpdate(true);
        RectTransform[] discs = _loadingDiscs;
        
        seq.Join(_jumpingFrog.rectTransform.DOJump(_endJumpPosition.position, 700f, 1, 1.15f).SetEase(_jumpCurve));
        seq.JoinCallback(() =>
        {
            _jumpingFrog.enabled = true;
            _sittingFrog.enabled = false;
        });
        seq.Insert( .15f,_back_mat.DOFloat(0f, "_Progress", 1f).SetEase(Ease.InSine));
        seq.Join(_progressBar.DOSizeDelta(new Vector2(50f, _progressBar.sizeDelta.y), 0.25f).SetEase(Ease.OutCubic));
        seq.Join(_progressGroup.DOFade(0, .25f).SetEase(Ease.OutCubic));

        float delay = .15f;
        foreach (RectTransform disc in discs.Reverse())
        {
            if (!disc) continue;
            seq.Join(disc.DOScale(Vector3.zero, 0.15f).From(Vector3.one).SetEase(Ease.InBack).SetDelay(delay));
            delay += .05f;
        }
    }

    void Update()
    {
        _loadingDiscs.ForEach(disc => disc.Rotate(0, 0, 
            _loadingDiscRotationSpeed[Mathf.Min(disc.GetSiblingIndex(), _loadingDiscRotationSpeed.Length - 1)] * Time.unscaledDeltaTime));
    }

    void Setup()
    {
        if (_back_mat) return;
        
        if (_background)
        {
            _background.material = new Material(_background.material);
            _back_mat = _background.material;
            _back_mat.SetFloat("_Progress", 0f);
        }
        
        _loadingDiscs = new RectTransform[_loadingDiscParent.childCount];
        for (int i = 0; i < _loadingDiscParent.childCount; i++)
        {
            RectTransform disc = _loadingDiscParent.GetChild(i) as RectTransform;
            if (!disc) continue;
            _loadingDiscs[i] = disc;
            disc.localScale = Vector3.zero;
        }

        _baseBarWidth = _progressBar.rect.width;
        _progressGroup = _progressBar.gameObject.GetComponent<CanvasGroup>();
        _progressGroup.alpha = 0;
        _progressBar.rect.SetSize(0, _progressBar.rect.y);
    }
    
    void Awake()
    {
        Setup();
    }
}
