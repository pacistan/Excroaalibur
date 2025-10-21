using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GCrown : GEquipment
{
    [FormerlySerializedAs("damage")]
    public int _baseDamage = 2;
    
    public int _bonusPassDamage = 1;

    [SerializeField, HideInEditorMode]
    public int _currentDamage { get; private set; }

    void Start()
    {
        _currentDamage = _baseDamage;
    }

    public void ResetCrown()
    {
        _currentDamage = _baseDamage;
    }

    public void OnPass()
    {
        _currentDamage += _bonusPassDamage;
    }
    
}