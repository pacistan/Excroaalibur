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

    [SerializeField, ReadOnly]
    public GPion pion;

    public GCellData(Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = ETileType.Normal;
        pion = null;
    }

    public GCellData(GCellData inCellData,Vector2Int inGridCoordinates, GPion inPion = null)
    {
        gridCoordinates = inGridCoordinates;
        tileType = inCellData.tileType;
        pion = inPion;
    }

    public bool IsCellChanged()
    {
        return tileType != ETileType.Normal;
    }
}
