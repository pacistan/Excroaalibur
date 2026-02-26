using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public enum ERarity {Common, Rare, Legendary}

[CreateAssetMenu(fileName = "Upgrade", menuName = "Upgrades/Upgrade")]
public class GSOUpgrade : SerializedScriptableObject
{
    [field: SerializeField] 
    public LocalizedString Name { get; private set; }
    
    [field: SerializeField] 
    public LocalizedString Description { get; private set; }
    
    [field: SerializeField] 
    public Sprite Icon { get; private set; }
    
    [field: SerializeField] 
    public List<GAttributeModifier> Effects { get; private set; } = new List<GAttributeModifier>();
    
    [field: SerializeField] 
    public GModifierProvider Conditions { get; private set; }
    
    [field : SerializeField, ReadOnly] 
    public string ItemID { get; private set; }
    
    public GSOUpgrade CreateInstance()
    {
        GSOUpgrade upgrade = Instantiate(this);
        upgrade.ItemID = ItemID;
        foreach (GAttributeModifier effect in upgrade.Effects)
        {
            effect.Source = upgrade;
        }
        return upgrade;
    }
    
    GSOUpgrade()
    {
        UpdateUID();
    }

    [Button]
    public void UpdateUID()
    {
        ItemID = Guid.NewGuid().ToString();
    }

    public void WhenDestroyed()
    {
        if (Conditions != null)
        {
            Conditions.Dispose();
        }
    }
}
