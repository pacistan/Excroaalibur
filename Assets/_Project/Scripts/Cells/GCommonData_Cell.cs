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
    }
    #endif
}