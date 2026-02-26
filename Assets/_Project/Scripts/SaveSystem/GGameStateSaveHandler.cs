using System;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GGameStateSaveHandler : GSaveHandler<GGameStateSaveData>
{
    [SerializeField]
    GCommonInstantiationData _commonInstantiationData;

    public GGameStateSaveHandler(GCommonInstantiationData commonInstantiationData)
    { 
        _commonInstantiationData = commonInstantiationData;
    }

    protected override GGameStateSaveData GenerateSaveData()
    {
        GGameStateSaveData saveData = new GGameStateSaveData();

        GSOMapData mapData = GGameManager.Instance.loadableMapData;
        saveData.mapIndex = GSaveManager.Instance.GetMapIndex(mapData);

        saveData.waveNumber = GTurnBaseManager.Instance.GetScore();

        var ennemies = GGridObjectRegistry.GetItemsByPredicate<GPawn>(pawn => !pawn.data.isPlayer);
        var players = GGridObjectRegistry.GetItemsByPredicate<GPawn>(pawn => pawn.data.isPlayer);

        saveData.ennemies = new GEnnemySaveData[ennemies.Count()];
        saveData.players = new GPlayerSaveData[players.Count()];

        for (var i = 0; i < saveData.ennemies.Length; i++)
        {
            var enemyData = new GEnnemySaveData();
            var enemy = ennemies.ElementAt(i);
            enemyData.ennemyType = enemy.data.gridObjectType.ToString();
            enemyData.xCoordinate = enemy.GetCell().data.gridCoordinates.x;
            enemyData.yCoordinate = enemy.GetCell().data.gridCoordinates.y;
            enemyData.upgradeGuids = enemy.upgrades.Select(upgrade => upgrade.ItemID).ToArray();
            enemyData.stunTurnNumber = enemy.stunTurns;

            enemyData.hp = enemy.hp;
            saveData.ennemies[i] = enemyData;
        }
        for (var i = 0; i < saveData.players.Length; i++)
        {
            var playerData = new GPlayerSaveData();
            var player = players.ElementAt(i);
            playerData.xCoordinate = player.GetCell().data.gridCoordinates.x;
            playerData.yCoordinate = player.GetCell().data.gridCoordinates.y;
            playerData.stunTurnNumber = player.stunTurns;
            playerData.upgradeGuids = player.upgrades.Select(upgrade => upgrade.ItemID).ToArray();
            saveData.players[i] = playerData;
        }

        saveData.waveEnemyCapReach = GTurnBaseManager.Instance.GetCurrentWaveEnemyCapReached();

        GCrown crown = GGridObjectRegistry.GetItems<GCrown>()[0];
        saveData.xCrownCoordinate = crown.GetCell().data.gridCoordinates.x;
        saveData.yCrownCoordinate = crown.GetCell().data.gridCoordinates.y;
        saveData.crownDamage = crown.currentDamage;
        saveData.isFirstAction = GameObject.FindFirstObjectByType<GPlayerController>().isFirstAction;
        return saveData;
    }
}

[Serializable]
public class GGameStateSaveData
{
    public int mapIndex;
    public int waveNumber;
    public int waveEnemyCapReach;
    public GEnnemySaveData[] ennemies;
    public GPlayerSaveData[] players;
    public int xCrownCoordinate, yCrownCoordinate;
    public int crownDamage;
    public bool isFirstAction;
}

[Serializable]
public class GEnnemySaveData
{
    public string ennemyType;
    public int xCoordinate, yCoordinate;
    public int stunTurnNumber;
    public int hp;
    public string[] upgradeGuids;
}

[Serializable]
public class GPlayerSaveData
{
    public int xCoordinate, yCoordinate;
    public int stunTurnNumber;
    public string[] upgradeGuids;
}