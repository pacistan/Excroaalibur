using _Project.Scripts.Wave;
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
    private List<GAIController> _SpawningEnemiesQueue = new List<GAIController>();
    
    [HideInEditorMode, ReadOnly, Tooltip("List of potential spawn cells for enemies")]
    private List<GCell> _spawnCells = new List<GCell>();

    IntVariable _currentWave;
    private int _spawningInProcess = 0; 
    
    private bool bHasCheckedNextWave = false;
    
    /** Current Wave Data Use by the Wave Manager */
    private WaveData _waveData => GTurnBaseManager.Instance.GetCurrentWaveData();
    
    /* Get the Actual wave Count */
    public int GetWaveCount() => _waveCount;
    
    /* Has the Wave Manager created Next Wave this Turn */
    public bool HasCreateNextWave() => bHasCheckedNextWave;

    public void SetWaveCount(int waveCount) => _waveCount = waveCount;
    
    public bool IsSpawningInProgress() => HasEnemiesToSpawn() && _spawningInProcess > 0;
    
    public bool HasEnemiesToSpawn() => _SpawningEnemiesQueue.Count > 0;
    
    private void OnEnemySpawned()
    {
        _spawningInProcess = Mathf.Max(0, _spawningInProcess - 1);
        if (_spawningInProcess > 0) return;
        
        // TODO : Spawning process finish !
    }
    
    public void CheckNextWave(int turnCount)
    {
        if (HasEnemiesToSpawn()) return; // already have enemies to spawn ! 
        
        // TODO : Tutorial Wave logic ! 
        
        _waveData.GetEnemiesForWave(_waveCount, _spawnCells.Count, _SpawningEnemiesQueue);

        if (_SpawningEnemiesQueue.Count > 0)
        {
            UpdateWaveCount();
            
            // Ensure Spawn Cells are shuffled if we have less enemies than spawn points ! 
            if (_SpawningEnemiesQueue.Count < _spawnCells.Count)  
                _spawnCells.Shuffle();
        }

        if (_waveData.bShowPreviewSpawns) // Show Preview Spawns
        {
            int spawnable = Mathf.Min(_SpawningEnemiesQueue.Count, _spawnCells.Count);
            for (int i = 0; i < spawnable; i++)
            {
                _spawnCells[i].PreviewSpawnPawn();
            }
        }
        
        bHasCheckedNextWave = true;
    }

    /** Spawn the Enemies in _SpawningEnemiesQueue */
    public void SpawnNextWave()
    {
        if (!HasEnemiesToSpawn()) return;
        
        _spawningInProcess = 0;
        int spawnable = Mathf.Min(_SpawningEnemiesQueue.Count, _spawnCells.Count);
        Debug.Log($"[WaveManager] Spawning {spawnable} enemies.");
        GHudManager.Instance.playMenu.SetWaveNumberText(_waveCount);

        for (int i = spawnable - 1; i >= 0; i--)
        {
            var controllerPrefab = _SpawningEnemiesQueue[i];
            if (controllerPrefab == null)
            {
                Debug.LogError("Enemy prefab is null in WaveManager.");
                continue;
            }
            
            GController controller = Instantiate(controllerPrefab);
            GTurnBaseManager.Instance.RegisterController(controller);
            
            GPawn pawn = controller ? controller.GetComponent<GPawn>() : null;
            if (pawn == null)
            {
                Debug.LogWarning("[WaveManager] Spawned controller has no GPawn component.");
                continue;
            }
            _spawningInProcess++;
            StartCoroutine(_spawnCells[i].SpawnPawnFinish(pawn, OnEnemySpawned));
            _SpawningEnemiesQueue.RemoveAt(i);
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



