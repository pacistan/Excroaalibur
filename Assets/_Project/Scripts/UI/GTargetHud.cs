using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ BoxGroup("Components")]
public class GTargetHud : MonoBehaviour
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

    [SerializeField]
    List<GUpgradeIcon> _upgradeIcons;

    [SerializeField]
    LocalizeStringEvent _upgradeDescriptionText;
    
    [SerializeField]
    LocalizeStringEvent _upgradeNameText;
    
    [SerializeField]
    GameObject _upgradeDetailsPanel;

    [SerializeField]
    float _upgradeDetailsPanelOffsetX;

    GGridObject _previousGridObject;
    private Sequence _tweenSequence;
    private int _currentUpgradeIconIndex;
    
    // Reparenting fields for canvas switching
    private Transform _originalParent;
    private Canvas _originalCanvas;
    private bool _isReparented = false;
    
    // Track subscribed pawns to avoid duplicate subscriptions
    private HashSet<GPawn> _subscribedPawns = new HashSet<GPawn>();
    
    // Track if this is the first action for hover display
    public bool isFirstAction = true;


    private void Start()
    {
        // Store original parent and canvas references
        _originalParent = transform.parent;
        _originalCanvas = GetComponentInParent<Canvas>();
        
        for (int i = 0; i < _upgradeIcons.Count; i++)
        {
            var upgradeIcon = _upgradeIcons[i];
            upgradeIcon.index = i;
            upgradeIcon.OnUpgradeIconHovered += OnUpgradeIconHovered;
            upgradeIcon.OnUpgradeIconUnhovered += OnUpgradeIconUnhovered;
        }
        
        // Subscribe to game state changes
        GGameManager.Instance.OnChangeMacroStateEvent += OnGameStateChanged;
        
        // Subscribe to upgrade changes for all existing pawns
        RefreshPawnUpgradeSubscriptions();
    }

    private void OnApplicationQuit()
    {
        for (int i = 0; i < _upgradeIcons.Count; i++)
        {
            var upgradeIcon = _upgradeIcons[i];
            upgradeIcon.index = i;
            upgradeIcon.OnUpgradeIconHovered -= OnUpgradeIconHovered;
            upgradeIcon.OnUpgradeIconUnhovered -= OnUpgradeIconUnhovered;
        }
        
        // Unsubscribe from game state changes
        if (GGameManager.Instance != null)
        {
            GGameManager.Instance.OnChangeMacroStateEvent -= OnGameStateChanged;
        }
        
        // Unsubscribe from all pawn upgrade changes
        UnsubscribeFromAllPawnUpgradeChanges();
    }
    
    private void RefreshPawnUpgradeSubscriptions()
    {
        Debug.Log("GTargetHud: Refreshing pawn upgrade subscriptions");
        
        // Subscribe to all current pawns
        GPawn[] allPawns = FindObjectsOfType<GPawn>();
        Debug.Log($"GTargetHud: Found {allPawns.Length} pawns");
        
        foreach (GPawn pawn in allPawns)
        {
            if (!_subscribedPawns.Contains(pawn))
            {
                pawn.OnUpgradeChanged += OnPawnUpgradeChanged;
                _subscribedPawns.Add(pawn);
                Debug.Log($"GTargetHud: Subscribed to upgrade changes for pawn {pawn.name}");
            }
        }
    }
    
    private void UnsubscribeFromAllPawnUpgradeChanges()
    {
        foreach (GPawn pawn in _subscribedPawns)
        {
            if (pawn != null)
            {
                pawn.OnUpgradeChanged -= OnPawnUpgradeChanged;
            }
        }
        _subscribedPawns.Clear();
    }
    
    private void OnPawnUpgradeChanged()
    {
        Debug.Log($"GTargetHud: OnPawnUpgradeChanged called. Previous grid object: {_previousGridObject}");
        
        // Refresh the hover display if we're currently showing a pawn
        if (_previousGridObject is GPawn currentPawn)
        {
            Debug.Log($"GTargetHud: Refreshing upgrade display for pawn {currentPawn.name}");
            // Only update the upgrade info without triggering full tween rebuild
            UpdateGridObjectHoveredInfo(currentPawn, isFirstAction);
        }
        else
        {
            Debug.Log("GTargetHud: No pawn currently being displayed");
        }
    }
    
    // Public method to manually refresh display for a specific pawn
    public void RefreshPawnDisplay(GPawn pawn)
    {
        Debug.Log($"GTargetHud: Manual refresh requested for pawn {pawn.name}");
        if (pawn == _previousGridObject)
        {
            Debug.Log($"GTargetHud: Pawn matches current display, refreshing info only");
            UpdateGridObjectHoveredInfo(pawn, isFirstAction);
        }
        else
        {
            Debug.Log($"GTargetHud: Pawn doesn't match current display, setting as new display");
            OnGridObjectHovered(pawn, isFirstAction);
        }
    }
    
    // Called when a new pawn is registered in the game
    private void OnPawnRegistered(GPawn pawn)
    {
        if (!_subscribedPawns.Contains(pawn))
        {
            pawn.OnUpgradeChanged += OnPawnUpgradeChanged;
            _subscribedPawns.Add(pawn);
        }
    }
    
    // Called when a pawn is unregistered from the game
    private void OnPawnUnregistered(GPawn pawn)
    {
        if (_subscribedPawns.Contains(pawn))
        {
            pawn.OnUpgradeChanged -= OnPawnUpgradeChanged;
            _subscribedPawns.Remove(pawn);
        }
    }
    
    private void OnGameStateChanged(EMacroStates newState, EMacroStates oldState)
    {
        if (newState == EMacroStates.Upgrade_Select_Character)
        {
            ReparentToActiveCanvas();
        }
        else if (oldState == EMacroStates.Upgrade_Select_Character)
        {
            RestoreOriginalParent();
        }
    }
    
    private void ReparentToActiveCanvas()
    {
        if (_isReparented) return;
        
        // Find the active menu canvas for upgrade character selection
        var menuManager = GMenuManager.Instance;
        if (menuManager != null)
        {
            // Try to find the upgrade character select menu directly
            var upgradeCharacterMenu = FindObjectOfType<GUpgradeCharacterSelectMenu>();
            if (upgradeCharacterMenu != null)
            {
                var activeCanvas = upgradeCharacterMenu.GetComponentInParent<Canvas>();
                if (activeCanvas != null && activeCanvas != _originalCanvas)
                {
                    transform.SetParent(activeCanvas.transform, false);
                    _isReparented = true;
                }
            }
        }
    }
    
    private void RestoreOriginalParent()
    {
        if (!_isReparented) return;
        
        transform.SetParent(_originalParent, false);
        _isReparented = false;
    }

    public void OnGridObjectHovered(GGridObject gridObject, bool  isFirstAction)
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
            actionList.UpdateButtons(pawn, isFirstAction);
            _panelRmbIndicator.SetActive(pawn && pawn.data.isPlayer);

            _headerHasCrownIconImage.sprite = pawn && pawn.equipment && pawn.equipment is GCrown
                ? _headerCrownIconSprite
                : _headerNoCrownIconSprite;

            _headerHasCrownIconImage.color = gridObject.pawnColor;

            _headerNameText.text = gridObject.headerName;
            _classNameText.text = $"Class : {gridObject.className}";

            foreach (var image in _actionTokenImgArray)
            {
                image.gameObject.SetActive(pawn && pawn.data.isPlayer);
            }

            _upgradeIcons.ForEach(i => i.gameObject.SetActive(false));

            if (pawn)
            {
                if (pawn.data.isPlayer)
                {
                    for (int i = 0; i < _actionTokenImgArray.Length; i++)
                    {
                        //_actionTokenImgArray[i].gameObject.SetActive(i <= pawn.data.actionTokens);
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
                    _aiHpNumberText.text = $"{pawn.hp}/{pawn.AttributesController.GetFinal(EAttributeType.MaxHealth)} HPs";
                }

                var upgrades = pawn.upgrades;
                int maxUpgradeShown = Math.Min(_upgradeIcons.Count, upgrades.Count);
                for (int i = 0; i < maxUpgradeShown; i++)
                {
                    _upgradeIcons[i].SetUpgradeIcon(upgrades[i].Icon);
                    _upgradeIcons[i].gameObject.SetActive(true);
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

    public void UpdateActionList()
    {
        
    }
    
    public void UpdateGridObjectHoveredInfo(GGridObject gridObject, bool isFirstAction)
    {
        GPawn pawn = gridObject as GPawn;
        actionList.UpdateButtons(pawn, isFirstAction);
        
        _headerHasCrownIconImage.sprite = pawn && pawn.equipment && pawn.equipment is GCrown ?
            _headerCrownIconSprite : _headerNoCrownIconSprite;

        foreach (var image in _actionTokenImgArray)
        {
            image.gameObject.SetActive(pawn && pawn.data.isPlayer);
        }
        if (pawn)
        {
            if (pawn.data.isPlayer)
            {
                for (int i = 0; i < _actionTokenImgArray.Length; i++)
                {
                    //_actionTokenImgArray[i].gameObject.SetActive(i < pawn.data.actionTokens);
                    bool isActionTokenOn = i < pawn.remainingActionToken;
                    _actionTokenImgArray[i].sprite = isActionTokenOn ?
                        _actionTokenOnSprite : _actionTokenOffSprite;
                    _actionTokenImgArray[i].color = isActionTokenOn ?
                        _actionTokenOnColor : _actionTokenOffColor;
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
                _aiHpNumberText.text = $"{pawn.hp}/{pawn.AttributesController.GetFinal(EAttributeType.MaxHealth)} HPs";
            }

            // Update upgrade icons
            _upgradeIcons.ForEach(i => i.gameObject.SetActive(false));
            
            var upgrades = pawn.upgrades;
            int maxUpgradeShown = Math.Min(_upgradeIcons.Count, upgrades.Count);
            for (int i = 0; i < maxUpgradeShown; i++)
            {
                _upgradeIcons[i].SetUpgradeIcon(upgrades[i].Icon);
                _upgradeIcons[i].gameObject.SetActive(true);
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

    private void OnUpgradeIconHovered(int index)
    {
        if(_currentUpgradeIconIndex == index) return;
        _upgradeDetailsPanel.SetActive(true);
        _currentUpgradeIconIndex = index;
        if(_previousGridObject is GPawn pawn)
        {
            var upgrade = pawn.upgrades[index];
            var upgradeIcon = _upgradeIcons[index].transform;
            _upgradeNameText.StringReference.SetReference(upgrade.Name.TableReference, upgrade.Name.TableEntryReference);
            _upgradeDescriptionText.StringReference.SetReference(upgrade.Description.TableReference, upgrade.Description.TableEntryReference);
            _upgradeDetailsPanel.transform.position = upgradeIcon.position + _upgradeDetailsPanelOffsetX * Vector3.right;
        }
    }

    private void OnUpgradeIconUnhovered(int index)
    {
        if(_currentUpgradeIconIndex != index) return;
        _currentUpgradeIconIndex = -1;
        _upgradeDetailsPanel.SetActive(false);
    }
}
