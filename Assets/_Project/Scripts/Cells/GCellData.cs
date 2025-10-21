using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static GCellVisualsController;

    public enum ETileType { Normal, Wall, Hole }
    public enum EGridObjectType {None, Sentry, PlayerPawn, Altar, Brawler, Speedy, Crown}

[System.Serializable]
public struct GCellData
{
    [SerializeField, ReadOnly]
    public Vector2Int gridCoordinates;
    
    [SerializeField]
    public ETileType tileType;

    [SerializeField, ShowIf("tileType", ETileType.Normal)]
    public EGridObjectType objectType;
    
    public GCellData(Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = ETileType.Normal;
        objectType = EGridObjectType.None;
    }

    public GCellData(GCellData inCellData,Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = inCellData.tileType;
        objectType = inCellData.objectType;
    }

    public bool IsCellChanged()
    {
        return tileType != ETileType.Normal || objectType != EGridObjectType.None;
    }
}
