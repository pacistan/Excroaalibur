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
        ClearDebug();
        _stepMap = GPathfindingUtility.GetStepMap(_startCell);
    }
    
    [Button]
    private void TestPathFinding()
    {
        if (_stepMap == null)
        {
            _stepMap = GPathfindingUtility.GetStepMap(_startCell);
        }
        var path = GPathfindingUtility.GetPath(_startCell, _endCell, _stepMap);
        foreach (var hexDirection in path)
        {
            Debug.Log(hexDirection);
        }
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
