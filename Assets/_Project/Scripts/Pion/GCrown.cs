using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Serialization;

    public class GCrown : GEquipment
    {
        public int _baseDamage = 2;

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
        
        public int SetCurrentDamage(int amount)
        {
            currentDamage = amount;
            return currentDamage;
        }
        
        public void IncrementDamage(int amount)
        {
            currentDamage += amount;
        }
        
        
    }