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

