using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

/** All possible attribute types ! */
public enum EAttributeType
{
    MaxHealth,
    MaxAction,
    PassDMGUpgrade,
    PushStrength,
    MoveDistance,
    FollowDistance,
    PushDamage,
    PushStunAmount
    // Add New Attribute Types Here !
}

public enum EModifierType
{
    Additive = 1,       // ex : +10 = 10
    Multiplicative = 2, // ex : +10% = 0.1, Each Multiplicative modifier is summed before being applied
}

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
}

/** Controller responsible for managing a collection of attributes for a game entity. */
public class GAttributesController : SerializedMonoBehaviour
{
    /** Struct holding the state of a single attribute. */
    private struct SAttribute
    {
        public float Base;
        public float CachedFinal; // Always valid 
    }
    
    [SerializeField, HideInEditorMode]
    private readonly Dictionary<EAttributeType, SAttribute> _attributes = new();
    [SerializeField, HideInEditorMode]
    private readonly Dictionary<EAttributeType, List<GAttributeModifier>> _mods = new();

    /** Simple per-attribute event (new value only) */
    private readonly Dictionary<EAttributeType, Action<float, float>> _onAttributeChangedCallback = new();
    
    public void LoadProfile(GAttributeProfile profile)
    {
        if (profile == null) return;
        
        _attributes.Clear();
        _mods.Clear();
        
        // Attributes
        for (int i = 0; i < profile.Attributes.Count; i++)
        {
            var def = profile.Attributes[i];

            _attributes[def.Type] = new SAttribute
            {
                Base = def.BaseValue,
                CachedFinal = 0f
            };

            _mods[def.Type] = new List<GAttributeModifier>(4);

            // Compute initial Final (no event on load by default)
            RecomputeFinal_NoNotify(def.Type);
        }
    }
    
    /** Add a callback for when an attribute changes ( oldValue, newValue ) */
    public void SubscribeCallBack(EAttributeType type, Action<float, float> callback)
    {
        if (callback == null) return;

        if (_onAttributeChangedCallback.TryGetValue(type, out Action<float, float> existing))
            _onAttributeChangedCallback[type] = existing + callback;
        else
            _onAttributeChangedCallback[type] = callback;
    }

    /** Remove a callback for when an attribute changes ( oldValue, newValue ) */
    public void UnsubscribeCallBack(EAttributeType type, Action<float, float> callback)
    {
        if (callback == null) return;

        if (_onAttributeChangedCallback.TryGetValue(type, out Action<float, float> existing))
        {
            existing -= callback;
            if (existing == null) _onAttributeChangedCallback.Remove(type);
            else _onAttributeChangedCallback[type] = existing;
        }
    }

    /** Clear all callbacks for all attributes */
    public void ClearAllCallbacks()
    {
        _onAttributeChangedCallback.Clear();
    }
    
    /** Clear all callbacks for a specific attribute typ e*/
    public void ClearAllAttributesCallbacks(EAttributeType type)
    {
        if (_onAttributeChangedCallback.ContainsKey(type))
            _onAttributeChangedCallback.Remove(type);
    }
    
    public bool Has(EAttributeType type) => _attributes.ContainsKey(type);

    // TODO : Transform in TryGet pattern ! 
    
    /** returns -1f if attribute not found */
    public float GetBase(EAttributeType type) => _attributes.TryGetValue(type, out SAttribute attribute) ? attribute.Base : -1f;

    /** returns -1f if attribute not found */
    public float GetFinal(EAttributeType type) => _attributes.TryGetValue(type, out SAttribute attribute) ? attribute.CachedFinal : -1f;
    
    public void SetBase(EAttributeType type, float value)
    {
        if (!_attributes.TryGetValue(type, out SAttribute attribute))
        {
            Debug.LogError($"[Attributes] SetBase on unknown attribute '{type}'.");
            return;
        }

        attribute.Base = value;
        _attributes[type] = attribute;

        RecomputeFinal_NotifyIfChanged(type);
    }

    public void AddModifier(GAttributeModifier mod)
    {
        if (!_mods.TryGetValue(mod.Type, out List<GAttributeModifier> list))
        {
            Debug.LogError($"[Attributes] AddModifier on unknown attribute '{mod.Type}'.");
            return;
        }

        list.Add(mod);
        RecomputeFinal_NotifyIfChanged(mod.Type);
    }
    
