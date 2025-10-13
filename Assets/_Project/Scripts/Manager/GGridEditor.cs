#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class GGridEditor : MonoBehaviour
{
    const string pathToGridLayoutFolders = "Assets/_Project/Grid/GridPresets";
    
    [SerializeField] GGridData _gridData;
    
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
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : FindObjectOfType<GGridManager>();      
        ClearCells();
        
        int rows = _gridData.rowNum;
        int columns = _gridData.columnNum;
        gridManager._currentGridSize = new Vector2Int(columns, rows);
        int size = rows * columns;
        gridManager._grid = new GCell[size];
        
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
                    CreateCell(row, column, i++, ref gridManager._grid);
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
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button(ButtonSizes.Large), GUIColor(.9f, 0.2f, .1f)]
    public void ClearCells()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : FindObjectOfType<GGridManager>();
        GCell[] grid = gridManager._grid;
        if (grid != null)
        {
            for (int i = grid.Length - 1; i >= 0; i--)
            {
                if (grid[i] == null) continue;
                if (grid[i]._ui != null)
                    DestroyImmediate(grid[i]._ui.gameObject);
                DestroyImmediate(grid[i].gameObject);
            }

            grid = null;
        }
    }
    
    private void CreateCell(int row, int column, int i, ref GCell[] grid)
    {
        Vector3 position;
        position.x = (row + column * .5f - column / 2) * (GHexMetrics.innerRadius * 2f);
        position.z = column * (GHexMetrics.outerRadius * 1.5f);
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
            cell._ui = rectTr;
        }
        
        // Cell Data Init
        {
            Vector2Int cellCoordinates = new Vector2Int(row, column);
            if (_gridData.cellData != null && _gridData.cellData.ContainsKey(cellCoordinates))
            {
                cell._data = new GCellData(_gridData.cellData[cellCoordinates], cellCoordinates);
            }
            else
            {
                cell._data = new GCellData(cellCoordinates);
            }
        }
        cell._cellVisualsController.UpdateCellVisuals();
        cell._hexCoordinates = GHexCoordinate.FrommOffsetCoordinate(row, column);
    }

    // Creates a new ScriptableObject of type GGridData in the referenced Folder with the data of the active grid
    [Button, FoldoutGroup("Serialization")]
    private void SaveGridLayout()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : FindObjectOfType<GGridManager>();
        GGridData newAsset = ScriptableObject.CreateInstance<GGridData>();
        newAsset.GenerateCellData(gridManager._grid, gridManager._currentGridSize);
        UnityEditor.AssetDatabase.CreateAsset(newAsset, $"{pathToGridLayoutFolders}/{_gridDataFileName}.asset");
        UnityEditor.AssetDatabase.SaveAssets();
    }

    private void EnablePickingUIGrid()
    {
        SceneVisibilityManager manager = SceneVisibilityManager.instance;
        manager.DisablePicking(_cellsCanvas.gameObject, true);
    }
}

#endif