using Sirenix.OdinInspector;
using System;
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
    private EEquipmentType _previousEquipmentType;
    
    void Start()
    {
        _pawn.OnStunned += OnStunned;
        _pawn.OnUnstunned += OnUnstunned;
        _pawn.OnHealthChanged += HealthChange;
        
        if (_pawn.hp < 0) return; // No Hp ! 
        HealthChange(_pawn.hp);
    }

    public void HealthChange(int health)
    {
        UpdateText();
    }
    
    public void OnStunned()
    {
        UpdateText();
    }

    public void OnUnstunned()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        String text = "";
        if (_pawn.IsStunned) text += "STUNNED\n";
        
        if (_pawn.hp >= 0) { 
            text += $"HP: {_pawn.hp}";
        }
        
        _debugTxt.text = text;
    }

    public void UpdatePawnVisuals()
    {
        // Equipment Type
        EEquipmentType newEquipmentType = _pawn.equipmentType;
        if(_previousEquipmentType != _pawn.equipmentType)
        {
            if (_pawn.equipment)
            {
                DestroyImmediate(_pawn.equipment.gameObject);
            }
            GEquipment equipmentPrefab = _instantiationData.equipmentTypeData[_pawn.equipmentType];
            if (equipmentPrefab)
            {
                _pawn.equipment = PrefabUtility.InstantiatePrefab(equipmentPrefab) as GEquipment;
                _pawn.equipment.transform.parent = _pawn._equipmentParentTr;
                _pawn.equipment.transform.localPosition = Vector3.zero;
                _pawn.equipment.SetOwner(_pawn);
                EditorUtility.SetDirty(_pawn.equipment);
            }
            _previousEquipmentType = newEquipmentType;
        }
        EditorUtility.SetDirty(this);
    }
}
