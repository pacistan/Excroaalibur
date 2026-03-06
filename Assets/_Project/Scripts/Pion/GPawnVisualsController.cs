using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(GPawn))]
public class GPawnVisualsController : SerializedMonoBehaviour
{
    [SerializeField, FoldoutGroup("Components")]
    GPawn _pawn;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas")]
    Canvas _worldCanvas;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/HP"), HideIf("isPlayerAccessor")]
    GameObject _hpContainer;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/HP"), HideIf("isPlayerAccessor")]
    TextMeshProUGUI _txtCurrentHp;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/HP"), HideIf("isPlayerAccessor")]
    TextMeshProUGUI _txtMaxHp;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/HP"), HideIf("isPlayerAccessor")]
    Color _txtHpNormalColor;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/HP"), HideIf("isPlayerAccessor")]
    Color _txtHpPrevisualisationColor;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/HP"), HideIf("isPlayerAccessor")]
    Image _imgHpBarForeground;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/HP"), HideIf("isPlayerAccessor")]
    Image _imgHpBarPrevisualisation;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/Status")]
    Image _imgStatus;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/Status")]
    GameObject _imgStatusContainer;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/Status")]
    Sprite _spriteStun;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/Status")]
    Sprite _spriteStunProtected;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/Status"), HideIf("isPlayerAccessor")]
    Sprite _spriteDeath;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/ActionTokens"), ShowIf("isPlayerAccessor")]
    ActionToken_Container _actionTokenContainer;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/ActionTokens"), ShowIf("isPlayerAccessor")]
    List<Image> _imgListActionTokens;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas/ActionTokens"), ShowIf("isPlayerAccessor")]
    Sprite _spriteActionTokenOn, _spriteActionTokenOff;
    
    [SerializeField, FoldoutGroup("Components") ]
    private GCommonInstantiationData _instantiationData;
    
    [SerializeField, HideInInspector]
    private bool _previousHasCrown;

    [SerializeField, FoldoutGroup("Components")]
    Renderer _mainRenderer;
    
    [SerializeField, FoldoutGroup("Components")]
    Material _stunnedMaterial;

    [SerializeField, FoldoutGroup("Components")]
    Animator _animator;
    
    [SerializeField]
    int _stunMaterialIndex = 0;

    public Animator GetAnimator() => _animator;
    
    bool isPlayerAccessor {
    get
    {
        return _pawn && _pawn.data ? _pawn.data.isPlayer : true;
    }}
    
    [FormerlySerializedAs("OnStunUnityEvent")]
    [SerializeField, FoldoutGroup("Events")]
    UnityEvent OnStunStartEvent;
    
    [SerializeField, FoldoutGroup("Events")]
    UnityEvent OnStunStopEvent;
    
    [SerializeField, FoldoutGroup("Events")]
    UnityEvent OnPushEvent;
    
    [SerializeField, FoldoutGroup("Events")]
    UnityEvent OnDamagedUnityEvent;
    
    Material _defaultMaterial;
    
    int _previousHpNumber;
    int _previousStunTurnNumber;

    
    void Start()
    {
        if (!_pawn.data.isPlayer && !(_pawn is GAltar))
        {
            if (_pawn.AttributesController.Has(EAttributeType.MaxHealth))
            {
                _txtCurrentHp.text = $"{_pawn.hp}";
                _txtMaxHp.text = $"/{_pawn.AttributesController.GetFinal(EAttributeType.MaxHealth)}";
                _imgHpBarForeground.fillAmount = 1;
                _imgHpBarPrevisualisation.fillAmount = 1;
            }
            else
            {
                _hpContainer.SetActive(false);
            }
        }
        else if(_pawn.data.isPlayer)
        {
            UpdateActionPointUI(0, _pawn.AttributesController.GetFinal(EAttributeType.MaxAction));
            _pawn.AttributesController.SubscribeCallBack(EAttributeType.MaxAction, UpdateActionPointUI);
        }
        _stunMaterialIndex = Mathf.Min(_mainRenderer.materials.Length, _stunMaterialIndex);
        _defaultMaterial = _mainRenderer.materials[_stunMaterialIndex];
        
    }

