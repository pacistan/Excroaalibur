using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Instantiation Data", menuName = "CommonData/Instantiation Data")]
public class GCommonInstantiationData : SerializedScriptableObject
{
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine)]
    public Dictionary<EGridObjectType, GGridObject> objectTypeData;
    
#if UNITY_EDITOR
    [OnInspectorInit]
    public void CreateData()
    {
        if (objectTypeData == null)
        {
            objectTypeData = new Dictionary<EGridObjectType, GGridObject>();
            foreach (var gridObjectSpawnType in Enum.GetValues(typeof(EGridObjectType)) as EGridObjectType[])
            {
                objectTypeData.Add(gridObjectSpawnType, null);
            }
        }
    }
#endif
}
