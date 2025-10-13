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
        currentCell._cellVisualsController.UpdateCellDebugNum("O");
        foreach (var hexDirection in path)
        {
            currentCell = currentCell._neighbors[(int)hexDirection];
            currentCell._cellVisualsController.UpdateCellDebugNum("|||");
        }
        currentCell._cellVisualsController.UpdateCellDebugNum("X");
    }

    [Button]
    private void ClearDebug()
    {
        GGridManager gridManager = GGridManager.Instance ? GGridManager.Instance : GameObject.FindFirstObjectByType<GGridManager>();
        foreach (var cell in gridManager._grid)
        {
            cell._cellVisualsController.UpdateCellDebugNum("");
        }
    }
}
