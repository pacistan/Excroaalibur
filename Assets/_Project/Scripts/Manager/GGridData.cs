using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "Grid Data", order = 1)]
public class GGridData : SerializedScriptableObject
{
    [field : SerializeField] public int RowNum { get; private set; }
    [field : SerializeField] public int ColumnNum { get; private set; }
    [field : SerializeField] public Dictionary<Vector2Int, GCellData> CellData { get; set; }


    public void GenerateCellData(GCell[] _grid, Vector2Int gridSize)
    {
        RowNum = gridSize.x;
        ColumnNum = gridSize.y;
        CellData = new Dictionary<Vector2Int, GCellData>();
        for (int row = 0, i = 0; row < gridSize.x; row++)
        {
            for (int column = 0; column < gridSize.y; column++)
            {
                if (_grid[i]._data.IsCellChanged())
                {
                    CellData.Add(new Vector2Int(row, column), _grid[i]._data);
                }
                i++;
            }
        }
        EditorUtility.SetDirty(this);
    }
}
