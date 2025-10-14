using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GGridObjectRegistry : GSingleton<GGridObjectRegistry>
{
    Dictionary<Type, List<GGridObject>> _registry = new Dictionary<Type, List<GGridObject>>();

    public void Register(GGridObject gridObject)
    {
        Type gridObjectType = gridObject.GetType();
        if (!_registry.ContainsKey(gridObjectType))
        {
            _registry.Add(gridObjectType, new List<GGridObject>());
        }
        
        if (_registry[gridObjectType].Contains(gridObject))
        {
            Debug.LogWarning($"Object of type {gridObjectType} already registered", gridObject);
        }
        else
        {
            _registry[gridObjectType].Add(gridObject);
        }
    }

    public void Unregister(GGridObject gridObject)
    {
        Type gridObjectType = gridObject.GetType();
        if (!_registry.ContainsKey(gridObjectType))
        {
            Debug.LogWarning($"The type {gridObjectType} is not Registered", gridObject);
        }
        
        if (!_registry[gridObjectType].Contains(gridObject))
        {
            Debug.LogWarning($"Object of type {gridObjectType} is not Registered", gridObject);
        }
        else
        {
            _registry[gridObjectType].Add(gridObject);
        }
    }

    public IEnumerable<T> GetItemsByPredicate<T>(Func<T, bool> predicate) where T : GGridObject
    {
        Type type = typeof(T);
        if (!_registry.ContainsKey(type))
        {
            return Enumerable.Empty<T>(); // Return empty if type not registered
        }
        return _registry[type].Cast<T>().Where(predicate);
    }

    public List<T> GetItems<T>() where T : GGridObject
    {
        Type type = typeof(T);
        if (!_registry.ContainsKey(type))
        {
            return new List<T>(); // Return empty if type not registered
        }
        return _registry[type].Cast<T>().ToList();
    }
}
