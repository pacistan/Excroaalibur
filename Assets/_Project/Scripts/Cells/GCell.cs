using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class GCell : SerializedMonoBehaviour
{
    // Data of the Cell
    [SerializeField]
    public GCellData _data;

    [field: SerializeField, FoldoutGroup("PersistantData/Components"), ReadOnly]
    public RectTransform _UI;

    [SerializeField, FoldoutGroup("PersistantData/Components")]
    public GCellVisualsController _cellVisualsController;
    
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GHexCoordinate _hexCoordinates;

    [field : SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GCell[] _neighbors{get; private set;}

    [SerializeField, ReadOnly]
    public GPion pion;

    public void Initialize()
    {
        _neighbors = new GCell[Enum.GetValues(typeof(HexDirection)).Length];
    }
    
    public void SetNeighbor (HexDirection direction, GCell cell)
    {
        _neighbors[(int)direction] = cell;
        cell._neighbors[(int)direction.Opposite()] = this;
    }

    public bool IsWalkable()
    {
        return _data.tileType == GCellData.ETileType.Normal;
    }
    
    void OnValidate()
    {
        if (Application.isEditor && !Application.isPlaying && _cellVisualsController != null)
        {
            _cellVisualsController.UpdateCellVisuals();
        }
    }
}
