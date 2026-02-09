using Sirenix.OdinInspector;
using Stanpac.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

/* Manage the Wave and Spawn of Ennemies */
public class GWaveComponent : MonoBehaviour 
{
    public Action<bool> OnSpawningProcessFinsish;
    
    [HideInEditorMode, ReadOnly, Tooltip("Number of completed waves")]
    private int _waveCount = 0;
    
    [HideInEditorMode, ReadOnly, Tooltip("Currrent Queue of Ennemies to Spawn")]
    private List<GAIController> _spawningEnemiesQueue = new List<GAIController>();
    
    [HideInEditorMode, ReadOnly, Tooltip("List of upgrade to apply to next Enemies")]
    private List<GSOUpgrade> _upgradeEntriesQueue = new List<GSOUpgrade>();
    
    [HideInEditorMode, ReadOnly, Tooltip("List of potential spawn cells for enemies")]
    private List<GCell> _spawnCells = new List<GCell>();
    
    private int _spawningInProcess = 0; 
    private bool bHasCheckedNextWave = false;
    
    // When true, the Wave Manager can start Buffing Enemies 
    private int _waveEnemyCapReach = 0;
    
    /** Current Wave Data Use by the Wave Manager */
    private GWaveData _waveData => GTurnBaseManager.Instance.GetCurrentWaveData();
    
    /* Get the Actual wave Count */
    public int GetWaveCount() => _waveCount;
    
    /* Has the Wave Manager created Next Wave this Turn */
    public bool HasCreateNextWave() => bHasCheckedNextWave;
    
    public void SetWaveCount(int waveCount) => _waveCount = waveCount;
    
    public bool IsSpawningInProgress() => HasEnemiesToSpawn() && _spawningInProcess > 0;
    
    public bool HasEnemiesToSpawn() => _spawningEnemiesQueue.Count > 0;
    
    private void OnEnemySpawned()
    {
        _spawningInProcess = Mathf.Max(0, _spawningInProcess - 1);
        if (_spawningInProcess > 0) return;
        
        // Spawning process finish !
    }
    
    public void CheckNextWave(int turnCount)
    {
        if (HasEnemiesToSpawn()) return; // already have enemies to spawn ! 
        
        // TODO : Tutorial Wave logic ! 
        
        GenerateWave(_waveCount + 1, _spawnCells.Count);

        if (_spawningEnemiesQueue.Count > 0)
        {
            // Ensure Spawn Cells are shuffled if we have less enemies than spawn points ! 
            if (_spawningEnemiesQueue.Count < _spawnCells.Count)  
                _spawnCells.Shuffle();
            else if (_waveEnemyCapReach == 0) // We have reach the cap of enemies to spawn, we can start buffing them !
                _waveEnemyCapReach = _waveCount + 1; 
        }

        if (_waveData.bShowPreviewSpawns) // Show Preview Spawns
        {
            int spawnable = Mathf.Min(_spawningEnemiesQueue.Count, _spawnCells.Count);
            for (int i = 0; i < spawnable; i++)
            {
                _spawnCells[i].PreviewSpawnPawn();
            }
        }
        
        bHasCheckedNextWave = true;
    }
    
    public void GenerateWave(int WaveIndex, int MaxSpawnCellsCount)
    {
        if (WaveIndex % _waveData.WaveCountForBoss == 0)  // Boss Wave
        {
            int bossCount = Mathf.CeilToInt(WaveIndex / _waveData.WaveCountForBoss * _waveData.BossCountMultiplier);
            GenerateEnemies(bossCount, false, _waveData._BossWeightedSelection);
            
            int buffsCount = Mathf.CeilToInt(WaveIndex / _waveData.WaveCountForBoss * _waveData.BuffMultiplier);
            GenerateBuffs(buffsCount);
        }
        else  // Classic Wave
        {
            bool bSpawnSameEnemy = UnityEngine.Random.value < _waveData.SameEnemyProbability;
            int enemyCount = _waveData.EnemiesRounded == GWaveData.ERounded.RoundUp 
                ? Mathf.CeilToInt(WaveIndex * _waveData.EnemiesCountMultiplier) 
                : Mathf.FloorToInt(WaveIndex * _waveData.EnemiesCountMultiplier);
            
            GenerateEnemies(enemyCount, bSpawnSameEnemy, _waveData._EnemiesWeightedSelection);

            if (_waveEnemyCapReach != 0)
            {
                int buffsCount = _waveData.BuffRounded == GWaveData.ERounded.RoundUp 
                    ? Mathf.CeilToInt((WaveIndex - _waveEnemyCapReach) * _waveData.BuffMultiplier) 
                    : Mathf.FloorToInt((WaveIndex - _waveEnemyCapReach) * _waveData.BuffMultiplier);
                GenerateBuffs(buffsCount);
            }
        }
    }

