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
    public RectTransform _ui;

    [SerializeField, FoldoutGroup("PersistantData/Components")]
    public GCellVisualsController _cellVisualsController;
    
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GHexCoordinate _hexCoordinates;

    [field : SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GCell[] _neighbors {get; private set;}

    [field: SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GPawn _ownedPawn;
    
    [field: SerializeField, ReadOnly]
    public GGridObject _ownedGridObject {get; private set; }

    public void Initialize()
    {
        _neighbors = new GCell[Enum.GetValues(typeof(EHexDirection)).Length];
    }
    
    public void SetNeighbor (EHexDirection direction, GCell cell)
    {
        _neighbors[(int)direction] = cell;
        cell._neighbors[(int)direction.Opposite()] = this;
    }

    public bool IsWalkable()
    {
        return _data.tileType == GCellData.ETileType.Normal && _ownedPawn == null;
    }

    public void SetPawn(GPawn pawn)
    {
        _ownedPawn = pawn;
    }

    public GPawn GetPion()
    {
        return _ownedPawn;
    }
    
    public void SetGridObject(GGridObject Object)
    {
        _ownedGridObject = Object;
    }
    
    void OnValidate()
    {
        if (!Application.isPlaying && _cellVisualsController != null)
        {
            _cellVisualsController.UpdateCellVisuals();
        }
    }
}
