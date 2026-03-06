using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionToken_Container : MonoBehaviour
{
    [SerializeField]
    Sprite _tokenSprite;
    
    [SerializeField]
    Sprite _consumedTokenSprite;
    
    [SerializeField]
    Image _slashSprite;

    Image[] _actionTokens = Array.Empty<Image>();
    List<RectTransform> _previsTokens = new List<RectTransform>();

    int _remainingToken = -1;
    int _maxTokens = -1;
    
    void Awake()
    {
        List<Image> childs = new List<Image>(GetComponentsInChildren<Image>());
        childs.Remove(_slashSprite);
        _actionTokens = childs.ToArray();
        _remainingToken = _actionTokens.Length;
        _maxTokens = _remainingToken;
    }

    public void initTokens(int max, int remaining)
    {
        for (int i = 0; i < _actionTokens.Length; i++)
        {
            _actionTokens[i].enabled = i < max;
            _actionTokens[i].sprite = i < remaining ? _consumedTokenSprite : _tokenSprite;
        }
        _maxTokens = max;
        _remainingToken = remaining;
    }

    public void SetMaxTokenAmount(int max)
    {
        for (int i = 0; i < _actionTokens.Length; i++)
        {
            _actionTokens[i].enabled = i < max;
        }
        _maxTokens = max;
        _remainingToken = Mathf.Clamp(_remainingToken, 0, _maxTokens);
    }
    
    public void SetTokenAmount(int amount)
    {
        if (_remainingToken < 0) return;
        
        int diff = amount - _remainingToken;

        if (diff == 0) return;

        StopPrevisToken();
        
        if (diff > 0)
            RegenerateToken(Mathf.Abs(diff));
        else
            ConsumeToken(Mathf.Abs(diff));
    }

    void ConsumeToken(int consume = 1)
    {
        Sequence sequence = DOTween.Sequence();

        RectTransform slashRect = _slashSprite.rectTransform;
        for (int i = 0; i < consume; i++)
        {
            if (_remainingToken <= 0) continue;
            Image token = _actionTokens[_remainingToken - 1];
            Vector2 tokenPosition = token.rectTransform.localPosition;
            tokenPosition.x += token.rectTransform.sizeDelta.x * token.rectTransform.pivot.x;
            tokenPosition.y -= token.rectTransform.sizeDelta.y * token.rectTransform.pivot.y;

            sequence.AppendCallback(() =>
            {
                slashRect.localPosition = tokenPosition;
            });
            sequence.AppendCallback(() => { _slashSprite.enabled = true; });
            sequence.Append(
                _slashSprite.rectTransform.DOSizeDelta(new Vector2(1f, 1f), .15f).From(new Vector2(1.5f, 0f)).SetEase(Ease.OutCubic));
            sequence.Append(token.rectTransform.DOShakePosition(.25f, 1f, 25).SetEase(Ease.OutCubic));
            sequence.JoinCallback(() => { token.sprite = _consumedTokenSprite; });
            sequence.Append(
                _slashSprite.rectTransform.DOSizeDelta(new Vector2(0f, 1.5f), .15f).SetEase(Ease.InCubic));
            sequence.AppendCallback(() => { _slashSprite.enabled = false; });

            _remainingToken--;
        }
    }

    void RegenerateToken(int regenerate = 1)
    {
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < regenerate; i++)
        {
            if (_remainingToken >= _maxTokens) continue;
            _remainingToken++;
            Image token = _actionTokens[_remainingToken - 1];

            sequence.Append(token.rectTransform.DOPunchPosition(new Vector3(0, .5f, 0), .15f));
            sequence.JoinCallback(() => { token.sprite = _tokenSprite; });
        }
    }

    public void PrevisToken(int amount)
    {
        for (int i = 1; i <= amount; i++)
        {
            int previsID = _remainingToken - i;
            if (previsID >= _actionTokens.Length || previsID < 0) continue;
            
            RectTransform token = _actionTokens[previsID].rectTransform;
            _previsTokens.Add(token);
            token.DOLocalMoveY(.5f, .15f);
        }
    }
    
    
    public void StopPrevisToken()
    {
        foreach (RectTransform token in _previsTokens)
        {
            token.DOLocalMoveY(0f, .15f);
        }
        _previsTokens.Clear();
    }
}
