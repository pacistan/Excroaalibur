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
    public GEquipment _equipment {get; private set; }

    public void Initialize()
    {
        _neighbors = new GCell[Enum.GetValues(typeof(EHexDirection)).Length];
    }

    public void Start()
    {
        if (_ownedPawn)
            _ownedPawn.SetCell(this);
    }

    public void SetNeighbor (EHexDirection direction, GCell cell)
    {
        _neighbors[(int)direction] = cell;
        cell._neighbors[(int)direction.Opposite()] = this;
    }

    public bool IsWalkable(bool ignorePawn = false)
    {
        return _data.tileType == GCellData.ETileType.Normal && (_ownedPawn == null || ignorePawn);
    }

    public void SetPawn(GPawn pawn)
    {
        _ownedPawn = pawn;
    }

    public GPawn GetPawn()
    {
        return _ownedPawn;
    }
    
    public void SetEquipment(GEquipment equipment)
    {
        _equipment = equipment;
    }
    
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && _cellVisualsController != null)
        {
            _cellVisualsController.UpdateCellVisuals();
        }
    }
#endif
}
