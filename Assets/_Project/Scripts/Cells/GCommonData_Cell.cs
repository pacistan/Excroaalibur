using Sirenix.OdinInspector;
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

    [SerializeField]
    public Dictionary<ETileType, FTileTypeData> tileTypeData;

    [SerializeField]
    public Dictionary<EPawnSpawnType, GPawn> pawnTypeData;
    
    [SerializeField]
    public Dictionary<EEquipmentType, GEquipment> equipmentTypeData;
}
