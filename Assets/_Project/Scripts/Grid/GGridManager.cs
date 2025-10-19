using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GGridManager : GSingleton<GGridManager>
{
    [SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public GCell[] _grid;

    [field: SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public Vector2Int _currentGridSize;

    [field: SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public float _hexSize;

    [field: SerializeReference, ReadOnly, FoldoutGroup("Grid Data"), HideInInspector]
    public bool _isOffsetOnPairs;
    
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

    public GCell GetCell(Vector2Int gridCoordinate)
    {
        return _grid[gridCoordinate.x + gridCoordinate.y * _currentGridSize.y ];
    }
    
    public EHexDirection[] GetPath(GCell from, GCell to, ETileType[] possibleEndTypes, bool reloadStepMap = false, int maxNumberOfSteps = -1)
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
        if (maxNumberOfSteps != -1 && maxNumberOfSteps < currentCellStep)
        {
            var finalStep = _stepMap.First(a =>
            {
                bool isRightStepValue = a.Value == maxNumberOfSteps;
                GCell cell = GGridManager.Instance.GetCell(a.Key);
                bool isTileWalkable = cell.IsWalkable(ref possibleEndTypes, false);
                return isRightStepValue;
            });
            currentCell = GGridManager.Instance.GetCell(finalStep.Key);
            if (!currentCell) return null;
            currentCellStep = finalStep.Value;
        }

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
        if (maxNumberOfSteps != -1 && maxNumberOfSteps < currentCellStep)
        {
            var finalStep = _stepMap.First(a =>
            {
                bool isRightStepValue = a.Value == maxNumberOfSteps;
                return isRightStepValue;
            });
            currentCell = GGridManager.Instance.GetCell(finalStep.Key);
            if (!currentCell) return null;
            currentCellStep = finalStep.Value;
        }
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

    
    /** Generate Step map with only <see cref="ETileType.Normal"/> */
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
        ETileType[] walkableTypes = new []{ ETileType.Normal };
        StepRecursion(from, stepNum + 1, ref walkableTypes);
    }
    
    public void GenerateStepMap(GCell from, ETileType[] walkableTypes)
    {
        _stepMap = new Dictionary<Vector2Int, int>();
        if (!from || !from.IsWalkable(true))
        {
            return;
        }
        
        int stepNum = 0;
        _stepMap.Add(from._data.gridCoordinates, stepNum);
        from._cellVisualsController.UpdateCellDebugNum($"{stepNum}");
        StepRecursion(from, stepNum + 1, ref walkableTypes);
    }

    private void StepRecursion(GCell from, int stepNum, ref ETileType[] walkableTypes)
    {
        foreach (GCell cell in from._neighbors)
        {
            if (cell && cell.IsWalkable(ref walkableTypes))
            {
                var coordinates = cell._data.gridCoordinates;
                if(!_stepMap.ContainsKey(coordinates))
                {
                    _stepMap.Add(cell._data.gridCoordinates, stepNum);
                    cell._cellVisualsController.UpdateCellDebugNum(stepNum.ToString());
                    StepRecursion(cell, stepNum + 1, ref walkableTypes);
                }
                else if(_stepMap[coordinates] >= stepNum)
                {
                    _stepMap[coordinates] = stepNum;
                    cell._cellVisualsController.UpdateCellDebugNum(stepNum.ToString());
                    StepRecursion(cell, stepNum + 1, ref walkableTypes);
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
        if (_stepMap.ContainsKey(ogCell._data.gridCoordinates))
        {
            return ogCell;
        }
        GCell outCell = null;
        int lowestStep = int.MaxValue;
        foreach (GCell cell in ogCell._neighbors)
        {
            if (!cell) continue;
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


