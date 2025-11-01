using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections;
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

    [SerializeField, FoldoutGroup("PersistantData/Components")]
    private Transform _pawnTransformPoint;

    [SerializeField, FoldoutGroup("PersistantData/Components")]
    private Transform _pawnSpawnPoint;

    [SerializeField, FoldoutGroup("PersistantData/Components")]
    Transform _equipmentTransformPoint;
    
    [SerializeField, ReadOnly, FoldoutGroup("PersistantData")][FormerlySerializedAs("_hexCoordinates")]
    public GHexCoordinate hexCoordinates;
    
    [field : SerializeField, ReadOnly, FoldoutGroup("PersistantData")][field: FormerlySerializedAs("<_neighbors>k__BackingField")]
    public GCell[] neighbors {get; private set;}
    
    [SerializeField, FoldoutGroup("Events")]
    private UnityEvent _OnPreviewSpawnedPawn;
    
    [SerializeField, FoldoutGroup("Events")]
    private UnityEvent _OnSpawnedPawnFinished;

    [field :SerializeField, ShowIf("GetTileType", ETileType.Spawner)]
    public GCell spawnLinkCell {get; private set;}
    
    public ETileType GetTileType => data.tileType;

    public Transform GetTransformPoint(GGridObject gridObject)
    {
        if(gridObject is GEquipment) return _equipmentTransformPoint;
        else if(data.tileType == ETileType.Spawner) return _pawnSpawnPoint;
        else return _pawnTransformPoint;
    }
    
    public void SetGridObject(GGridObject inGridObject, bool updateTransform = true)
    {
        if (inGridObject && !inGridObject.TrySetCell(this)) return;
        
        gridObject = inGridObject;
        if (!updateTransform || !inGridObject) return;
        
        Transform parent = GetTransformPoint(gridObject);
        gridObject.transform.parent = parent;
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
    
    public IEnumerator SpawnPawnFinish(GPawn pawn, Action OnSpawnFinishCallback)
    {
        RegisterGridObject(pawn);
        UpdateGridObject();
        
        Vector3 targetPos = spawnLinkCell.transform.position;
        //targetPos = /*2 * transform.position - */targetPos;
        targetPos.y = pawn.transform.position.y;
        pawn.transform.LookAt(targetPos);
        //pawn.transform.position = spawnLinkCell.transform.position;
        pawn.visuals.SetAnimationState(GPawn.SpawnAnimationName, 0f);
        
        RuntimeManager.PlayOneShotAttached("event:/Pawn/Enemy/Spawn", pawn.gameObject);
        // TODO : Call When the Spawn Process is finished (Animation, VFX, etc.) !!
        yield return new WaitForSeconds(1);/*() =>
            !pawn.visuals.GetAnimator().GetCurrentAnimatorStateInfo(0).IsName(GPawn.SpawnAnimationName));*/
        _OnSpawnedPawnFinished?.Invoke();
        OnSpawnFinishCallback?.Invoke();
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
        if (data.tileType == ETileType.Spawner && !spawnLinkCell)
        {
            spawnLinkCell = neighbors.First(a => a && a.data.tileType == ETileType.Hole);
            if (spawnLinkCell == null)
            {
                Debug.LogError("Spawner with no adjacent Hole Cell", gameObject);
            }
        }
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
