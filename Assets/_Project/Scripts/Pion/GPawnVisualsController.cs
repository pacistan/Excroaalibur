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
    int _health = 0;
    bool _isStunned = false;
    
    [SerializeField, FoldoutGroup("Components") ]
    private GCommonInstantiationData _instantiationData;
    
    [SerializeField, HideInInspector]
    private bool _previousHasCrown;
    
    void Start()
    {
        _pawn.OnStunned += OnStunned;
        _pawn.OnUnstunned += OnUnstunned;
        _pawn.OnHealthChanged += HealthChange;
        HealthChange(_pawn.hp);
    }

    public void HealthChange(int health)
    {
        _health = health;
        UpdateText();
    }
    
    public void OnStunned()
    {
        _isStunned = true;
        UpdateText();
    }

    public void OnUnstunned()
    {
        _isStunned = false;
        UpdateText();
    }

    private void UpdateText()
    {
        String text = "";
        if (_isStunned) text += "STUNNED\n";
        
        if (_health >= 0) { 
            text += $"HP: {_health}";
        }
        
        _debugTxt.text = text;
    }

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
                    _pawn.equipment.transform.parent = _pawn._equipmentParentTr;
                    _pawn.equipment.transform.localPosition = Vector3.zero;
                    _pawn.equipment.owner = _pawn;
                    EditorUtility.SetDirty(_pawn.equipment);
                }
            }
            _previousHasCrown = _pawn.hasCrown;
            EditorUtility.SetDirty(this);
        }
    }
}
