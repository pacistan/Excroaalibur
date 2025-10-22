using Sirenix.OdinInspector;
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

    public void ChangeCellHighlightColor(Color color)
    {
        _highlight.color = color;
    }

    public void ResetCellHighlightColor()
    {
        var tileTypeData = cellCommonData.tileTypeData[_cell.data.tileType];
        _highlight.color = tileTypeData.highlightColor;
    }

    public void UpdateScaling(float scale)
    {
        _visualsParent.localScale = new Vector3(scale, scale, scale);
        _collidersParent.localScale = new Vector3(scale, scale, scale);
        _highlight.rectTransform.localScale = new Vector3(scale, scale, scale);
    }
}