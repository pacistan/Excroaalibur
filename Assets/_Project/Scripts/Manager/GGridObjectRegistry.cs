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

    /// <summary>
    /// Doesn't work needs work
    /// </summary>
    /// <param name="predicate">Condition Lambda to filter the result</param>
    /// <typeparam name="T">The Type of GridObject Quieried</typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetItemsByPredicate<T>(Func<T, bool> predicate) where T : GGridObject
    {
        Type type = typeof(T);
        if (!Instance._registry.ContainsKey(type))
        {
            return Enumerable.Empty<T>(); // Return empty if type not registered
        }
        return Instance._registry[type].Cast<T>().Where(predicate);
    }

    public static List<T> GetItems<T>() where T : GGridObject
    {
        Type type = typeof(T);
        if (!Instance._registry.ContainsKey(type))
        {
            return new List<T>(); // Return empty if type not registered
        }
        return Instance._registry[type].Cast<T>().ToList();
    }
    
    public static T GetClosestObjectOfType<T>(GCell startCell, out int distance, bool regenerateStepMap = false, bool forceSearch = true) where T : GGridObject
    {
        distance = -1;
        List<T> gridObjects = GGridObjectRegistry.GetItems<T>();
        if (gridObjects == null || gridObjects.Count() == 0) return null;

        if (regenerateStepMap)
        {
            GGridManager.Instance.GenerateStepMap(startCell);
        }
        
        int shortestDistance = int.MaxValue;
        T targetGridObject = null;
        foreach (var gridObject in gridObjects)
        {
            int targetStep = GGridManager.Instance.GetStep(gridObject.GetCell(), true);
            if (targetStep != -1 && targetStep < shortestDistance) 
            {
                shortestDistance = targetStep;
                targetGridObject = gridObject;
            }
        }
        distance = shortestDistance;
        return targetGridObject;
    }
    
    public static T GetClosestObjectOfTypeWithPredicate<T>(GCell startCell, out int distance,Func<T, bool> predicate, bool regenerateStepMap = false, bool forceSearch = true) where T : GGridObject
    {
        distance = -1;
        List<T> gridObjects = GGridObjectRegistry.GetItemsByPredicate<T>(predicate).ToList();

        if (gridObjects == null || gridObjects.Count() == 0) return null;
        
        if (regenerateStepMap)
        {
            GGridManager.Instance.GenerateStepMap(startCell);
        }
        
        int shortestDistance = int.MaxValue;
        T targetGridObject = null;
        foreach (var gridObject in gridObjects)
        {
            int targetStep = GGridManager.Instance.GetStep(gridObject.GetCell(), true);
            if (targetStep != -1 && targetStep < shortestDistance) 
            {
                shortestDistance = targetStep;
                targetGridObject = gridObject;
            }
        }
        distance = shortestDistance == int.MaxValue ? -1 : shortestDistance;
        return targetGridObject;
    }
    
    
}
