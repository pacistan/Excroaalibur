using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ BoxGroup("Components")]
public class GPlayerHudManager : MonoBehaviour
{
    [field: SerializeField,  BoxGroup("Components")]
    public GActionList actionList { get; private set; }

    [SerializeField, BoxGroup("Components")]
    private Image[] _actionTokenImgArray;
    
    [SerializeField, BoxGroup("Components")]
    private CanvasGroup _canvasGroup;
    
    [SerializeField, BoxGroup("Components")]
    [FoldoutGroup("Components/Image")]
    private Image _headerPanelImage;
    
    [SerializeField,  BoxGroup("Components")]
    [FoldoutGroup("Components/Image")]
    private Image _cadreImage;
    
    [SerializeField,  BoxGroup("Components")]
    [FoldoutGroup("Components/Image")]
    private Image _headerObjectIconImage;
    
    [SerializeField,  BoxGroup("Components")]
    [FoldoutGroup("Components/Image")]
    private Image _headerHasCrownIconImage;
    
    [SerializeField,  BoxGroup("Components")]
    [FoldoutGroup("Components/Text")]
    private TextMeshProUGUI _headerNameText;

    [SerializeField,  BoxGroup("Components")]
    [FoldoutGroup("Components/Text")]
    private TextMeshProUGUI _classNameText;

    [SerializeField, BoxGroup("Components")]
    [FoldoutGroup("Components/Text")]
    TextMeshProUGUI _aiHpNumberText;
    
    [SerializeField, BoxGroup("Components")]
    GameObject _panelRmbIndicator;
    
    [SerializeField, BoxGroup("Sprites")]
    private Sprite _headerCrownIconSprite;
    
    [SerializeField, BoxGroup("Sprites")]
    private Sprite _headerNoCrownIconSprite;
    
    [SerializeField, BoxGroup("Sprites")]
    Sprite _actionTokenOnSprite;
    
    [SerializeField, BoxGroup("Sprites")]
    Sprite _actionTokenOffSprite;
    
    [SerializeField, BoxGroup("Sprites")]
    Color _actionTokenOnColor;
    
    [SerializeField, BoxGroup("Sprites")]
    Color _actionTokenOffColor;

    [FormerlySerializedAs("_movementDuration")]
    [SerializeField, BoxGroup("Panel Movement")]
    float _inMovementDuration, _outMovementDuration;
    
    [FormerlySerializedAs("_onPosition")]
    [SerializeField, BoxGroup("Panel Movement")]
    Vector2 _inPosition;

    [FormerlySerializedAs("_offPosition")]
    [SerializeField, BoxGroup("Panel Movement")]
    Vector2 _outPosition;

    [FormerlySerializedAs("_movementCurve")]
    [SerializeField, BoxGroup("Panel Movement")]
    AnimationCurve _inMovementCurve, _outMovementCurve;

    [SerializeField, BoxGroup("Panel Movement")]
    RectTransform _leftPanelRectTransform;

    GGridObject _previousGridObject;
    
    private Sequence _tweenSequence;

