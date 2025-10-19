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
    EPawnSpawnType _previousPawnSpawnType;
    
    [SerializeField, HideInInspector]
    EEquipmentType _previousEquipmentType;
    
#if UNITY_EDITOR
    public void UpdateCellVisuals()
    {
        if (_cell._ui == null) return;
        _text = _cell._ui.GetComponentInChildren<TextMeshProUGUI>();
        _highlight = _cell._ui.GetComponentInChildren<Image>();
        var tileTypeData = cellCommonData.tileTypeData[_cell._data.tileType];

        // Tile Type
        {
            Material[] materials = tileTypeData.materials;
            Mesh mesh = tileTypeData.mesh;

            _meshRenderer.sharedMaterials = materials;
            _meshFilter.sharedMesh = mesh;
        }
        
        // Pawn Type
        EPawnSpawnType newPawnType = _cell._data.pawnType;
        if(_previousPawnSpawnType != newPawnType)
        {
            if (_cell._equipment)
            {
                _cell._data.pawnType = _previousPawnSpawnType;
                Debug.LogWarning("Can't Change Pawn when there is an equipment");
            }
            else
            {
                if (_cell.ownedPawn)
                {
                    DestroyImmediate(_cell.ownedPawn.gameObject);
                }
                GPawn pawnPrefab = instantiationCommonData.pawnTypeData[newPawnType];
                if (pawnPrefab)
                {
                    _cell.ownedPawn = PrefabUtility.InstantiatePrefab(pawnPrefab, _cell._pawnSpawnPoint) as GPawn;
                    _cell.ownedPawn.transform.localPosition = Vector3.zero;
                    _cell.ownedPawn.SetCell(_cell);
                }
                _previousPawnSpawnType = newPawnType;
            }
        }
        
        // Equipment Type
        EEquipmentType newEquipmentType = _cell._data.equipmentType;
        if(_previousEquipmentType != _cell._data.equipmentType)
        {
            if (_cell.ownedPawn)
            {
                _cell._data.equipmentType = _previousEquipmentType;
                Debug.LogWarning("Can't Change equipment when there is a pawn");
            }
            else
            {
                if (_cell._equipment)
                {
                    DestroyImmediate(_cell._equipment.gameObject);
                }
                GEquipment equipmentPrefab = instantiationCommonData.equipmentTypeData[newEquipmentType];
                if (equipmentPrefab)
                {
                    _cell._equipment = PrefabUtility.InstantiatePrefab(equipmentPrefab) as GEquipment;
                    _cell._equipment.transform.parent = _cell._pawnSpawnPoint;
                    _cell._equipment.transform.localPosition = Vector3.zero;
                    _cell._equipment.SetCell(_cell);
                }
                _previousEquipmentType = newEquipmentType;
            }
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
        var tileTypeData = cellCommonData.tileTypeData[_cell._data.tileType];
        _highlight.color = tileTypeData.highlightColor;
    }

    public void UpdateScaling(float scale)
    {
        _visualsParent.localScale = new Vector3(scale, scale, scale);
        _collidersParent.localScale = new Vector3(scale, scale, scale);
        _highlight.rectTransform.localScale = new Vector3(scale, scale, scale);
    }
}