    void UpdateActionPointUI(float oldAttributeValue, float newAttributeValue)
    {
        _actionTokenContainer.SetMaxTokenAmount(Mathf.FloorToInt(newAttributeValue));
    }
    
    void OnEnable()
    {
        _pawn.OnStunned += OnStunned;
        _pawn.OnUnstunned += OnUnstunned;
        _pawn.OnHealthChanged += HealthChange;
        _pawn.OnKill += OnKilledVisuals;
        _pawn.OnAnimationPush += OnPush;
    }

    void OnDisable()
    {
        _pawn.OnStunned -= OnStunned;
        _pawn.OnUnstunned -= OnUnstunned;
        _pawn.OnHealthChanged -= HealthChange;
        _pawn.OnKill -= OnKilledVisuals;
        _pawn.OnAnimationPush -= OnPush;
    }

    public bool HasAnimations()
    {
        return _animator != null;
    }
    
    /** Walking / Idle / Push / Throw */
    public void SetAnimationState(string animationStateName, float transitionDuration = .1f)
    {
        if (!_animator)
        {
            Debug.LogWarning("Animator is not set");
            return;
        }

        // TODO : Rework that !!!
        if (animationStateName == GPawn.IdleAnimationName)
        {
            animationStateName = _pawn.equipment ? "Idle_Sword" : GPawn.IdleAnimationName;
        }
        
        _animator.CrossFade(animationStateName, transitionDuration);
    }

    public void SetAnimationParameter(string animationParameterName, bool value)
    {
        if (!_animator)
        {
            Debug.LogWarning("Animator is not set");
            return;
        }
        _animator.SetBool(animationParameterName, value);        
    }
    
    public void HealthChange(float oldValue, float newValue)
    {
    }
    
    public void OnStunned()
    {
    }

    public void OnUnstunned()
    {
    }
    
    public void OnPush()
    {
        OnPushEvent?.Invoke();
    }

    public void OnUpdateHealthPoints()
    {
        if (_pawn.data.isPlayer || _pawn is GAltar) return;
        string text = "";
        
        if (_pawn.hp >= 0 && _pawn.hp != _previousHpNumber) 
        { 
            text += $"{_pawn.hp}";
            // TODO : Start Take Damage Feedbacks
            OnDamagedUnityEvent?.Invoke();

            RuntimeManager.PlayOneShotAttached("event:/Pawn/Damaged", gameObject);
        }
        
        _previousHpNumber = _pawn.hp;
        _txtCurrentHp.text = text;
        _imgHpBarForeground.fillAmount = _pawn.GetHpRatio();
        _imgHpBarPrevisualisation.fillAmount = _pawn.GetHpRatio();
        _hpContainer.transform.DOShakePosition(1f).SetEase(Ease.InCubic);
        _hpContainer.transform.DOShakeRotation(1f).SetEase(Ease.InCubic);
    }

    public void OnUpdateActionsToken()
    {
        _actionTokenContainer.SetTokenAmount(Mathf.FloorToInt(_pawn.remainingActionToken));
    }

    public void OnInitActionsToken()
    {
        _actionTokenContainer.initTokens(Mathf.FloorToInt(_pawn.AttributesController.GetFinal(EAttributeType.MaxAction)),Mathf.FloorToInt(_pawn.remainingActionToken));
    }
    
    public void OnUpdateStunTurn()
    {
        if (_pawn.isStunnedProtected)
        {
            _imgStatus.sprite = _spriteStunProtected;
            _imgStatusContainer.SetActive(true);
        }
        
        if (_pawn.stunTurns > 0 && _previousStunTurnNumber != _pawn.stunTurns) // Start Stun 
        {
            List<Material> materials = _mainRenderer.materials.ToList();
            materials[_stunMaterialIndex] = _stunnedMaterial;
            _mainRenderer.SetMaterials(materials);
            _imgStatus.sprite = _spriteStun;
            _imgStatusContainer.SetActive(true);
            OnStunStartEvent?.Invoke();
            SetAnimationParameter(GPawn.AnimParam_IsStunned, true);
        }
        else if (_pawn.stunTurns == 0 && _previousStunTurnNumber != _pawn.stunTurns) // Stop Stun 
        {
            List<Material> materials = _mainRenderer.materials.ToList();
            materials[_stunMaterialIndex] = _defaultMaterial;
            _mainRenderer.SetMaterials(materials);
            OnStunStopEvent?.Invoke();
            SetAnimationParameter(GPawn.AnimParam_IsStunned, false);
        }

        if (_pawn.stunTurns == 0 && !_pawn.isStunnedProtected)
        {
            _imgStatusContainer.SetActive(false);
        }
        
        _previousStunTurnNumber = _pawn.stunTurns;
    }

