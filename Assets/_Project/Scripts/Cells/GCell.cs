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

    void OnValidate()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            _cellVisualsController.UpdateCellVisuals();
        }
    }
}
