using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[SelectionBase]
public class GCell : SerializedMonoBehaviour
{
    
    [field : SerializeField, ReadOnly]
    public GGridObject gridObject { get; private set; }

    [SerializeField][FormerlySerializedAs("_data")]
    [InlineProperty, HideLabel, BoxGroup("Data")]
    public GCellData data;
    
    [field: SerializeField, FoldoutGroup("PersistantData/Components"), ReadOnly][FormerlySerializedAs("_ui")]
    public RectTransform ui;
    
    [FormerlySerializedAs("cellVisualsController")]
    [SerializeField, FoldoutGroup("PersistantData/Components")][FormerlySerializedAs("_cellVisualsController")]
    public GCellVisualsController visuals;
    
    [SerializeField, FoldoutGroup("PersistantData/Components")][FormerlySerializedAs("_pawnSpawnPoint")]
    public Transform pawnSpawnPoint;
    
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")][FormerlySerializedAs("_hexCoordinates")]
    public GHexCoordinate hexCoordinates;
    
    [field : SerializeField, ReadOnly, FoldoutGroup("PersistantData")][field: FormerlySerializedAs("<_neighbors>k__BackingField")]
    public GCell[] neighbors {get; private set;}
    
    [SerializeField, FoldoutGroup("Events")]
    private UnityEvent _OnPreviewSpawnedPawn;
    
    [SerializeField, FoldoutGroup("Events")]
    private UnityEvent _OnSpawnedPawnFinished;

    public ETileType GetTileType => data.tileType;
    
    public void SetGridObject(GGridObject inGridObject, bool updateTransform = true)
    {
        if (inGridObject && !inGridObject.TrySetCell(this)) return;
        
        gridObject = inGridObject;
        if (!updateTransform || !inGridObject) return;
        
        gridObject.transform.parent = pawnSpawnPoint;
        gridObject.transform.localPosition = Vector3.zero; 
        gridObject.transform.localRotation = Quaternion.identity;
    }
    
    /** Update the position of the grid object to be centered in the cell */
    public void UpdateGridObject()
    {
        gridObject.transform.localPosition = Vector3.zero;
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
    
    public void RegisterGridObject(GGridObject inGridObject)
    {
        gridObject = inGridObject;
        inGridObject.TrySetCell(this);
    }

    public void PreviewSpawnPawn()
    {
        _OnPreviewSpawnedPawn?.Invoke();
        // TODO : Other debug here !
    }
    
    public void SpawnPawnFinish(GPawn pawn)
    {
        RegisterGridObject(pawn);
        UpdateGridObject();
        
        RuntimeManager.PlayOneShotAttached("event:/Pawn/Enemy/Spawn", pawn.gameObject);
        // TODO : Call When the Spawn Process is finished (Animation, VFX, etc.) !!
        _OnSpawnedPawnFinished?.Invoke();
        GWaveManager.Instance.OnEnemySpawned();
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
            gridObject.TrySetCell(this);
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && visuals != null)
        {
            visuals.UpdateCellVisuals();
        }
    }
#endif
}
