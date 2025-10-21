using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(GPawn))]
public class GPawnVisualsController : SerializedMonoBehaviour
{
    [SerializeField, FoldoutGroup("Components")]
    GPawn _pawn;
    
    [SerializeField, FoldoutGroup("Components")]
    TextMeshProUGUI _debugTxt;
    
    [SerializeField, FoldoutGroup("Components") ]
    private GCommonInstantiationData _instantiationData;
    
    [SerializeField, HideInInspector]
    private bool _previousHasCrown;

    [SerializeField, FoldoutGroup("Components")]
    Renderer _mainRenderer;
    
    [SerializeField, FoldoutGroup("Components")]
    Material _stunnedMaterial;

    [SerializeField]
    int _stunMaterialIndex = 0;
    

    Material _defaultMaterial;
    
    int _previousHpNumber;
    int _previousStunTurn;

    void Start()
    {
        OnUpdateStunTurn();
        OnUpdateHealthPoints();
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
        string text = "";
        
        if (_pawn.hp >= 0 && _pawn.hp != _previousHpNumber) 
        { 
            text += $"{_pawn.hp}";
            //TODO : Start Take Damage Feedbacks

            RuntimeManager.PlayOneShotAttached("event:/Pawn/Damaged", gameObject);
        }
        
        _previousHpNumber = _pawn.hp;
        _debugTxt.text = text;
    }
    
    public void OnUpdateStunTurn()
    {
        if (_pawn.IsStunned && _previousStunTurn != _pawn.stunTurn)
        {
            List<Material> materials = _mainRenderer.materials.ToList();
            materials[_stunMaterialIndex] = _stunnedMaterial;
            _mainRenderer.SetMaterials(materials);
            //TODO : Start Stun Feedbacks
        }
        else if (!_pawn.IsStunned && _previousStunTurn != _pawn.stunTurn)
        {
            List<Material> materials = _mainRenderer.materials.ToList();
            materials[_stunMaterialIndex] = _defaultMaterial;
            _mainRenderer.SetMaterials(materials);
            //TODO : Start UnStun Feedbacks            
        }
        
        _previousStunTurn = _pawn.stunTurn;
    }

    #if UNITY_EDITOR
    public void SetClickable(SceneVisibilityManager manager, bool value)
    {
        if (value)
        {
            manager.EnablePicking(_debugTxt.transform.parent.gameObject, true);
        }
        else
        {
            manager.DisablePicking(_debugTxt.transform.parent.gameObject, true);
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
