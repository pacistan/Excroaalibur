using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "Grid Data", order = 1)]
public class GGridData : SerializedScriptableObject
{
    [field : SerializeField] public int columnNum { get; private set; }
    [field : SerializeField] public int rowNum { get; private set; }
    [field : SerializeField] public Dictionary<Vector2Int, GCellData> cellData { get; set; }

#if UNITY_EDITOR
    /// <summary>
    /// Override the CellData of this GridData Scriptable Object with the CellData of the active grid
    /// </summary>
    /// <param name="grid">The active grid</param>
    /// <param name="gridSize">The size in rows and columns of the active grid</param>
    public void GenerateCellData(GCell[] grid, Vector2Int gridSize)
    {
        columnNum = gridSize.x;
        rowNum = gridSize.y;
        cellData = new Dictionary<Vector2Int, GCellData>();
        for (int row = 0, i = 0; row < gridSize.x; row++)
        {
            for (int column = 0; column < gridSize.y; column++)
            {
                if (grid[i]._data.IsCellChanged())
                {
                    cellData.Add(new Vector2Int(row, column), grid[i]._data);
                }
                i++;
            }
        }
        EditorUtility.SetDirty(this);
    }
#endif
}