    // Helper method to create and add a modifier in one call 
    public GAttributeModifier AddMod(EAttributeType type, EModifierType modType, float value, object source)
    {
        var m = new GAttributeModifier(type, modType, value, source);
        AddModifier(m);
        return m;
    }
    
    public void AddModifiers(List<GAttributeModifier> mods)
    {
        if (mods == null || mods.Count == 0) return;
        
        var dirty = new HashSet<EAttributeType>();
        
        for (int i = 0; i < mods.Count; i++)
        {
            GAttributeModifier mod = mods[i];

            if (!_mods.TryGetValue(mod.Type, out List<GAttributeModifier> list))
            {
                Debug.LogError($"[Attributes] AddModifier on unknown attribute '{mod.Type}'.");
                continue;
            }

            list.Add(mod);
            dirty.Add(mod.Type);
        }
        
        foreach (var type in dirty)
            RecomputeFinal_NotifyIfChanged(type);
    }

    public void RemoveModifier(GAttributeModifier mod)
    {
        if (!_mods.TryGetValue(mod.Type, out List<GAttributeModifier> list)) return;

        if (list.Remove(mod))
            RecomputeFinal_NotifyIfChanged(mod.Type);
    }
    
    /** Remove all modifiersfrom a given source */
    public void RemoveAllModifiersFromSource(object source)
    {
        if (source == null) return;

        List<EAttributeType> TypeList = new List<EAttributeType>();

        foreach (var mod in _mods)
        {
            List<GAttributeModifier> list = mod.Value;
            if (list == null || list.Count == 0) continue;

            int removed = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Source == source)
                {
                    list.RemoveAt(i);
                    removed++;
                }
            }

            if (removed > 0)
                TypeList.Add(mod.Key);
        }

        for (int i = 0; i < TypeList.Count; i++)
            RecomputeFinal_NotifyIfChanged(TypeList[i]);
    }

    /** Clear all modifiers of a given type */
    public void ClearModifiers(EAttributeType type)
    {
        if (!_mods.TryGetValue(type, out List<GAttributeModifier> list)) return;

        if (list.Count > 0)
        {
            list.Clear();
            RecomputeFinal_NotifyIfChanged(type);
        }
    }
    
    private void RecomputeFinal_NoNotify(EAttributeType type)
    {
        if (! _attributes.TryGetValue(type, out var st)) return;
        st.CachedFinal = ComputeFinal(type, st.Base);
        _attributes[type] = st;
    }

    private void RecomputeFinal_NotifyIfChanged(EAttributeType type)
    {
        if (!_attributes.TryGetValue(type, out SAttribute attribute)) return;

        float oldFinal = attribute.CachedFinal;
        float newFinal = ComputeFinal(type, attribute.Base);

        if (Mathf.Approximately(oldFinal, newFinal))
        {
            // Even if modifiers changed, final didn't; no need to notify
            attribute.CachedFinal = newFinal;
            _attributes[type] = attribute;
            return;
        }

        attribute.CachedFinal = newFinal;
        _attributes[type] = attribute;

        if ( _onAttributeChangedCallback.TryGetValue(type, out Action<float, float> callback))
            callback?.Invoke(oldFinal, newFinal);
    }

    private float ComputeFinal(EAttributeType type, float baseValue)
    {
        float v = ApplyModifiers(type, baseValue);
        
        // For now, clamp to 0 minimum and no maximum 
        if (v < 0f) v = 0f;

        // for now, round to nearest integer
        return Mathf.Round(v);
    }
    
    private float ApplyModifiers(EAttributeType type, float start)
    {
        if (!_mods.TryGetValue(type, out var list) || list.Count == 0)
            return start;
        
        float v = start;
        float sumPercent = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            var mod = list[i];
            if (mod.Value == 0f) continue;
            
            switch (mod.ModifierType)
            {
                case EModifierType.Additive: v += mod.Value; break;
                case EModifierType.Multiplicative: sumPercent += mod.Value; break;
            }
        }

        v *= (1f + sumPercent);
        return v;
    }
}
