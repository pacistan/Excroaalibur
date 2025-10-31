using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(GCell))]
public class GCellVisualsController : MonoBehaviour
{
    [SerializeField]
    public GCellCommonData cellCommonData;
    
    [SerializeField]
    GCommonInstantiationData instantiationCommonData;
    
    [SerializeField, FoldoutGroup("Components")]
    MeshRenderer _meshRenderer;
    [SerializeField, FoldoutGroup("Components")]
    MeshFilter _meshFilter;
    [SerializeField, FoldoutGroup("Components")]
    public GCell _cell;
    [SerializeField, FoldoutGroup("Components")]
    TextMeshProUGUI _text;
    [SerializeField, FoldoutGroup("Components")]
    Image _highlight;
    [SerializeField, FoldoutGroup("Components")]
    Transform _visualsParent;
    [SerializeField, FoldoutGroup("Components")]
    Transform _collidersParent;

    [SerializeField, ReadOnly, FoldoutGroup("Components")]
    List<GameObject> _visualPresetInstances;

    [SerializeField, HideInInspector]
    EGridObjectType _previousObjectSpawnType;

    [SerializeField, HideInInspector, ReadOnly]
    public bool isHovered;

    [SerializeField, HideInInspector, ReadOnly]
    public bool isSelected;

    // TODO : Change later 
    private bool _wasPrevisualized;
    
    [SerializeField, HideInInspector, ReadOnly]
    public bool isPrevisualized = false;
    
    [FormerlySerializedAs("_currentactionHighlightActionType")]
    [SerializeField, HideInInspector, ReadOnly]
    public ETileHighlightActionType _currentHighlightActionType;

    [SerializeField, HideInInspector, ReadOnly]
    public ETileHighlightType _currentHighlightType = ETileHighlightType.CellSelect;

    [SerializeField, HideInInspector]
    public float _previousPositionY;

    [SerializeField, ReadOnly, Tooltip("Work In Progress")]
    bool _isPullingNeighbors;

    [SerializeField, ReadOnly]
    int _cadrillageNumber;
    
#if UNITY_EDITOR
    public void UpdateCellVisuals()
    {
        if (_cell.ui == null) return;
        _text = _cell.ui.GetComponentInChildren<TextMeshProUGUI>();
        _highlight = _cell.ui.GetComponentInChildren<Image>();
        var tileTypeData = cellCommonData.tileTypeData[_cell.data.tileType];

        // Tile Type
        {
            Material[] materials = tileTypeData.materials;
            Mesh mesh = tileTypeData.mesh;

            _meshRenderer.sharedMaterials = materials;
            _meshFilter.sharedMesh = mesh;
            
            Vector3 position = _meshFilter.transform.parent.position;
            position.y = tileTypeData.heightOffset;
            _meshFilter.transform.parent.position = position;
        }
        
        // Object Type
        EGridObjectType newObjectType = _cell.data.objectType;
        if(_previousObjectSpawnType != newObjectType)
        {
            Debug.Log("D");
            if (_cell.GetGridObject<GPawn>())
            {
                DestroyImmediate(_cell.gridObject.gameObject);
            }
            GGridObject objectPrefab = instantiationCommonData.objectTypeData[newObjectType];
            if (objectPrefab)
            {
                GGridObject gridObject = PrefabUtility.InstantiatePrefab(objectPrefab) as GGridObject;
                Transform parent = gridObject is GEquipment ? _cell.equipmentSpawnPoint : _cell.pawnSpawnPoint;
                gridObject.transform.parent = parent;
                _cell.SetGridObject(gridObject, true);
                EditorUtility.SetDirty(gridObject);
            }
            _previousObjectSpawnType = newObjectType;
        }
        
        // UI
        {
            _text.color = tileTypeData.textColor;
            _highlight.color = tileTypeData.highlightColor;
            
            var rectTransform = _highlight.rectTransform.parent.GetComponent<RectTransform>();
            Vector3 position = rectTransform.position;
            position.y = tileTypeData.heightOffset;
            rectTransform.position = position;
        }
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(_cell);
        Image image;
    }

