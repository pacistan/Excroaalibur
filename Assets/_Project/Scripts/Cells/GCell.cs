using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GCell : SerializedMonoBehaviour
{
    [SerializeField]
    public GCellData _data { get; set; }

    [FormerlySerializedAs("_rectTransform")]
    [field: SerializeField, FoldoutGroup("PersistantData/Components"), ReadOnly]
    public RectTransform _UI;
    
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GHexCoordinate _hexCoordinates;

    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    GCell[] _neighbors = new GCell[6];

    public void Initialize()
    {
        _neighbors = new GCell[Enum.GetValues(typeof(HexDirection)).Length];
    }
    
    public void SetNeighbor (HexDirection direction, GCell cell)
    {
        _neighbors[(int)direction] = cell;
        cell._neighbors[(int)direction.Opposite()] = this;
    }
}

public class GCommonData : GSingleton<GCommonData>
{
    [field : SerializeField]
    public GCommonData_Cell _cellData { get; private set; }
}

[CreateAssetMenu(fileName = "Cell Data", menuName = "CommonData/Cell Data")]
public class GCommonData_Cell : SerializedScriptableObject
{
    [System.Serializable]
    public struct GFTileTypeData
    {
        [field: SerializeField]
        Mesh mesh;

        [field: SerializeField]
        Material[] materials;
    }

    [SerializeField]
    Dictionary<GCellData.ETileType, GFTileTypeData> TileTypeData;
}