    private void GenerateBuffs(int buffsCount)
    {
        int[] AlreadyAddedBuffs = Array.Empty<int>();
        for (int i = 0; i < buffsCount; i++)
        {
            if (AlreadyAddedBuffs.Length == _waveData._BuffsWeightedSelection.Count)  // Already have all the buffs, Reset the Pool of Buffs
                AlreadyAddedBuffs = Array.Empty<int>(); 
            
            int buffIndex = _waveData._BuffsWeightedSelection.SelectChoiceIndex(UnityEngine.Random.value, AlreadyAddedBuffs);
            var buffEntry = _waveData._BuffsWeightedSelection.GetChoice(buffIndex).item;
            _upgradeEntriesQueue.AddRange(buffEntry.Upgrades);
            
            // Add the selected buff index to the list of already added buffs !
            int index = AlreadyAddedBuffs.Length;
            Array.Resize<int>(ref AlreadyAddedBuffs, index + 1);
            AlreadyAddedBuffs[index] = buffIndex;
        }
    }

    private void GenerateEnemies(int enemyCount, bool bSpawnSameEnemy, in WeightedSelection<GWaveData.EnemyEntry> enemiesWeightedSelection)
    {
        if (bSpawnSameEnemy)
        {
            var enemyEntry = enemiesWeightedSelection.Select(UnityEngine.Random.value);
            for (int i = 0; i < enemyCount; i++)
            {
                _spawningEnemiesQueue.Add(enemyEntry.enemyPrefab);
            }
        }
        else
        {
            for (int i = 0; i < enemyCount; i++)
            {
                var enemyEntry = enemiesWeightedSelection.Select(UnityEngine.Random.value);
                _spawningEnemiesQueue.Add(enemyEntry.enemyPrefab);
            }
        }
    }

    /** Spawn the Enemies in _SpawningEnemiesQueue */
    public void SpawnNextWave()
    {
        if (!HasEnemiesToSpawn()) return;
        
        UpdateWaveCount();
        
        _spawningInProcess = 0;
        int spawnable = Mathf.Min(_spawningEnemiesQueue.Count, _spawnCells.Count);
        Debug.Log($"[WaveManager] Spawning {spawnable} enemies.");
        GHudManager.Instance.playMenu.SetWaveNumberText(_waveCount);
        
        int upgradePerEnemy = Math.DivRem(_upgradeEntriesQueue.Count, spawnable, out int remainder);
        for (int i = spawnable - 1; i >= 0; i--)
        {
            var controllerPrefab = _spawningEnemiesQueue[i];
            if (controllerPrefab == null)
            {
                Debug.LogError("Enemy prefab is null in WaveManager.");
                continue;
            }

            GAIController controller = Instantiate(controllerPrefab);
            GTurnBaseManager.Instance.RegisterController(controller);
            
            for (int j = 0; j < upgradePerEnemy; j++)
            {
                if (_upgradeEntriesQueue.Count == 0) break;
                GSOUpgrade upgrade = _upgradeEntriesQueue[0].CreateInstance();
                _upgradeEntriesQueue.RemoveAt(0);
                controller.pawn.AddUpgrade(upgrade);
            }

            if (remainder != 0)
            {
                GSOUpgrade upgrade = _upgradeEntriesQueue[0].CreateInstance();
                _upgradeEntriesQueue.RemoveAt(0);
                controller.pawn.AddUpgrade(upgrade);
                remainder--;
            }
            
            GPawn pawn = controller ? controller.GetComponent<GPawn>() : null;
            if (pawn == null)
            {
                Debug.LogWarning("[WaveManager] Spawned controller has no GPawn component.");
                continue;
            }
            _spawningInProcess++;
            StartCoroutine(_spawnCells[i].SpawnPawnFinish(pawn, OnEnemySpawned));
            _spawningEnemiesQueue.RemoveAt(i);
        }
    }

    private void UpdateWaveCount()
    {
        _waveCount++;
        GGameManager.Instance.UpdateIntensity(_waveCount); 
    }
    
    protected void Awake()
    {
        _spawnCells = GGridManager.Instance.GetAllCellsOfType(ETileType.Spawner);
        GTurnBaseManager.Instance.OnStartControllerTurn += HandleStartControllerTurn;
    }
    
    void HandleStartControllerTurn(GController controller)
    {
        bHasCheckedNextWave = false;
    }

    void OnDisable()
    {
        GGameManager.Instance.UpdateIntensity(0); // Reset Intensity
    }
}



