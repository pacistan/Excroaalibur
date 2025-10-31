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
    [BoxGroup("Components/World Canvas"), HideIf("isPlayerAccessor")]
    TextMeshProUGUI _txtCurrentHp;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas"), HideIf("isPlayerAccessor")]
    TextMeshProUGUI _txtMaxHp;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas"), HideIf("isPlayerAccessor")]
    Color _txtHpNormalColor;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas"), HideIf("isPlayerAccessor")]
    Color _txtHpPrevisualisationColor;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas"), HideIf("isPlayerAccessor")]
    Image _imgHpBarForeground;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas"), HideIf("isPlayerAccessor")]
    Image _imgHpBarPrevisualisation;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas")]
    Image _imgStatus;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas")]
    Sprite _spriteStun;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas"), HideIf("isPlayerAccessor")]
    Sprite _spriteDeath;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas"), ShowIf("isPlayerAccessor")]
    List<Image> _imgListactionTokens;
    
    [SerializeField, FoldoutGroup("Components")]
    [BoxGroup("Components/World Canvas"), ShowIf("isPlayerAccessor")]
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

    bool isPlayerAccessor {
    get
    {
        return _pawn ? _pawn.data.isPlayer : true;
    }}

    [SerializeField]
    UnityEvent OnStunUnityEvent, OnDamagedUnityEvent;
    
    Material _defaultMaterial;
    
    int _previousHpNumber;
    int _previousStunTurn;

    void Start()
    {
        if (!_pawn.data.isPlayer && !(_pawn is GAltar))
        {
            _txtCurrentHp.text = $"{_pawn.hp}";
            _txtMaxHp.text = $"/{_pawn.hp}";
            _imgHpBarForeground.fillAmount = 1;
            _imgHpBarPrevisualisation.fillAmount = 1;
        }
        _stunMaterialIndex = Mathf.Min(_mainRenderer.materials.Length, _stunMaterialIndex);
        _defaultMaterial = _mainRenderer.materials[_stunMaterialIndex];
    }

    void OnEnable()
    {
        _pawn.OnStunned += OnStunned;
        _pawn.OnUnstunned += OnUnstunned;
        _pawn.OnHealthChanged += HealthChange;
        _pawn.OnKill += OnKilledVisuals;
    }

    void OnDisable()
    {
        _pawn.OnStunned -= OnStunned;
        _pawn.OnUnstunned -= OnUnstunned;
        _pawn.OnHealthChanged -= HealthChange;
        _pawn.OnKill -= OnKilledVisuals;
    }

    /** Walking / Idle / Push / Throw */
    public void SetAnimationState(string animationStateName)
    {
        if (!_animator)
        {
            Debug.LogWarning("Animator is not set");
            return;
        }
        _animator.CrossFade(animationStateName, .3f);
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
    
    public void HealthChange()
    {
    }
    
    public void OnStunned()
    {
    }

    public void OnUnstunned()
    {
    }

    public void OnUpdateHealthPoints()
    {
        if (_pawn.data.isPlayer || _pawn is GAltar) return;
        string text = "";
        
        if (_pawn.hp >= 0 && _pawn.hp != _previousHpNumber) 
        { 
            text += $"{_pawn.hp}";
            //TODO : Start Take Damage Feedbacks
            OnDamagedUnityEvent?.Invoke();

            RuntimeManager.PlayOneShotAttached("event:/Pawn/Damaged", gameObject);
        }
        
        _previousHpNumber = _pawn.hp;
        _txtCurrentHp.text = text;
        _imgHpBarForeground.fillAmount = (float)_pawn.hp / (float)_pawn.data.startHp;
        _imgHpBarPrevisualisation.fillAmount = (float)_pawn.hp / (float)_pawn.data.startHp;
    }

    public void OnUpdateActionsToken()
    {
        _imgListactionTokens[0].sprite = _pawn.remainingActionToken >= 1 ?
            _spriteActionTokenOn : _spriteActionTokenOff;
        
        _imgListactionTokens[1].sprite = _pawn.remainingActionToken >= 2 ?
            _spriteActionTokenOn : _spriteActionTokenOff;
        
        //string text = "";
        //_txtCurrentHp.text = $"{_pawn.remainingActionToken}";
    }
    
    public void OnUpdateStunTurn()
    {
        if (_pawn.IsStunned && _previousStunTurn != _pawn.stunTurn)
        {
            List<Material> materials = _mainRenderer.materials.ToList();
            materials[_stunMaterialIndex] = _stunnedMaterial;
            _mainRenderer.SetMaterials(materials);
            _imgStatus.sprite = _spriteStun;
            _imgStatus.enabled = true;
            OnStunUnityEvent?.Invoke();
            //TODO : Start Stun Feedbacks
        }
        else if (!_pawn.IsStunned && _previousStunTurn != _pawn.stunTurn)
        {
            List<Material> materials = _mainRenderer.materials.ToList();
            materials[_stunMaterialIndex] = _defaultMaterial;
            _mainRenderer.SetMaterials(materials);
            _imgStatus.sprite = _spriteStun;
            _imgStatus.enabled = false;
            //TODO : Start UnStun Feedbacks            
        }
        
        _previousStunTurn = _pawn.stunTurn;
    }

    public void OnPrevisualisation(int damage, int stunTurns)
    {
        if (_pawn is GAltar) return;
        int tempStun = _pawn.stunTurn + stunTurns;
        bool isDead = false;
        if (!_pawn.data.isPlayer)
        {
            int tempHp = Mathf.Max(0, _pawn.hp - damage);
            _imgHpBarForeground.fillAmount = (float)tempHp / (float)_pawn.data.startHp;
            _txtCurrentHp.text = $"{tempHp}";
            if (tempHp != _pawn.hp)
            {
                _txtCurrentHp.color = _txtHpPrevisualisationColor;
            }
            if (tempHp == 0)
            {
                _imgStatus.sprite = _spriteDeath;
                _imgStatus.enabled = true;
                isDead = true;
            }
        }
        if (tempStun > 0 && !isDead)
        {
            _imgStatus.sprite = _spriteStun;
            _imgStatus.enabled = true;
        }
    }

    public void OnDisablePrevisualisation()
    {
        if (_pawn is GAltar) return;
        if (!_pawn.data.isPlayer)
        {
            _imgHpBarForeground.fillAmount = (float)_pawn.hp / (float)_pawn.data.startHp;
            _txtCurrentHp.text = $"{_pawn.hp}";
            _txtCurrentHp.color = _txtHpNormalColor;
        }
        if (_pawn.stunTurn > 0)
        {
            _imgStatus.sprite = _spriteStun;
            _imgStatus.enabled = true;
        }
        else
        {
            _imgStatus.enabled = false;
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
