using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GGridManager : GSingleton<GGridManager>
{
    const string pathToGridLayoutFolders = "Assets/_Project/Grid/GridPresets";
    
    [SerializeField] GGridData _gridData;
    [field : SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public GCell[] _grid { get; private set; }
    
    [field : SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public  Vector2Int _currentGridSize { get; private set; }
    
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

    public GCell GetCell(GHexCoordinate coordinate)
    {
        int id = coordinate.X + coordinate.Y;
        if (_grid.Length > id) return null;
        return _grid[id];
    }

    public GCell GetCell(Vector3 position)
    {
        return GetCell(GHexCoordinate.FromPosition(position));
    }
    #if UNITY_EDITOR
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
                    
        ClearCells();
        
        int rows = _gridData.RowNum;
        int columns = _gridData.ColumnNum;
        _currentGridSize = new Vector2Int(columns, rows);
        int size = rows * columns;
        _grid = new GCell[size];
        
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
                    CreateCell(row, column, i++);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(_gridData);
        EditorUtility.SetDirty(_cellsParent);
        EditorUtility.SetDirty(_cellsCanvas);
    }

    [HorizontalGroup("Split", 0.5f)]
    [Button(ButtonSizes.Large), GUIColor(.9f, 0.2f, .1f)]
    public void ClearCells()
    {
        if (_grid != null)
        {
            for (int i = _grid.Length - 1; i >= 0; i--)
            {
                if (_grid[i] == null) continue;
                if (_grid[i]._UI != null)
                    DestroyImmediate(_grid[i]._UI.gameObject);
                DestroyImmediate(_grid[i].gameObject);
            }

            _grid = null;
        }
    }
    
    private void CreateCell(int row, int column, int i)
    {
        Vector3 position;
        position.x = (row + column * .5f - column / 2) * (GHexMetrics.innerRadius * 2f);
        position.z = column * (GHexMetrics.outerRadius * 1.5f);
        position.y = 0;
     
        GCell cell = _grid[i] = PrefabUtility.InstantiatePrefab(_cellPrefab, _cellsParent) as GCell;
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
                GCell neighbor = _grid[i - 1];
                cell.SetNeighbor(HexDirection.W, neighbor);
            }
            if (column > 0)
            {
                if ((column & 1) == 0)
                {
                    GCell neighbor = _grid[i - _gridData.RowNum];
                    cell.SetNeighbor(HexDirection.SE, neighbor);
                    if (row > 0)
                    {
                        neighbor = _grid[i - _gridData.RowNum - 1];
                        cell.SetNeighbor(HexDirection.SW, neighbor);
                    }
                }
                else
                {
                    GCell neighbor = _grid[i - _gridData.RowNum];
                    cell.SetNeighbor(HexDirection.SW, neighbor);
                    if (row < _gridData.RowNum - 1)
                    {
                        neighbor = _grid[i - _gridData.RowNum + 1];
                        cell.SetNeighbor(HexDirection.SE, neighbor);
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
            cell._UI = rectTr;
        }
        
        // Cell Data Init
        {
            Vector2Int cellCoordinates = new Vector2Int(row, column);
            if (_gridData.CellData != null && _gridData.CellData.ContainsKey(cellCoordinates))
            {
                cell._data = new GCellData(_gridData.CellData[cellCoordinates], cellCoordinates);
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
        GGridData newAsset = ScriptableObject.CreateInstance<GGridData>();
        newAsset.GenerateCellData(_grid, _currentGridSize);
        UnityEditor.AssetDatabase.CreateAsset(newAsset, $"{pathToGridLayoutFolders}/{_gridDataFileName}.asset");
        UnityEditor.AssetDatabase.SaveAssets();
    }

    private void EnablePickingUIGrid()
    {
        SceneVisibilityManager manager = SceneVisibilityManager.instance;
        manager.DisablePicking(_cellsCanvas.gameObject, true);
    }
#endif
}

public static class HexDirectionExtensions {
    public static HexDirection Opposite (this HexDirection direction) {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection ToRight(this HexDirection direction)
    {
        return (HexDirection)(((int)direction + 1) % 6);
    }

    public static HexDirection ToLeft(this HexDirection direction)
    {
        return (HexDirection)(((int)direction - 1) % 6);
    }

}

public static class GPathfindingUtility
{
    public static HexDirection[] GetPath(GCell from, GCell to,  Dictionary<Vector2Int, int> stepMap)
    {
        if (!stepMap.ContainsKey(to._data.gridCoordinates)) return null;
        
        GCell currentCell = to;
        int currentCellStep = stepMap[to._data.gridCoordinates];
        HexDirection[] path = new HexDirection[currentCellStep];

        int i = 0;
        while (currentCellStep != 0 && i < 4000)
        {
            foreach (var cell in currentCell._neighbors)
            {
                if (!cell)
                {
                    continue;
                }
                Vector2Int gridCoordinates = cell._data.gridCoordinates;
                if (stepMap.ContainsKey(gridCoordinates) && stepMap[gridCoordinates] < currentCellStep)
                {
                    HexDirection direction = cell._hexCoordinates.GetLineDirection(currentCell._hexCoordinates);
                    currentCell = cell;
                    currentCellStep = stepMap[gridCoordinates];
                    path[currentCellStep] = direction;
                    cell._cellVisualsController.UpdateCellDebugNum($"|||");
                    i++;
                    break;
                }
            }
            i++;
        }
        return path;
    }

    public static Dictionary<Vector2Int, int> GetStepMap(GCell from)
    {
        Dictionary<Vector2Int, int> stepMap = new Dictionary<Vector2Int, int>();
        int stepNum = 0;
        
        stepMap.Add(from._data.gridCoordinates, stepNum);
        from._cellVisualsController.UpdateCellDebugNum($"{stepNum}");
        StepRecursion(from, ref stepMap, stepNum + 1);
        
        return stepMap;
    }

    private static void StepRecursion(GCell from, ref Dictionary<Vector2Int, int> stepMap, int stepNum)
    {
        foreach (GCell cell in from._neighbors)
        {
            if (cell && cell.IsWalkable())
            {
                var coordinates = cell._data.gridCoordinates;
                if(!stepMap.ContainsKey(coordinates))
                {
                    stepMap.Add(cell._data.gridCoordinates, stepNum);
                    cell._cellVisualsController.UpdateCellDebugNum(stepNum.ToString());
                    StepRecursion(cell, ref stepMap, stepNum + 1);
                }
                else if(stepMap[coordinates] >= stepNum)
                {
                    stepMap[coordinates] = stepNum;
                    cell._cellVisualsController.UpdateCellDebugNum(stepNum.ToString());
                    StepRecursion(cell, ref stepMap, stepNum + 1);
                }

            }
        }

    }
    
}

public enum HexDirection {
    NE, E, SE, SW, W, NW
}