using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GCell))]
public class GCellVisualsController : SerializedMonoBehaviour
{
    [SerializeField]
    GCommonData_Cell _commonCellData;

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

    public void UpdateCellVisuals()
    {
        if (_cell._UI == null) return;
        _text = _cell._UI.GetComponentInChildren<TextMeshProUGUI>();
        _highlight = _cell._UI.GetComponentInChildren<Image>();
        
        var tileTypeData = _commonCellData.TileTypeData[_cell._data.tileType];
        Material[] materials = tileTypeData.materials;
        Mesh mesh = tileTypeData.mesh;
        
        _meshRenderer.sharedMaterials = materials;
        _meshFilter.sharedMesh = mesh;

        _text.color = tileTypeData.textColor;
        _highlight.color = tileTypeData.highlightColor;
        HideInHierarchy();
        
        EditorUtility.SetDirty(this);
    }

    public void UpdateCellDebugNum(string newDebugText)
    {
        _text.text = newDebugText;
    }

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
}