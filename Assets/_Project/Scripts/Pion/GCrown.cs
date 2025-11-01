using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GCrown : GEquipment
{
    public int _baseDamage = 2;
    public int _bonusPassDamage = 1;

    [SerializeField, HideInEditorMode]
    public int currentDamage { get; private set; }
    
    void Start()
    {
        currentDamage = _baseDamage;
        GHudManager.Instance.playMenu.SetCrownDamageText(currentDamage);
    }

    public void ResetCrown()
    {
        currentDamage = _baseDamage;
       
    }

    public void OnPass()
    {
        currentDamage += _bonusPassDamage;
    }
    
}