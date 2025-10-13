using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GGridManager : GSingleton<GGridManager>
{
    [SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public GCell[] _grid;

    [field: SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public Vector2Int _currentGridSize;

    [HideInInspector]
    public Dictionary<Vector2Int, int> _stepMap;

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
    
    public EHexDirection[] GetPath(GCell from, GCell to, bool reloadStepMap = false)
    {
        if (reloadStepMap)
        {
            GenerateStepMap(from);
        }
        else
        {
            if (!_stepMap.ContainsKey(to._data.gridCoordinates) ||
                !_stepMap.ContainsKey(from._data.gridCoordinates)) return null;
        }
        
        GCell currentCell = to;
        int currentCellStep = _stepMap[to._data.gridCoordinates];
        EHexDirection[] path = new EHexDirection[currentCellStep];

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
                if (_stepMap.ContainsKey(gridCoordinates) && _stepMap[gridCoordinates] < currentCellStep)
                {
                    EHexDirection direction = cell._hexCoordinates.GetLineDirection(currentCell._hexCoordinates);
                    currentCell = cell;
                    currentCellStep = _stepMap[gridCoordinates];
                    path[currentCellStep] = direction;
                    i++;
                    break;
                }
            }
            i++;
        }
        return path;
    }

    public void GenerateStepMap(GCell from)
    {
        _stepMap = new Dictionary<Vector2Int, int>();
        if (!from || !from.IsWalkable(true))
        {
            return;
        }
        
        int stepNum = 0;
        _stepMap.Add(from._data.gridCoordinates, stepNum);
        from._cellVisualsController.UpdateCellDebugNum($"{stepNum}");
        StepRecursion(from, stepNum + 1);
    }

    private void StepRecursion(GCell from, int stepNum)
    {
        foreach (GCell cell in from._neighbors)
        {
            if (cell && cell.IsWalkable())
            {
                var coordinates = cell._data.gridCoordinates;
                if(!_stepMap.ContainsKey(coordinates))
                {
                    _stepMap.Add(cell._data.gridCoordinates, stepNum);
                    cell._cellVisualsController.UpdateCellDebugNum(stepNum.ToString());
                    StepRecursion(cell, stepNum + 1);
                }
                else if(_stepMap[coordinates] >= stepNum)
                {
                    _stepMap[coordinates] = stepNum;
                    cell._cellVisualsController.UpdateCellDebugNum(stepNum.ToString());
                    StepRecursion(cell, stepNum + 1);
                }

            }
        }

    }
}