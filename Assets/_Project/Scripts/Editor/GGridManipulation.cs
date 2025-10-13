using System;
using UnityEditor;
using UnityEngine;

// Handles Input Events to modify Grid
[InitializeOnLoad]
public class GGridManipulation
{

    static GGridManipulation()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sv)
    {
        var e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Keypad1)
        {
            OnChangeTileType();
            e.Use();
            SceneView.RepaintAll();
        }
        

    }

    private static void OnChangeTileType()
    {
        var selectedObjs = Selection.gameObjects;
        foreach (var obj in selectedObjs)
        {
            if (!obj.TryGetComponent<GCell>(out var cell))
                continue;

            Undo.RecordObject(cell, "Cycle Tile Type");

            var values = (GCellData.ETileType[])Enum.GetValues(typeof(GCellData.ETileType));
            int len = values.Length;
            int cur = (int)cell._data.tileType;
            int next = (cur + 1) % len;

            // If _data is a struct, modify via copy then reassign:
            var data = cell._data;
            data.tileType = values[next];
            cell._data = data;
            cell._cellVisualsController.UpdateCellVisuals();
            EditorUtility.SetDirty(cell);
        }
    }
}
