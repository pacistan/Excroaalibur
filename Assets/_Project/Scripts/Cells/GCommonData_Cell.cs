using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cell Data", menuName = "CommonData/Cell Data")]
public class GCellCommonData : SerializedScriptableObject
{
    [System.Serializable]
    public struct FTileTypeData
    {
        [field: SerializeField, BoxGroup("Mesh")]
        public Mesh mesh { get; private set; }

        [field: SerializeField, BoxGroup("Mesh")]
        public Material[] materials { get; private set; }
        
        [field: SerializeField, BoxGroup("UI")] 
        public Color textColor { get; private set; }
        
        [field: SerializeField, BoxGroup("UI")] 
        public Color highlightColor { get; private set; }
    }

    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
    public Dictionary<ETileType, FTileTypeData> tileTypeData;

    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine)]
    public Dictionary<EPawnSpawnType, GPawn> pawnTypeData;
    
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine)]
    public Dictionary<EEquipmentType, GEquipment> equipmentTypeData;
    
    #if UNITY_EDITOR
    [OnInspectorInit]
    public void CreateData()
    {
        if (tileTypeData == null)
        {
            tileTypeData = new Dictionary<ETileType, FTileTypeData>();
            foreach (var tileType in Enum.GetValues(typeof(ETileType)) as ETileType[])
            {
                tileTypeData.Add(tileType, new FTileTypeData());
            }
        }

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
