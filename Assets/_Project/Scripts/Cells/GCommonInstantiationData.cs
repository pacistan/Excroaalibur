using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Instantiation Data", menuName = "CommonData/Instantiation Data")]
public class GCommonInstantiationData : SerializedScriptableObject
{
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine)]
    public Dictionary<EPawnSpawnType, GPawn> pawnTypeData;
    
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine)]
    public Dictionary<EEquipmentType, GEquipment> equipmentTypeData;
    
#if UNITY_EDITOR
    [OnInspectorInit]
    public void CreateData()
    {
        if (pawnTypeData == null)
        {
            pawnTypeData = new Dictionary<EPawnSpawnType, GPawn>();
            foreach (var pawnSpawnType in Enum.GetValues(typeof(EPawnSpawnType)) as EPawnSpawnType[])
            {
                pawnTypeData.Add(pawnSpawnType, null);
            }
        }

        if (equipmentTypeData == null)
        {
            equipmentTypeData = new Dictionary<EEquipmentType, GEquipment>();
            foreach (var equipmentType in Enum.GetValues(typeof(EEquipmentType)) as EEquipmentType[])
            {
                equipmentTypeData.Add(equipmentType, null);
            }
        }
    }
#endif
}
