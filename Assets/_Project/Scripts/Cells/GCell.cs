using Sirenix.OdinInspector;
using System;
using System.Linq;
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

    [FormerlySerializedAs("_ownedPawn")]
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    public GPawn ownedPawn;
    
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
        if (ownedPawn)
            ownedPawn.SetCell(this);
    }

    public void SetNeighbor(EHexDirection direction, GCell cell)
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
        return _data.tileType == ETileType.Normal && (ownedPawn == null || ignorePawn);
    }

    public bool IsWalkable(ref ETileType[] walkableTypes, bool ignorePawn = false)
    {
        return walkableTypes.Contains(_data.tileType) && (ownedPawn == null || ignorePawn);
    }
    
    public void GiveEquipement(GEquipment equipment)
    {
        _equipment = equipment;
        _equipment.transform.parent = _pawnSpawnPoint;
        _equipment.transform.localPosition = Vector3.zero;
        _equipment.SetCell(this);
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
