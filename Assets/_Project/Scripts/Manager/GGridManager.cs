using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

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
    
    [Button]
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
        _currentGridSize = new Vector2Int(rows, columns);
        int size = columns * rows;
        _grid = new GCell[size];
        
        try
        {
            for (int row = 0, i = 0; row < rows; row++)
            {
                // Update progress bar
                EditorUtility.DisplayProgressBar(
                    "Creating Grid", 
                    $"Row {row + 1}/{rows}", 
                    (float)row / rows
                );
            
                for (int column = 0; column < columns; column++)
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

    [Button]
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
        position.x = (row + column * .5f - column / 2) * (HexMetrics.innerRadius * 2f);
        position.z = column * (HexMetrics.outerRadius * 1.5f);
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
            if (column > 0)
            {
                cell.SetNeighbor(HexDirection.W, _grid[i - 1]);
            }
            if (row > 0)
            {
                if ((row & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, _grid[i - _gridData.ColumnNum]);
                    if (column > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, _grid[i - _gridData.ColumnNum - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, _grid[i - _gridData.ColumnNum]);
                    if (row < _gridData.RowNum - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, _grid[i - _gridData.ColumnNum + 1]);
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
                cell._data = _gridData.CellData[cellCoordinates];
            }
            else
            {
                cell._data = new GCellData(cellCoordinates);
            }
        }
     
        cell._hexCoordinates = GHexCoordinate.FrommOffsetCoordinate(row, column);
    }

    [Button, FoldoutGroup("Serialization")]
    private void SaveGridLayout()
    {
        GGridData newAsset = ScriptableObject.CreateInstance<GGridData>();
        newAsset.GenerateCellData(_grid, _currentGridSize);
        UnityEditor.AssetDatabase.CreateAsset(newAsset, $"{pathToGridLayoutFolders}/{_gridDataFileName}.asset");
        UnityEditor.AssetDatabase.SaveAssets();
    }
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

public enum HexDirection {
    NE, E, SE, SW, W, NW
}