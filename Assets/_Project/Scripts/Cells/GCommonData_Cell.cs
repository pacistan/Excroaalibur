using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
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
        
        [field: SerializeField, BoxGroup("Mesh")]
        public float heightOffset { get; private set; }
        
        [field: SerializeField, BoxGroup("UI")] 
        public Color textColor { get; private set; }
        
        [field: SerializeField, BoxGroup("UI")] 
        public Color highlightColor { get; private set; }
    }

    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
    public Dictionary<ETileType, FTileTypeData> tileTypeData;

    public Dictionary<ETileHighlightType, Sprite> tileHighlightData;
    
    public Dictionary<ETileHighlightActionType, Color> tileHighlightActionData;
    
    public Color previsualizedColor = Color.green;
    
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

        if (tileHighlightData == null)
        {
            tileHighlightData = new Dictionary<ETileHighlightType, Sprite>();
            foreach (var tileType in Enum.GetValues(typeof(ETileHighlightType)) as ETileHighlightType[])
            {
                tileHighlightData.Add(tileType, null);
            }
        }
        
        
        if (tileHighlightActionData == null)
        {
            tileHighlightActionData = new Dictionary<ETileHighlightActionType, Color>();
            foreach (var tileActionType in Enum.GetValues(typeof(ETileHighlightActionType)) as ETileHighlightActionType[])
            {
                tileHighlightActionData.Add(tileActionType, Color.white);
            }
        }

        EditorUtility.SetDirty(this);
    }
    #endif
}