    public GameObject OnCreateVisualPreset(GCellVisualPresetData brush, bool serialize = true)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(brush.prefab);
        _visualPresetInstances.Add(instance);
        if (serialize)
        {
            _cell.data.AddPaintedVisual(brush);
        }
        
        if (instance)
        {
            instance.transform.parent = _visualsParent;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(0, brush.rotation, 0);
        }
        EditorUtility.SetDirty(this);
        return instance;
    }

    public void ClearVisualsPresets()
    {
        _visualPresetInstances.ForEach(instance => DestroyImmediate(instance));
        _cell.data.paintedVisuals.Clear();
        _visualPresetInstances.Clear();
    }
#endif
    
    public void UpdateCellDebugNum(string newDebugText)
    {
        _text.text = newDebugText;
    }

    public void ChangeSprite(ETileHighlightType highlightType)
    {
        var highlightSprite = cellCommonData.tileHighlightData[highlightType];
        if (highlightSprite && _currentHighlightActionType != ETileHighlightActionType.Normal)
        {
            _highlight.enabled = true; 
            _highlight.sprite = cellCommonData.tileHighlightData[highlightType];
        }
        else
        {
            _highlight.enabled = false; 
        }
    }
    
    public void UpdateScaling(float scale)
    {
        _visualsParent.localScale = new Vector3(scale, scale, scale);
        _collidersParent.localScale = new Vector3(scale, scale, scale);
        _highlight.rectTransform.localScale = new Vector3(scale, scale, scale);
    }

    void LateUpdate()
    {
        UpdateHighlightSprite();
        if (_wasPrevisualized != isPrevisualized)
        {
            _wasPrevisualized = isPrevisualized;
            if (isPrevisualized)
            {
                _highlight.color = cellCommonData.previsualizedColor;
            }
            else
            {
                SetHighlightActionType(_currentHighlightActionType);
            }
        }
    }

    void Start()
    {
        SetHighlightActionType(ETileHighlightActionType.Normal);
        int x = _cell.data.gridCoordinates.x;
        int y = _cell.data.gridCoordinates.y;
        
        _cadrillageNumber = 1 + (((x % 3) + 1 + (y % 2) * 2) % 3);
        
        _cell.ui.GetComponentInChildren<TextMeshProUGUI>().text = _cadrillageNumber.ToString();
        _cell.ui.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        
        Color color = _meshRenderer.material.color;
        Color.RGBToHSV(color, out float h, out float s, out float v);
        float tint = _cadrillageNumber == 1 ? cellCommonData.cadrillageTint1 : _cadrillageNumber == 2 ? cellCommonData.cadrillageTint2 : cellCommonData.cadrillageTint3;
        v += tint;
        color = Color.HSVToRGB(h, s, v);
        _meshRenderer.material.color = color;
    }

    private void UpdateHighlightSprite()
    {
        ETileHighlightType newHighlightType = ETileHighlightType.CellBase;

        newHighlightType = isSelected ? ETileHighlightType.CellSelect :
            isHovered ? ETileHighlightType.CellHover : ETileHighlightType.CellBase;
        
        if (newHighlightType != _currentHighlightType)
        {
            ChangeSprite(newHighlightType);
        }
        
        _currentHighlightType = newHighlightType;
    }

    public void SetHighlightActionType(ETileHighlightActionType highlightActionType)
    {
        _currentHighlightActionType = highlightActionType;

        Color color = Color.white;

        if (highlightActionType == ETileHighlightActionType.Normal)
        {
            color = cellCommonData.tileTypeData[_cell.data.tileType].highlightColor;
        }
        else
        {
            color = cellCommonData.tileHighlightActionData[highlightActionType];
        }

        _highlight.color = color;
        ChangeSprite(_currentHighlightType);
    }
    
    void OnValidate()
    {
        if (transform.position.y != _previousPositionY)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, -cellCommonData.maxHeight, cellCommonData.maxHeight);
            transform.position = pos;
        
            float diff = _previousPositionY - transform.position.y;
            Vector3 uiPos = _cell.ui.position;
            uiPos.y -= diff;
            _cell.ui.position = uiPos;
        
            _previousPositionY = transform.position.y;
        }       
    }


    
    /*
    void OnDrawGizmos()
    {
        if (transform.position.y != _previousPositionY)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Min(pos.y, cellCommonData.maxHeight);
            pos.y = Mathf.Max(pos.y, -cellCommonData.maxHeight);
            transform.position = pos;
            
            float diff = _previousPositionY - transform.position.y;
            Vector3 uiPos = _cell.ui.position;
            uiPos.y -= diff;
            _cell.ui.position = uiPos;
            
            _previousPositionY = transform.position.y;
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            foreach (Transform child in transform)
            {
                
                EditorUtility.SetDirty(child.gameObject);
            }
            EditorUtility.SetDirty(_cell.ui.gameObject);       
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            
#endif
            return;
            
            if (!_isPullingNeighbors)
            {
                GGridManager gridManager = GGridManager.Instance ?? FindFirstObjectByType<GGridManager>(); 
                gridManager.grid.ForEach(a => a.visuals._previousPositionY = a.transform.position.y);
                _previousPositionY = transform.position.y;
                return;
            }
            int i = 0;
            foreach (var neighbor in _cell.neighbors)
            {
                if(!neighbor || neighbor.data.tileType != ETileType.Normal) continue;
                if (Mathf.Abs(transform.position.y - neighbor.transform.position.y) > cellCommonData.maxOffsetHeight)
                {
                    neighbor.visuals.UpdateYPosGizmo(transform.position.y);
                }
            }
        }
    }

    bool isCheckedThisFrame;
    */
    
    public void UpdateYPosGizmo(float neighborHeight)
    {
        float diff = neighborHeight - transform.position.y;
        float newHeight = transform.position.y + Mathf.Min(Mathf.Abs(diff), cellCommonData.maxOffsetHeight) * diff / Mathf.Abs(diff);
        Vector3 pos = transform.position;
        pos.y = newHeight;
        transform.position = pos;
        
        float diffe = _previousPositionY - transform.position.y;
        Vector3 uiPos = _cell.ui.position;
        uiPos.y -= diffe;
        _cell.ui.position = uiPos;

        
        _previousPositionY = transform.position.y;
        
        foreach (var neighbor in _cell.neighbors)
        {
            
            if (!neighbor || neighbor.data.tileType != ETileType.Normal) continue;
            if (Mathf.Abs(transform.position.y - neighbor.transform.position.y) > cellCommonData.maxOffsetHeight)
            {
                neighbor.visuals.UpdateYPosGizmo(transform.position.y);
            }
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
#endif
    }
}

[CustomEditor(typeof(GCellVisualsController))]
public class YourClassNameEditor : Editor
{
    
    private SerializedProperty _previousPositionYProp;
    private GCellVisualsController _target;

    private void OnEnable()
    {
        _target = (GCellVisualsController)target;
        _previousPositionYProp = serializedObject.FindProperty("_previousPositionY");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();

        if (_target.transform.position.y != _previousPositionYProp.floatValue)
        {
            ApplyPositionClamping();
        }
    }

    private void ApplyPositionClamping()
    {
        Vector3 pos = _target.transform.position;
        pos.y = Mathf.Clamp(pos.y, -_target.cellCommonData.maxHeight, _target.cellCommonData.maxHeight);
        _target.transform.position = pos;

        float diff = _previousPositionYProp.floatValue - _target.transform.position.y;
        if (_target._cell != null && _target._cell.ui != null)
        {
            Vector3 uiPos = _target._cell.ui.position;
            uiPos.y -= diff;
            _target._cell.ui.position = uiPos;
        }

        _previousPositionYProp.floatValue = _target.transform.position.y;
        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(_target);
    }
}
