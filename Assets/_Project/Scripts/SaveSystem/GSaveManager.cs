using Sirenix.Utilities;
using System;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.Windows;
using UnityEditor;
using File = System.IO.File;


[DefaultExecutionOrder(100)]
public class GSaveManager : GSingleton<GSaveManager>
{
    [SerializeField]
    string _saveFileName;

    [SerializeField]
    GCommonInstantiationData _commonInstantiationData;
    
    public bool IsSaveFileCreated() => File.Exists(Application.streamingAssetsPath + "/" + _saveFileName + ".json");
    
    [ContextMenu("Save")]
    public void SerializeToJson()
    {
        string path = Application.streamingAssetsPath + "/" + _saveFileName + ".json";
        string jsonData = JsonUtility.ToJson(GenerateSaveData(), false);
        File.WriteAllText(path, jsonData);
    }

    public void DeleteSaveFile()
    {
        File.Delete(Application.streamingAssetsPath + "/" + _saveFileName + ".json");
    }
    
    private GGameStateSaveData DeserializeFromJson()
    {
        string jsonData = File.ReadAllText(Application.streamingAssetsPath + "/" + _saveFileName + ".json");
        return JsonUtility.FromJson<GGameStateSaveData>(jsonData);
    }

    private GGameStateSaveData GenerateSaveData()
    {
        GGameStateSaveData saveData = new GGameStateSaveData();
        saveData.waveNumber = GTurnBaseManager.Instance.GetScore();

        var ennemies = GGridObjectRegistry.GetItemsByPredicate<GPawn>(pawn => !pawn.data.isPlayer);
        var players = GGridObjectRegistry.GetItemsByPredicate<GPawn>(pawn => pawn.data.isPlayer);

        saveData.ennemies = new GEnnemySaveData[ennemies.Count()];
        saveData.players = new GPlayerSaveData[players.Count()];
        
        for (var i = 0; i < saveData.ennemies.Length; i++)
        {
            var ennemyData = new GEnnemySaveData();
            var ennemy = ennemies.ElementAt(i);
            ennemyData.ennemyType = ennemy.data.gridObjectType.ToString();
            ennemyData.xCoordinate = ennemy.GetCell().data.gridCoordinates.x;
            ennemyData.yCoordinate = ennemy.GetCell().data.gridCoordinates.y;
            ennemyData.isStunned = ennemy.isStunned;

            ennemyData.hp = ennemy.hp;
            saveData.ennemies[i] = ennemyData;
        }
        for (var i = 0; i < saveData.players.Length; i++)
        {
            var playerData = new GPlayerSaveData();
            var player = players.ElementAt(i);
            playerData.xCoordinate = player.GetCell().data.gridCoordinates.x;
            playerData.yCoordinate = player.GetCell().data.gridCoordinates.y;
            playerData.isStunned = player.isStunned;

            saveData.players[i] = playerData;
        }
        GCrown crown = GGridObjectRegistry.GetItems<GCrown>()[0];
        saveData.xCrownCoordinate = crown.GetCell().data.gridCoordinates.x;
        saveData.yCrownCoordinate = crown.GetCell().data.gridCoordinates.y;
        saveData.crownDamage = crown.currentDamage;
        saveData.isFirstAction = FindFirstObjectByType<GPlayerController>().isFirstAction;
        return saveData;
    }

    private void OnStateChange(EMacroStates currentState, EMacroStates previousState)
    {
        if (previousState == EMacroStates.LoadingScreen && currentState == EMacroStates.Play &&
            !GGameManager.Instance.isLoadingNewSave)
        {
            var data = DeserializeFromJson();
            CreateGameStateFromData(data);
        }

        if (currentState == EMacroStates.End)
        {
            DeleteSaveFile();
        }

    }

    private void CreateGameStateFromData(GGameStateSaveData data)
    {
        GCrown crown = GGridObjectRegistry.GetItems<GCrown>()[0];
        GPawn[] players = GGridObjectRegistry.GetItemsByPredicate<GPawn>(pawn => pawn.data.isPlayer).ToArray();


        // Player Stuff
        {
            for (var i = 0; i < players.Length; i++)
            {
                GPawn player = players.ElementAt(i);
                var playerData = data.players.ElementAt(i);

                if(playerData.isStunned) player.Stun();

                GCell playerCell = GGridManager.Instance.GetCell(new Vector2Int(playerData.xCoordinate, playerData.yCoordinate));
                if (playerCell != player.GetCell())
                {
                    playerCell.SetGridObject(player, true);
                }
            }
        }
        
        // Ennemy Stuff
        {
            for (int i = 0; i < data.ennemies.Length; i++)
            {
                var ennemyData = data.ennemies.ElementAt(i);
                EGridObjectType objectType = (EGridObjectType)Enum.Parse(typeof(EGridObjectType), ennemyData.ennemyType);
                GGridObject gridObjectPrefab = _commonInstantiationData.objectTypeData[objectType];
                GPawn pawnPrefab =  gridObjectPrefab as GPawn;
                if (pawnPrefab == null)
                {
                    Debug.LogError($"Could not find pawn for type {ennemyData.ennemyType}");
                    continue;
                }

                GPawn pawn = Instantiate(pawnPrefab);
                GCell ennemyCell = GGridManager.Instance.GetCell(new Vector2Int(ennemyData.xCoordinate, ennemyData.yCoordinate));
                ennemyCell.SetGridObject(pawn, true);
                if(ennemyData.isStunned) pawn.Stun();

                pawn.SetHp(ennemyData.hp);
            }
        }
        
        // Crown Stuff
        {
            for (int i = crown.currentDamage; i <= data.crownDamage; i++) crown.OnPass();
            GHudManager.Instance.playMenu.SetCrownDamageText(crown.currentDamage);

            GCell crownCell =
                GGridManager.Instance.GetCell(new Vector2Int(data.xCrownCoordinate, data.yCrownCoordinate));
            GPawn crownPawn = crownCell.gridObject as GPawn;

            if (crown.owner != null)
                crown.owner.ReleaseEquipement(false, false);

            if (crownPawn)
                crownPawn.GiveEquipement(crown, true, false);
            else
                crownCell.SetGridObject(crown);
        }
        GPlayerController playerController = FindFirstObjectByType<GPlayerController>();
        playerController.isFirstAction = data.isFirstAction;
        GTurnBaseManager.Instance.SetWaveCount(data.waveNumber);
        GHudManager.Instance.playMenu.SetWaveNumberText(data.waveNumber);
    }
    
    void OnEnable()
    {
        GGameManager.Instance.OnChangeMacroStateEvent += OnStateChange;
    }

    void OnDisable()
    {
        GGameManager.Instance.OnChangeMacroStateEvent -= OnStateChange;
    }
}

[Serializable]
public class GGameStateSaveData
{
    public int waveNumber;
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
    public bool isStunned;
    public int hp;
}

[Serializable]
public class GPlayerSaveData
{
    public int xCoordinate, yCoordinate;
    public bool isStunned;
}

