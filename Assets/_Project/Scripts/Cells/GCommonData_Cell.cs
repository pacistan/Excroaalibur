using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cell Data", menuName = "CommonData/Cell Data")]
public class GCommonData_Cell : SerializedScriptableObject
{
    [System.Serializable]
    public struct GFTileTypeData
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

    [SerializeField]
    public Dictionary<GCellData.ETileType, GFTileTypeData> TileTypeData;
}
