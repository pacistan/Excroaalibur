using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

    public enum ETileType { Normal, Wall, Hole, Spawner}
    public enum EGridObjectType { None, Sentry, PlayerPawn, PlayerBig, PlayerWoke, Altar, Brawler, Speedy, Crown, PushableWall, DestroyableWall }
    public enum ETileHighlightType { CellBase, CellHover, CellSelect  }
    public enum EZoneActionType { Default, Move, Push, Throw, EnnemyAction }

[System.Serializable]
public struct GCellData
{
    //TODO : Add height
    [SerializeField, ReadOnly]
    public Vector2Int gridCoordinates;
    
    [SerializeField]
    public ETileType tileType;

    [SerializeField, ShowIf("tileType", ETileType.Normal)]
    public EGridObjectType objectType;

    [SerializeField]
    public List<GCellVisualPresetData> paintedVisuals;

    public void AddPaintedVisual(GCellVisualPresetData paintedVisual)
    {
        paintedVisuals.Add(paintedVisual);
    }
    
    public GCellData(Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = ETileType.Normal;
        objectType = EGridObjectType.None;
        paintedVisuals = new List<GCellVisualPresetData>();
    }
    

    public GCellData(GCellData inCellData,Vector2Int inGridCoordinates)
    {
        gridCoordinates = inGridCoordinates;
        tileType = inCellData.tileType;
        objectType = inCellData.objectType;
        paintedVisuals = inCellData.paintedVisuals;
    }

    public bool IsCellChanged()
    {
        return tileType != ETileType.Normal ||
               objectType != EGridObjectType.None ||
               (paintedVisuals != null && paintedVisuals.Count != 0);
    }
}

[System.Serializable]
public class GCellVisualPresetData
{
    public GameObject prefab;
    public Quaternion rotation;

    GCellVisualPresetData()
    {
    }

    public GCellVisualPresetData(GameObject inPrefab, Quaternion inRotation)
    {
        prefab = inPrefab;
        rotation = inRotation;
    }
}