    public void OnPrevisualisation(int damage, int stunTurns, int actionGain)
    {
        if (_pawn is GAltar) return;
        int tempStun = (_pawn.stunTurns > 0 || stunTurns > 0) && !_pawn.isStunnedProtected ? Mathf.Max(_pawn.stunTurns, stunTurns) : 0;
        bool isDead = false;
        if (!_pawn.data.isPlayer)
        {
            int tempHp = Mathf.Max(0, _pawn.hp - damage);
            _imgHpBarForeground.fillAmount = _pawn.GetHpRatio();
            _txtCurrentHp.text = $"{tempHp}";
            if (tempHp != _pawn.hp)
            {
                _txtCurrentHp.color = _txtHpPrevisualisationColor;
            }
            if (tempHp == 0)
            {
                _imgStatus.sprite = _spriteDeath;
                _imgStatusContainer.SetActive(true);
                isDead = true;
            }
        }
        else
        {
            int tempActionPoints = actionGain + _pawn.remainingActionToken - 1;
            _actionTokenContainer.PrevisToken(1);
        }
        if (tempStun > 0 && !isDead)
        {
            _imgStatus.sprite = _spriteStun;
            _imgStatusContainer.SetActive(true);
        }
    }

    public void OnDisablePrevisualisation()
    {
        if (_pawn is GAltar) return;
        if (!_pawn.data.isPlayer)
        {
            _imgHpBarForeground.fillAmount = _pawn.GetHpRatio();
            _txtCurrentHp.text = $"{_pawn.hp}";
            _txtCurrentHp.color = _txtHpNormalColor;
        }
        else
        {
            _actionTokenContainer.StopPrevisToken();
        }
        
        if (_pawn.stunTurns > 0)
        {
            _imgStatus.sprite = _spriteStun;
            _imgStatusContainer.SetActive(true);
        }
        else if (_pawn.isStunnedProtected)
        {
            _imgStatus.sprite = _spriteStunProtected;
            _imgStatusContainer.SetActive(true);
        }
        else
        {
            _imgStatusContainer.SetActive(false);
        }
    }
    
    #if UNITY_EDITOR
    public void SetClickable(SceneVisibilityManager manager, bool value)
    {
        if (value)
        {
            manager.EnablePicking(_worldCanvas.gameObject, true);
        }
        else
        {
            manager.DisablePicking(_worldCanvas.gameObject, true);
        }
    }   
    #endif
    
    private void OnKilledVisuals()
    {
        // TODO : Start On Kill Feedbacks
        RuntimeManager.PlayOneShot("event:/Pawn/Die", gameObject.transform.position);
    }
    

    #if UNITY_EDITOR
    public void UpdatePawnVisuals()
    {
        if(_previousHasCrown != _pawn.hasCrown)
        {
            if (_pawn.equipment)
            {
                DestroyImmediate(_pawn.equipment.gameObject);
            }
            else
            {
                GEquipment equipmentPrefab = _instantiationData.objectTypeData[EGridObjectType.Crown] as GEquipment;
                if (equipmentPrefab)
                {
                    // TODO Check ! 
                    _pawn.equipment = PrefabUtility.InstantiatePrefab(equipmentPrefab) as GEquipment;
                    _pawn.equipment.transform.parent = _pawn.equipmentParentTr;
                    _pawn.equipment.transform.localPosition = Vector3.zero;
                    _pawn.equipment.owner = _pawn;
                    EditorUtility.SetDirty(_pawn.equipment);
                }
            }
            _previousHasCrown = _pawn.hasCrown;
            EditorUtility.SetDirty(this);
        }
    }
    #endif
}
