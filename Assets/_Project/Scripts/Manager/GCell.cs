using Sirenix.OdinInspector;
using System;
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
