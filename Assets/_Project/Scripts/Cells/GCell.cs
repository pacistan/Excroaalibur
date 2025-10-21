using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[SelectionBase]
public class GCell : SerializedMonoBehaviour
{
    [SerializeField][FormerlySerializedAs("_data")]
    public GCellData data;
    
    [field: SerializeField, FoldoutGroup("PersistantData/Components"), ReadOnly][FormerlySerializedAs("_ui")]
    public RectTransform ui;
    
    [SerializeField, FoldoutGroup("PersistantData/Components")][FormerlySerializedAs("_cellVisualsController")]
    public GCellVisualsController cellVisualsController;
    
    [SerializeField, FoldoutGroup("PersistantData/Components")][FormerlySerializedAs("_pawnSpawnPoint")]
    public Transform pawnSpawnPoint;
    
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")][FormerlySerializedAs("_hexCoordinates")]
    public GHexCoordinate hexCoordinates;
    
    [field : SerializeField, ReadOnly, FoldoutGroup("PersistantData")][field: FormerlySerializedAs("<_neighbors>k__BackingField")]
    public GCell[] neighbors {get; private set;}
    
    public ETileType GetTileType => data.tileType;

    public GGridObject gridObject
    {
        get {
            return _gridObject;
        }
        
        set {
            if (value != null)
                value.transform.parent = pawnSpawnPoint;
            _gridObject = value;
        }
    }
    
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")]
    private GGridObject _gridObject;

    public void UpdateGridObject()
    {
        gridObject.transform.localPosition = Vector3.zero;
    }
    
    /** Remove the grid object from the cell */
    public void RemoveGridObject()
    {
        gridObject = null;
    }
    
    /** Generic method to get the grid object as a specific type */
    public T GetGridObject<T>() where T : GGridObject
    {
        return gridObject as T;
    }
    
    public void Initialize()
    {
        neighbors = new GCell[Enum.GetValues(typeof(EHexDirection)).Length];
    }
    
    public void SetNeighbor(EHexDirection direction, GCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public GCell GetNeighbor(EHexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public bool IsWalkable(bool ignorePawn = false)
    {
        return data.tileType == ETileType.Normal && (GetGridObject<GPawn>() == null || ignorePawn);
    }

    public bool IsWalkable(ref ETileType[] walkableTypes, bool ignorePawn = false)
    {
        return walkableTypes.Contains(data.tileType) && (GetGridObject<GPawn>() == null || ignorePawn);
    }
    
    public void Start()
    {
        if (gridObject)
            gridObject.SetCell(this);
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && cellVisualsController != null)
        {
            cellVisualsController.UpdateCellVisuals();
        }
    }
#endif
}
