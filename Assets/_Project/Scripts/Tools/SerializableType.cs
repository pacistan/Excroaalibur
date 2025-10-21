using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Type sérialisable générique (filtré sur TBase).
/// Le type est choisi dans l’inspector parmi les classes concrètes assignables à TBase.
/// </summary>
[Serializable]
public class SerializableType<TBase> : IEquatable<SerializableType<TBase>>
{
    [SerializeField, HideInInspector]
    private string assemblyQualifiedName;

    [LabelText("Type sélectionné")]
    [ValueDropdown(nameof(GetDropdown))]
    [OnValueChanged(nameof(OnTypeChanged))]
    [InlineButton(nameof(Clear), "Clear")]
    [SerializeField]
    private string fullName;

    public Type Type => string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName);

    public SerializableType() { }

    public SerializableType(Type t)
    {
        if (t != null && typeof(TBase).IsAssignableFrom(t) && !t.IsAbstract)
        {
            assemblyQualifiedName = t.AssemblyQualifiedName;
            fullName = t.FullName;
        }
        else
        {
            Debug.LogWarning($"Type {t?.Name} invalide : doit hériter de {typeof(TBase).Name} et être concret.");
        }
    }

    public override string ToString() => Type != null ? Type.FullName : "(null)";

    private void OnTypeChanged()
    {
        if (string.IsNullOrEmpty(fullName))
        {
            assemblyQualifiedName = null;
            return;
        }

        var resolved = ResolveByFullName(fullName);
        if (resolved != null && typeof(TBase).IsAssignableFrom(resolved))
            assemblyQualifiedName = resolved.AssemblyQualifiedName;
        else
            assemblyQualifiedName = null;
    }

    public void Clear()
    {
        assemblyQualifiedName = null;
        fullName = null;
    }
    
    // === ÉGALITÉ / HASH basés sur AQN ===
    public bool Equals(SerializableType<TBase> other)
        => other != null && string.Equals(assemblyQualifiedName, other.assemblyQualifiedName, StringComparison.Ordinal);

    public override bool Equals(object obj) => Equals(obj as SerializableType<TBase>);

    public override int GetHashCode() => assemblyQualifiedName == null ? 0 : assemblyQualifiedName.GetHashCode();

#if UNITY_EDITOR
    private static IEnumerable<ValueDropdownItem<string>> GetDropdown()
    {
        var types = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => SafeGetTypes(a))
            .Where(t => typeof(TBase).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .OrderBy(t => t.FullName);

        var list = new List<ValueDropdownItem<string>> { new("<None>", null) };
        list.AddRange(types.Select(t => new ValueDropdownItem<string>(t.FullName, t.FullName)));
        return list;
    }

    private static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly asm)
    {
        try { return asm.GetTypes(); }
        catch { return Array.Empty<Type>(); }
    }
#endif

    private static Type ResolveByFullName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(fullName, throwOnError: false);
                if (t != null) return t;
            }
            catch { }
        }
        return null;
    }
}
