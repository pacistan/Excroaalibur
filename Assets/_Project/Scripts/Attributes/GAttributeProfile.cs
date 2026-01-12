using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GAttributeDef
{
    public EAttributeType Type;

    [Min(0f)]
    public float BaseValue = 0f;
}

// ScriptableObject to hold a profile of attributes 
[CreateAssetMenu(fileName = "New Attribute Profile", menuName = "LeJeu/Attribute")]
public class GAttributeProfile : ScriptableObject
{
    public List<GAttributeDef> Attributes = new();

    [Button]
    private void AddAllAttributes()
    {
        Array attributeTypes = Enum.GetValues(typeof(EAttributeType));
        foreach (EAttributeType type in attributeTypes)
        {
            if (!Attributes.Exists(attr => attr.Type == type))
            {
                GAttributeDef newAttr = new GAttributeDef { Type = type, BaseValue = 0f };
                Attributes.Add(newAttr);
            }
        }
    }

    [Button]
    private void AddPlayerAttributes()
    {
        Array attributeTypes = Enum.GetValues(typeof(EAttributeType));
        foreach (EAttributeType type in attributeTypes)
        {
            if (type == EAttributeType.MaxHealth ) continue;
            
            if (!Attributes.Exists(attr => attr.Type == type))
            {
                GAttributeDef newAttr = new GAttributeDef { Type = type, BaseValue = 0f };
                Attributes.Add(newAttr);
            }
        }
        
        Attributes.RemoveAll(attr => attr.Type == EAttributeType.MaxHealth); // safe removal
    }
    
    // Editor Validation to Warn about duplicate attribute types
    private void OnValidate()
    {
        if (Attributes == null || Attributes.Count <= 0) return;
        
        HashSet<EAttributeType> types = new HashSet<EAttributeType>();
        foreach (GAttributeDef attr in Attributes)
        {
            if (types.Contains(attr.Type)) 
            {
                Debug.LogWarning($"Duplicate attribute type '{attr.Type}' in profile '{this.name}'");
            }
            types.Add(attr.Type);
        }
    }
}

