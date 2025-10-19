using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[SelectionBase]
public class GCell : SerializedMonoBehaviour
{
    // Data of the Cell
    [SerializeField]
    public GCellData _data;

    [field: SerializeField, FoldoutGroup("PersistantData/Components"), ReadOnly]
    public RectTransform _ui;
    

    [SerializeField, FoldoutGroup("PersistantData/Components")]
    public GCellVisualsController _cellVisualsController;
    
    [SerializeField, FoldoutGroup("PersistantData/Components")]
    public Transform _pawnSpawnPoint;
    
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GHexCoordinate _hexCoordinates;

    [field : SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GCell[] _neighbors {get; private set;}

    [field: SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GPawn _ownedPawn;

    public ETileType GetTileType => _data.tileType;
    
    [field: SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GEquipment _equipment;


    
    public void ReleaseEquipement()
    {
        _equipment.OnReleased();
        _equipment = null;
    }
    
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

    public GCell GetNeighbor(EHexDirection direction)
    {
        return _neighbors[(int)direction];
    }

    public bool IsWalkable(bool ignorePawn = false)
    {
        return _data.tileType == ETileType.Normal && (_ownedPawn == null || ignorePawn);
    }

    public bool IsWalkable(ref ETileType[] walkableTypes, bool ignorePawn = false)
    {
        foreach (var tileType in walkableTypes)
            if (_data.tileType == tileType)
                return false;
        return _ownedPawn == null || ignorePawn;
    }

    public void SetPawn(GPawn pawn)
    {
        _ownedPawn = pawn;
    }

    public GPawn GetPawn()
    {
        return _ownedPawn;
    }
    
    public void Posess(GEquipment equipment)
    {
        _equipment = equipment;
        _equipment.transform.parent = _pawnSpawnPoint;
        _equipment.transform.localPosition = Vector3.zero;
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
