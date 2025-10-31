using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GGridManager : GSingleton<GGridManager>
{
    [FormerlySerializedAs("_grid")]
    [SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public GCell[] grid;

    [field: SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public Vector2Int _currentGridSize;

    [field: SerializeReference, ReadOnly, FoldoutGroup("Grid Data")]
    public float _hexSize;

    [field: SerializeReference, ReadOnly, FoldoutGroup("Grid Data"), HideInInspector]
    public bool _isOffsetOnPairs;
    
    [HideInInspector]
    public Dictionary<Vector2Int, int> _stepMap;

    [field: SerializeField]
    public Transform painterParent { get; private set; }

    public GCell GetCell(GHexCoordinate coordinate)
    {
        int id = coordinate.X + coordinate.Z *  _currentGridSize.y + coordinate.Z / 2;
        return grid.Length <= id ? null : grid[id];
    }

    public GCell GetCell(Vector3 position)
    {
        return GetCell(GHexCoordinate.FromPosition(position));
    }

    public GCell GetCell(Vector2Int gridCoordinate)
    {
        return grid[gridCoordinate.x + gridCoordinate.y * _currentGridSize.y ];
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
        
        if (!_stepMap.ContainsKey(to.data.gridCoordinates) ||
            !_stepMap.ContainsKey(from.data.gridCoordinates)) return null;

        GCell currentCell = to;
        int currentCellStep = _stepMap[to.data.gridCoordinates];
        
        /*if (maxNumberOfSteps != -1 && maxNumberOfSteps < currentCellStep)
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
        }*/

        EHexDirection[] path = new EHexDirection[currentCellStep];

        int i = 0;
        while (currentCellStep != 0 && i < 4000)
        {
            foreach (var cell in currentCell.neighbors)
            {
                if (!cell)
                {
                    continue;
                }
                Vector2Int gridCoordinates = cell.data.gridCoordinates;
                if (_stepMap.ContainsKey(gridCoordinates) && _stepMap[gridCoordinates] < currentCellStep)
                {
                    EHexDirection direction = cell.hexCoordinates.GetLineDirection(currentCell.hexCoordinates);
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
        
        if (!_stepMap.ContainsKey(to.data.gridCoordinates) ||
            !_stepMap.ContainsKey(from.data.gridCoordinates)) return null;

        GCell currentCell = to;
        int currentCellStep = _stepMap[to.data.gridCoordinates];
        /*if (maxNumberOfSteps != -1 && maxNumberOfSteps < currentCellStep)
        {
            var finalStep = _stepMap.First(a =>
            {
                bool isRightStepValue = a.Value == maxNumberOfSteps;
                return isRightStepValue;
            });
            currentCell = GGridManager.Instance.GetCell(finalStep.Key);
            if (!currentCell) return null;
            currentCellStep = finalStep.Value;
        }*/
        EHexDirection[] path = new EHexDirection[currentCellStep];

        int i = 0;
        while (currentCellStep != 0 && i < 4000)
        {
            foreach (var cell in currentCell.neighbors)
            {
                if (!cell)
                {
                    continue;
                }
                Vector2Int gridCoordinates = cell.data.gridCoordinates;
                if (_stepMap.ContainsKey(gridCoordinates) && _stepMap[gridCoordinates] < currentCellStep)
                {
                    EHexDirection direction = cell.hexCoordinates.GetLineDirection(currentCell.hexCoordinates);
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

    
    /** Generate Step map with only <see cref="ETileType.Normal"/> */
    public void GenerateStepMap(GCell from)
    {
        _stepMap = new Dictionary<Vector2Int, int>();
        if (!from || !from.IsWalkable(true))
        {
            return;
        }
        
        int stepNum = 0;
        _stepMap.Add(from.data.gridCoordinates, stepNum);
        from.visuals.UpdateCellDebugNum($"{stepNum}");
        ETileType[] walkableTypes = new []{ ETileType.Normal };
        StepRecursion(from, stepNum + 1, ref walkableTypes);
    }
    
    public void GenerateStepMap(GCell from, ETileType[] walkableTypes)
    {
        _stepMap = new Dictionary<Vector2Int, int>();
        if (!from || !from.IsWalkable(ref walkableTypes, true))
        {
            return;
        }
        
        int stepNum = 0;
        _stepMap.Add(from.data.gridCoordinates, stepNum);
        from.visuals.UpdateCellDebugNum($"{stepNum}");
        StepRecursion(from, stepNum + 1, ref walkableTypes);
    }

    private void StepRecursion(GCell from, int stepNum, ref ETileType[] walkableTypes)
    {
        foreach (GCell cell in from.neighbors)
        {
            if (cell && cell.IsWalkable(ref walkableTypes))
            {
                var coordinates = cell.data.gridCoordinates;
                if(!_stepMap.ContainsKey(coordinates))
                {
                    _stepMap.Add(cell.data.gridCoordinates, stepNum);
                    cell.visuals.UpdateCellDebugNum(stepNum.ToString());
                    StepRecursion(cell, stepNum + 1, ref walkableTypes);
                }
                else if(_stepMap[coordinates] > stepNum)
                {
                    _stepMap[coordinates] = stepNum;
                    cell.visuals.UpdateCellDebugNum(stepNum.ToString());
                    StepRecursion(cell, stepNum + 1, ref walkableTypes);
                }
            }
        }

    }

    public int GetStep(GCell targetCell, bool forceSearch = false)
    {
        if (_stepMap.ContainsKey(targetCell.data.gridCoordinates))
        {
            return _stepMap[targetCell.data.gridCoordinates];
        }
        else if (forceSearch)
        {
            int lowestStep = -1;
            foreach (GCell neighbor in targetCell.neighbors)
            {
                if (!neighbor) continue;
                if (_stepMap.ContainsKey(neighbor.data.gridCoordinates))
                {
                    int step = _stepMap[neighbor.data.gridCoordinates];
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
        if (_stepMap.ContainsKey(ogCell.data.gridCoordinates))
        {
            return ogCell;
        }
        GCell outCell = null;
        int lowestStep = int.MaxValue;
        foreach (GCell cell in ogCell.neighbors)
        {
            if (!cell) continue;
            Vector2Int coordinates = cell.data.gridCoordinates;
            if (_stepMap.ContainsKey(coordinates) && _stepMap[coordinates] < lowestStep)
            {
                lowestStep = _stepMap[coordinates];
                outCell = cell;
            }
        }
        return outCell;
    }
    
    public List<GCell> GetAllCellsOfType(ETileType tileType)
    {
        List<GCell> cellsOfType = new List<GCell>();
        foreach (GCell cell in grid)
        {
            if (cell.data.tileType == tileType)
            {
                cellsOfType.Add(cell);
            }
        }
        return cellsOfType;
    }

    void Start()
    {
        //stepMap = new int[_grid.Length];
        //stepMap.Populate(-1);
    }
}


