using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(GCell))]
public class GCellVisualsController : SerializedMonoBehaviour
{
    [FormerlySerializedAs("_commonCellData")]
    [SerializeField]
    GCellCommonData cellCommonData;
    
    [SerializeField]
    GCommonInstantiationData instantiationCommonData;
    
    [SerializeField, FoldoutGroup("Components")]
    MeshRenderer _meshRenderer;
    [SerializeField, FoldoutGroup("Components")]
    MeshFilter _meshFilter;
    [SerializeField, FoldoutGroup("Components")]
    GCell _cell;
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
            if (_cell.GetGridObject<GPawn>())
            {
                DestroyImmediate(_cell.gridObject.gameObject);
            }
            GGridObject objectPrefab = instantiationCommonData.objectTypeData[newObjectType];
            if (objectPrefab)
            {
                _cell.gridObject = PrefabUtility.InstantiatePrefab(objectPrefab, _cell.pawnSpawnPoint) as GGridObject;
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
        _highlight.sprite = cellCommonData.tileHighlightData[highlightType];
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

        if (highlightActionType == ETileHighlightActionType.Normal)
        {
            _highlight.color = cellCommonData.tileTypeData[_cell.data.tileType].highlightColor;
        }
        else
        {
            _highlight.color = cellCommonData.tileHighlightActionData[highlightActionType];
        }
    }
}