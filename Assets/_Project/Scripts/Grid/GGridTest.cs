using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GGridTest : MonoBehaviour
{
    [FormerlySerializedAs("_startCoordinates")]
    [SerializeField]
    GCell _startCell;

    [FormerlySerializedAs("_endCoordinates")]
    [SerializeField]
    GCell _endCell;

    [SerializeField, HideInInspector]
    Dictionary<Vector2Int, int> _stepMap;

    [SerializeField]
    Vector2Int _testCoordinates;
    
    [Button]
    private void TestStepping()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();
        ClearDebug();
        gridManager.GenerateStepMap(_startCell);
    }
    
    [Button]
    private void TestPathFinding()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();
        var path =  gridManager.GetPath(_startCell, _endCell, true);
        GCell currentCell = _startCell;
        currentCell.visuals.UpdateCellDebugNum("O");
        foreach (var hexDirection in path)
        {
            currentCell = currentCell.neighbors[(int)hexDirection];
            currentCell.visuals.UpdateCellDebugNum("|||");
        }
        currentCell.visuals.UpdateCellDebugNum("X");
    }

    [Button]
    private void ClearDebug()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();
        foreach (var cell in gridManager._grid)
        {
            cell.visuals.UpdateCellDebugNum("");
        }
    }

    [Button]
    private void TestGrid()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();
        gridManager.GetCell(_testCoordinates);
        Debug.Log(_testCoordinates + " || " + gridManager._grid[_testCoordinates.x + _testCoordinates.y * _testCoordinates.x].data.gridCoordinates);

    }
}
