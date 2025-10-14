using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static GCellVisualsController;

[System.Serializable]
public struct GCellData
{
    public enum ETileType { Normal, Wall, Hole }
    public enum EPawnSpawnType {None, Sentry, PlayerPawn, Altar}
    public enum EEquipmentType {None, Crown}

    [SerializeField, ReadOnly]
    public Vector2Int gridCoordinates;
    
    [SerializeField]
    public ETileType tileType;

    [SerializeField, EnableIf("equipmentType", EEquipmentType.None)]
    public EPawnSpawnType pawnType;
    
    [SerializeField, EnableIf("pawnType", EPawnSpawnType.None)]
    public EEquipmentType equipmentType;

    
    public GCellData(Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = ETileType.Normal;
        pawnType = EPawnSpawnType.None;
        equipmentType = EEquipmentType.None;
    }

    public GCellData(GCellData inCellData,Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = inCellData.tileType;
        pawnType = inCellData.pawnType;
        equipmentType = inCellData.equipmentType;
    }

    public bool IsCellChanged()
    {
        return tileType != ETileType.Normal || pawnType != EPawnSpawnType.None || equipmentType != EEquipmentType.None;
    }
}
