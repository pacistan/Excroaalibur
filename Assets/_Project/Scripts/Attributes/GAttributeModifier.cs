using Sirenix.OdinInspector;
using System;
using UnityEngine;

/** Represents a modifier that can be applied to an attribute */
[Serializable]
public class GAttributeModifier
{
    [field : SerializeField] 
    public EAttributeType Type { get; private set; }
    [field : SerializeField] 
    public EModifierType ModifierType { get; private set; }
    [field : SerializeField] 
    public float Value { get; private set; }

    public object Source;
    
    public GAttributeModifier(EAttributeType type, EModifierType modifierType, float value, object source)
    {
        this.Type = type;
        this.ModifierType = modifierType;
        this.Value = value;
        this.Source = source;
    }
    
    public GAttributeModifier(EAttributeType type, EModifierType modifierType, float value, bool isTemporary, object source)
    {
        this.Type = type;
        this.ModifierType = modifierType;
        this.Value = value;
        this.Source = source;
    }

    public GAttributeModifier(GAttributeModifier other)
    {
        this.Type = other.Type;
        this.ModifierType = other.ModifierType;
        this.Value = other.Value;
        this.Source = other.Source;
    }
}

