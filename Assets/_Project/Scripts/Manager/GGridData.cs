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
        CellData = new Dictionary<Vector2Int, GCellData>();
        for (int z = 0, i = 0; z < gridSize.x; z++)
        {
            for (int x = 0; x < gridSize.y; x++)
            {
                if (_grid[i]._data.IsCellChanged())
                {
                    CellData.Add(new Vector2Int(x, z), _grid[i]._data);
                }
                i++;
            }
        }
        EditorUtility.SetDirty(this);
    }
}
