using Sirenix.OdinInspector;
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
    GameObject _visualPreset;
    
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
        }
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(_cell);
    }

    public void OnCreateVisualPreset(GameObject preset, Quaternion rotation = new Quaternion(), bool useRandomRotation = false)
    {
        if (_visualPreset)
        {
            DestroyImmediate(_visualPreset);
        }
        _visualPreset = preset;

        if (preset)
        {
            preset.transform.parent = _visualsParent;
            preset.transform.localPosition = Vector3.zero;
            if (useRandomRotation)
            {
                preset.transform.localRotation = rotation;
            }
            else
            {
                preset.transform.Rotate(rotation * Vector3.up);
            }
        }
        
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