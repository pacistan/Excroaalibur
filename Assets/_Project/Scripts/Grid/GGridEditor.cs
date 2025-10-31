#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class GGridEditor : MonoBehaviour
{
    const string pathToGridLayoutFolders = "Assets/_Project/Grid/GridPresets";
    
    [SerializeField] 
    [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
    GGridData _gridData;
    
    [SerializeField, FoldoutGroup("Components")]
    Transform _cellsParent;
    [SerializeField, FoldoutGroup("Components")]
    Canvas _cellsCanvas;
    [SerializeField, FoldoutGroup("Components")]
    GCell _cellPrefab;
    [SerializeField, FoldoutGroup("Components")]
    RectTransform _cellUiPrefab;
    
    [SerializeField, FoldoutGroup("Serialization")]
    string _gridDataFileName;
    
    [Tooltip("Makes the UI Cells not Selectable in the Scene view")]
    [SerializeField, OnValueChanged("EnablePickingUIGrid")]
    bool _isUIGridPickable = true;
    
    [HorizontalGroup("Split", 0.5f)]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    public void InstantiateGrid()
    {
        if (!_gridData || !_cellPrefab || !_cellUiPrefab || !_cellsCanvas || !_cellsParent)
        {
            Debug.LogError("Can't instantiate grid");
            return;
        }
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();      
        ClearCells();
        
        int rows = _gridData.rowNum;
        int columns = _gridData.columnNum;
        gridManager._currentGridSize = new Vector2Int(columns, rows);
        gridManager._isOffsetOnPairs = _gridData.isOffsetOnPairs;
        gridManager._hexSize = _gridData.hexSize;
        int size = rows * columns;
        gridManager.grid = new GCell[size];

        try
        {
            for (int column = 0, i = 0; column < columns; column++)
            {
                // Update progress bar
                EditorUtility.DisplayProgressBar(
                    "Creating Grid", 
                    $"Row {column + 1}/{columns}", 
                    (float)column / columns
                );
            
                for (int row = 0; row < rows; row++)
                {
                    CreateCell(row, column, i++, ref gridManager.grid);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        
        EditorUtility.SetDirty(gridManager);
        EditorUtility.SetDirty(_gridData);
        EditorUtility.SetDirty(_cellsParent);
        EditorUtility.SetDirty(_cellsCanvas);
        EnablePickingUIGrid();
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button(ButtonSizes.Large), GUIColor(.9f, 0.2f, .1f)]
    public void ClearCells()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();
        GCell[] grid = gridManager.grid;
        if (grid != null)
        {
            for (int i = grid.Length - 1; i >= 0; i--)
            {
                if (grid[i] == null) continue;
                if (grid[i].ui != null)
                    DestroyImmediate(grid[i].ui.gameObject);
                DestroyImmediate(grid[i].gameObject);
            }

            grid = null;
        }
    }

    private void CreateCell(int row, int column, int i, ref GCell[] grid)
    {
        Vector3 position;
        int pairOffset = _gridData.isOffsetOnPairs ? 0 : 0;
        position.x = (row + column * .5f - (column + pairOffset) / 2) * (GHexMetrix.innerRadius * 2f);
        position.z = column * (_gridData.hexSize * 1.5f);
        position.y = 0;
        
        GCell cell = grid[i] = PrefabUtility.InstantiatePrefab(_cellPrefab, _cellsParent) as GCell;
        if (cell == null)
        {
            Debug.LogError($"Cell {row}, {column} was not Instantiated");
            return;
        }
        
        // Cell Init
        {
            cell.transform.localPosition = position;
            cell.gameObject.name = $"Cell {row}, {column}";
            cell.Initialize();
            if (row > 0)
            {
                GCell neighbor = grid[i - 1];
                cell.SetNeighbor(EHexDirection.W, neighbor);
            }
            if (column > 0)
            {
                if ((column & 1) == 0)
                {
                    GCell neighbor = grid[i - _gridData.rowNum];
                    cell.SetNeighbor(EHexDirection.SE, neighbor);
                    if (row > 0)
                    {
                        neighbor = grid[i - _gridData.rowNum - 1];
                        cell.SetNeighbor(EHexDirection.SW, neighbor);
                    }
                }
                else
                {
                    GCell neighbor = grid[i - _gridData.rowNum];
                    cell.SetNeighbor(EHexDirection.SW, neighbor);
                    if (row < _gridData.rowNum - 1)
                    {
                        neighbor = grid[i - _gridData.rowNum + 1];
                        cell.SetNeighbor(EHexDirection.SE, neighbor);
                    }
                }
            }
        }
        
        RectTransform rectTr = PrefabUtility.InstantiatePrefab(_cellUiPrefab, _cellsCanvas.transform) as RectTransform;
        if (!rectTr)
        {
            Debug.LogError($"Label for cell at {row}, {column} was not Instantiated");
            return;
        }
        
        // Cell UI Init
        {
            rectTr.gameObject.name = $"Label_Cell {row}, {column}";
            rectTr.anchoredPosition = new Vector2(position.x, position.z);
            cell.ui = rectTr;
        }
        
        // Cell Data Init
        {
            Vector2Int cellCoordinates = new Vector2Int(row, column);
            if (_gridData.cellData != null && _gridData.cellData.ContainsKey(cellCoordinates))
            {
                cell.data = new GCellData(_gridData.cellData[cellCoordinates], cellCoordinates);
            }
            else
            {
                cell.data = new GCellData(cellCoordinates);
            }
        }
        cell.visuals.UpdateCellVisuals();
        cell.visuals.UpdateScaling(_gridData.hexSize);
        cell.hexCoordinates = GHexCoordinate.FrommOffsetCoordinate(row, column);
        foreach (var visualPreset in cell.data.paintedVisuals)
        {
            cell.visuals.OnCreateVisualPreset(visualPreset, visualPreset.rotation, false);
        }
        
        EditorUtility.SetDirty(cell);
        EditorUtility.SetDirty(cell.visuals);
    }

    // Creates a new ScriptableObject of type GGridData in the referenced Folder with the data of the active grid
    [Button, FoldoutGroup("Serialization")]
    private void SaveGridLayout()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();
        GGridData newAsset = ScriptableObject.CreateInstance<GGridData>();
        newAsset.GenerateCellData(gridManager.grid, gridManager._currentGridSize, gridManager._hexSize, gridManager._isOffsetOnPairs);
        UnityEditor.AssetDatabase.CreateAsset(newAsset, $"{pathToGridLayoutFolders}/{_gridDataFileName}.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        _gridData = newAsset;
    }

    private void EnablePickingUIGrid()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();
        SceneVisibilityManager manager = SceneVisibilityManager.instance;
        if (_isUIGridPickable)
        {
            manager.EnablePicking(_cellsCanvas.gameObject, true);
            foreach (var cell in gridManager.grid)
            {
                var owningPawn = cell.GetGridObject<GPawn>();
                if (owningPawn)
                {
                    owningPawn.visuals.SetClickable(manager, true);
                }
            }
        }
        else
        {
            manager.DisablePicking(_cellsCanvas.gameObject, true);
            foreach (var cell in gridManager.grid)
            {
                var owningPawn = cell.GetGridObject<GPawn>();
                if (owningPawn)
                {
                    owningPawn.visuals.SetClickable(manager, false);
                }
            }
        }
    }
}

#endif