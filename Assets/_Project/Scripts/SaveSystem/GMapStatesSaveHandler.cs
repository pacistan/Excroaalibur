using Sirenix.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class GMapStatesSaveHandler : GSaveHandler<GMapStatesSaveData>
{
    [field : SerializeField]
    public GSOMapData[] _mapData { get; private set; }

    protected override GMapStatesSaveData GenerateSaveData()
    {
        GMapStatesSaveData saveData = new GMapStatesSaveData();
        saveData.mapScore = _mapData.Select(map => map.NumberOfWavesOnThisMap).ToArray();
        return saveData;
    }


    public void UpdateGameStateFromData(GMapStatesSaveData data)
    {
        _mapData.ForEach(map => map.NumberOfWavesOnThisMap = data.mapScore[Array.IndexOf(_mapData, map)]);
    }
}


[Serializable]
public class GMapStatesSaveData
{
    public int[] mapScore;
}
