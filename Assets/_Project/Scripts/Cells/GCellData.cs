using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static GCellVisualsController;

[System.Serializable]
public struct GCellData
{
    public enum ETileType { Normal, Wall, Hole }

    [SerializeField, ReadOnly]
    public Vector2Int gridCoordinates;
    
    [SerializeField]
    public ETileType tileType;

    [FormerlySerializedAs("pion")]
    [SerializeField]
    public EPawnSpawnType pawnType;

    
    public GCellData(Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = ETileType.Normal;
        pawnType = EPawnSpawnType.None;
    }

    public GCellData(GCellData inCellData,Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = inCellData.tileType;
        pawnType = inCellData.pawnType;
    }

    public bool IsCellChanged()
    {
        return tileType != ETileType.Normal || pawnType != EPawnSpawnType.None;
    }
}
