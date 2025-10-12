using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct GCellData
{
    public enum ETileType { Normal, Wall, Hole }

    [SerializeField, ReadOnly]
    public Vector2Int gridCoordinates;
    
    [SerializeField]
    public ETileType tileType;

    public GCellData(Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = ETileType.Normal;
    }

    public bool IsCellChanged()
    {
        return tileType != ETileType.Normal;
    }
}
