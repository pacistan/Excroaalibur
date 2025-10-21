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
    private bool _previousHasCrown;
    
    int _previousHpNumber;
    int _previousStunTurn;
    
    void OnEnable()
    {
        _pawn.OnStunned += OnStunned;
        _pawn.OnUnstunned += OnUnstunned;
        _pawn.OnHealthChanged += HealthChange;
        _pawn.OnKill += OnKilledVisuals;
        OnUpdateHealthPoints();
        OnUpdateStunTurn();
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
            text += $"HP: {_pawn.hp}";
            //TODO : Start Take Damage Feedbacks
        }
        
        _previousHpNumber = _pawn.hp;
        _debugTxt.text = text;
    }
    
    public void OnUpdateStunTurn()
    {
        String text = "";
        if (_pawn.IsStunned && _previousStunTurn != _pawn.stunTurn)
        {
            text += "STUNNED\n";
            //TODO : Start Stun Feedbacks
        }
        else if (!_pawn.IsStunned && _previousStunTurn != _pawn.stunTurn)
        {
            //TODO : Start UnStun Feedbacks            
        }
        
        _previousStunTurn = _pawn.stunTurn;
        _debugTxt.text = text;
    }

    private void OnKilledVisuals()
    {
        // TODO : Start On Kill Feedbacks
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
    #endif
}
