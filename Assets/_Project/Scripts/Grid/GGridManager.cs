using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


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
        int id = coordinate.X + coordinate.Z *  _currentGridSize.y + coordinate.Z / 2;
        return _grid.Length <= id ? null : _grid[id];
    }

    public GCell GetCell(Vector3 position)
    {
        return GetCell(GHexCoordinate.FromPosition(position));
    }
    
    public EHexDirection[] GetPath(GCell from, GCell to, bool reloadStepMap = false, int maxNumberOfSteps = -1)
    {
        if (reloadStepMap)
        {
            GenerateStepMap(from);
        }
        else if(_stepMap == null)
        {
            Debug.LogError("Not Step Data Available or Generated");
            return null;
        }
        
        if (!_stepMap.ContainsKey(to._data.gridCoordinates) ||
            !_stepMap.ContainsKey(from._data.gridCoordinates)) return null;
        
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
        if (maxNumberOfSteps != -1 && maxNumberOfSteps < path.Length)
        {
            Array.Resize(ref path, maxNumberOfSteps);
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

    public int GetStep(GCell targetCell, bool forceSearch = false)
    {
        if (_stepMap.ContainsKey(targetCell._data.gridCoordinates))
        {
            return _stepMap[targetCell._data.gridCoordinates];
        }
        else if (forceSearch)
        {
            int lowestStep = -1;
            foreach (GCell neighbor in targetCell._neighbors)
            {
                if (!neighbor) continue;
                if (_stepMap.ContainsKey(neighbor._data.gridCoordinates))
                {
                    int step = _stepMap[neighbor._data.gridCoordinates];
                    if (step < lowestStep || lowestStep == -1)
                    {
                        lowestStep = step;
                    }
                }
            }
            return lowestStep == -1 ? -1 : lowestStep + 1;
        }
        else return -1;
    }

    public GCell GetLowestAdjacentCell(GCell ogCell)
    {
        GCell outCell = null;
        int lowestStep = int.MaxValue;
        foreach (GCell cell in ogCell._neighbors)
        {
            Vector2Int coordinates = cell._data.gridCoordinates;
            if (_stepMap.ContainsKey(coordinates) && _stepMap[coordinates] < lowestStep)
            {
                lowestStep = _stepMap[coordinates];
                outCell = cell;
            }
        }
        return outCell;
    }
}


