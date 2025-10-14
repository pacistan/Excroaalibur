using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(GCell))]
public class GCellVisualsController : SerializedMonoBehaviour
{
    public enum EPawnSpawnType {None, Sentry, PlayerPawn}

    
    [FormerlySerializedAs("_commonCellData")]
    [SerializeField]
    GCellCommonData cellCommonData;

    [SerializeField, OnValueChanged("HideInHierarchy")]
    bool _showVisualsInHierarchy;
    
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
    Transform _pawnSpawnPoint;

    [SerializeField, HideInInspector]
    EPawnSpawnType _previousPawnSpawnType;
    
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
            if (_cell._ownedPawn)
            {
                DestroyImmediate(_cell._ownedPawn.gameObject);
            }
            GPawn pawnPrefab = cellCommonData.pawnTypeData[newPawnType];
            if (pawnPrefab)
            {
                _cell._ownedPawn = PrefabUtility.InstantiatePrefab(pawnPrefab, _pawnSpawnPoint) as GPawn;
                _cell._ownedPawn.transform.localPosition = Vector3.zero;
                _cell._ownedPawn.SetCell(_cell);
            }
            _previousPawnSpawnType = newPawnType;
        }
        
        // UI
        {
            _text.color = tileTypeData.textColor;
            _highlight.color = tileTypeData.highlightColor;
        }
        
        HideInHierarchy();
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(_cell);
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

    #if UNITY_EDITOR
    private void HideInHierarchy()
    {
        if (_showVisualsInHierarchy)
        {
            _meshRenderer.transform.parent.gameObject.hideFlags = HideFlags.None;
        }
        else
        {
            _meshRenderer.transform.parent.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
    #endif
}