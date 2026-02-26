using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Serialization;

    public class GCrown : GEquipment
    {
        public static Action<GPawn, GPawn, int> OnPass;
        public static Action OnSwordReset;
        
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
            OnSwordReset?.Invoke();
        }
        
        public int SetCurrentDamage(int amount)
        {
            currentDamage = amount;
            if (amount > currentDamage)
            {
                OnPass?.Invoke(null, null, currentDamage);
            }
            return currentDamage;
        }
        
        public void IncrementDamage(GPawn thrower, GPawn receiver, int amount)
        {
            currentDamage += amount;
            OnPass?.Invoke(thrower, receiver, currentDamage);
        }
        
        
    }