    public void OnGridObjectHovered(GGridObject gridObject)
    {
        if ((!gridObject && !_previousGridObject) || _previousGridObject == gridObject) return;
        
        if (_tweenSequence != null && _tweenSequence.IsPlaying())
        {
            _tweenSequence.Kill();
            _tweenSequence = null;
        }
        
        TweenCallback callback = () =>
        {
            _headerPanelImage.sprite = gridObject.headerSprite;
            _cadreImage.sprite = gridObject.cadreSprite;
            _headerObjectIconImage.sprite = gridObject.headerObjectIconSprite;
            _headerObjectIconImage.color = gridObject.pawnColor;

            _cadreImage.gameObject.SetActive(gridObject.cadreSprite != null);

            GPawn pawn = gridObject as GPawn;
            actionList.UpdateButtons(pawn);
            _panelRmbIndicator.SetActive(pawn && pawn.isPlayer);

            _headerHasCrownIconImage.sprite = pawn && pawn.equipment && pawn.equipment is GCrown
                ? _headerCrownIconSprite
                : _headerNoCrownIconSprite;

            _headerHasCrownIconImage.color = gridObject.pawnColor;

            _headerNameText.text = gridObject.headerName;
            _classNameText.text = $"Class : {gridObject.className}";

            if (pawn)
            {
                if (pawn.isPlayer)
                {
                    for (int i = 0; i < _actionTokenImgArray.Length; i++)
                    {
                        //_actionTokenImgArray[i].gameObject.SetActive(i <= pawn.actionTokens);
                        bool isActionTokenOn = i < pawn.remainingActionToken;
                        _actionTokenImgArray[i].sprite = isActionTokenOn ? _actionTokenOnSprite : _actionTokenOffSprite;
                        _actionTokenImgArray[i].color = isActionTokenOn ? _actionTokenOnColor : _actionTokenOffColor;
                    }
                    _aiHpNumberText.gameObject.SetActive(false);
                }
                else
                {
                    foreach (var image in _actionTokenImgArray)
                    {
                        image.gameObject.SetActive(false);
                    }
                    _aiHpNumberText.gameObject.SetActive(true);
                    _aiHpNumberText.text = $"{pawn.hp}/{pawn.startHp} HPs";
                }
            }
        };
        
        if (!gridObject) 
        {
            ShowPanel(false);
            //_tweenSequence.AppendCallback(callback);
            _previousGridObject = null;
            return;
        }
        else if(!_previousGridObject)
        {
            _previousGridObject = gridObject;
            if (_tweenSequence == null)
            {
                _tweenSequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, false);
                _tweenSequence.OnComplete(() => _tweenSequence = null);
            }
            _tweenSequence.AppendCallback(callback);
            ShowPanel(true);
        }
        else
        {
            _previousGridObject = gridObject;
            ShowPanel(false);
            _tweenSequence.AppendCallback(callback);
            ShowPanel(true);
        }
        

    }
    
    

    public void UpdateGridObjectHoveredInfo(GGridObject gridObject)
    {
        GPawn pawn = gridObject as GPawn;
        actionList.UpdateButtons(pawn);
        
        _headerHasCrownIconImage.sprite = pawn && pawn.equipment && pawn.equipment is GCrown ?
            _headerCrownIconSprite : _headerNoCrownIconSprite;

        if (pawn)
        {
            if (pawn.isPlayer)
            {
                for (int i = 0; i < _actionTokenImgArray.Length; i++)
                {
                    _actionTokenImgArray[i].gameObject.SetActive(i < pawn.actionTokens);
                    bool isActionTokenOn = i < pawn.remainingActionToken;
                    _actionTokenImgArray[i].sprite = isActionTokenOn ?
                        _actionTokenOnSprite : _actionTokenOffSprite;
                    _actionTokenImgArray[i].color = isActionTokenOn ?
                        _actionTokenOnColor : _actionTokenOffColor;
                }
            }
            else
            {
                _aiHpNumberText.text = $"{pawn.hp}/{pawn.startHp} HPs";
            }
        }
    }
    
    public void ShowPanel(bool isShow)
    {
        if (_tweenSequence == null)
        {
            _tweenSequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, false);
            _tweenSequence.OnComplete(() => _tweenSequence = null);
        }
        Vector3 targetPos = isShow ? _inPosition : _outPosition;
        float targetFade = isShow ? 1f : 0f;
        float duration = isShow ? _inMovementDuration : _outMovementDuration;
        AnimationCurve curve = isShow ? _inMovementCurve : _outMovementCurve;
        
        _tweenSequence.Append(_leftPanelRectTransform.DOAnchorPos(targetPos, duration).SetEase(curve));
        _tweenSequence.Join(_canvasGroup.DOFade(targetFade, duration).SetEase(curve));
    }
}
