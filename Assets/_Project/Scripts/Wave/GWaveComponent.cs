using _Project.Scripts.Wave;
using Sirenix.OdinInspector;
using Stanpac.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/* Manage the Wave and Spawn of Ennemies */
public class GWaveComponent : MonoBehaviour 
{
    public Action<bool> OnSpawningProcessFinsish;
    
    [HideInEditorMode, ReadOnly, Tooltip("Number of completed waves")]
    private int _WaveCount = 0;
    
    [HideInEditorMode, ReadOnly, Tooltip("Currrent Pool of Enemies to Spawn")]
    private List<GAIController> _ennemiesPool = new List<GAIController>();
    
    [HideInEditorMode, ReadOnly, Tooltip("List of potential spawn cells for enemies")]
    private List<GCell> _spawnCells = new List<GCell>();

    private int _spawningInProcess = 0; 
    
    /** Current Wave Data Use by the Wave Manager */
    private WaveData _waveData;
    
    /* Get the Actual wave Count */
    public int GetWaveCount() => _waveData._waveType == WaveData.EWaveType.Endless ? _WaveCount : -1;

    public bool HasWave() => _waveData != null &&
                             (_waveData._waveType == WaveData.EWaveType.Endless ||
                              (_waveData.waves != null && _waveData.waves.Count > 0));
    public bool IsSpawningInProgress() => HasEnemiesToSpawn() || _spawningInProcess > 0;
    
    private bool HasEnemiesToSpawn() => _ennemiesPool.Count > 0;
    
    private void OnEnemySpawned()
    {
        _spawningInProcess = Mathf.Max(0, _spawningInProcess - 1);
        if (_spawningInProcess > 0) return;
        
        // TODO : Spawning process finish !
    }
    
    private void OnPrePlayerTurn(int turnCount)
    {
        CheckNextWave(turnCount);
    }
    
    private void OnPostPlayerTurn(int turnCount)
    {
        if (_waveData!= null && _waveData._waveType == WaveData.EWaveType.Endless)
            CheckNextWave(turnCount);
       
        SpawnNextWave();
    }

    private void CheckNextWave(int turnCount)
    {
        if (!HasWave()) return; // No wave to process !
        
        if (HasEnemiesToSpawn()) return; // already have enemies to spawn ! 
        
        if (_waveData._waveType == WaveData.EWaveType.Endless) // Endless wave logic
        {
            if (GTurnBaseManager.Instance.EnemiesCount > 0) return; // Wait until all enemies are dead !
            _ennemiesPool.AddRange(Enumerable.Repeat(_waveData.enemyPrefab, _WaveCount + 1)); 
        }
        else if (_waveData._waveType == WaveData.EWaveType.Finite) // Finite wave logic
        {
            if (_waveData.waves == null || _waveData.waves.Count == 0)  return;
            
            WaveData.SWave wave = _waveData.waves.FirstOrDefault();
            if (wave == null || turnCount < wave.turn) return;
            
            wave.IsPreview = true;
            _ennemiesPool.AddRange(wave.GetEnemiesToSpawn());
            _waveData.waves.RemoveAt(0);
        }

        if (_ennemiesPool.Count > 0)
        {
            UpdateWaveCount();
            if (_ennemiesPool.Count < _spawnCells.Count)
                _spawnCells.Shuffle();
        }
        
        if (_waveData._waveType == WaveData.EWaveType.Endless) return; // Do not preview for endless mode
        
        // Preview Spawn
        int spawnable = Mathf.Min(_ennemiesPool.Count, _spawnCells.Count);
        for (int i = 0; i < spawnable; i++)
        {
            _spawnCells[i].PreviewSpawnPawn();
        }
    }

    private void SpawnNextWave()
    {
        if (!HasEnemiesToSpawn()) return;
        
        _spawningInProcess = 0;
        int spawnable = Mathf.Min(_ennemiesPool.Count, _spawnCells.Count);
        Debug.Log($"[WaveManager] Spawning {spawnable} enemies.");
        GHudManager.Instance.playMenu.SetWaveNumberText(_WaveCount);

        for (int i = spawnable - 1; i >= 0; i--)
        {
            var controllerPrefab = _ennemiesPool[i];
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
            _ennemiesPool.RemoveAt(i);
        }
        
        if (_waveData._waveType == WaveData.EWaveType.Endless) return; // Do not clear the pool for endless mode
        
        _ennemiesPool.Clear();    
    }
    
    private void UpdateWaveCount()
    {
        _WaveCount++;
        GGameManager.Instance.UpdateIntensity(GetWaveCount()); 
    }
    
    protected void Awake()
    {
        _spawnCells = GGridManager.Instance.GetAllCellsOfType(ETileType.Spawner);
        GTurnBaseManager.Instance.OnPrePlayerTurn += OnPrePlayerTurn;
        GTurnBaseManager.Instance.OnPostPlayerTurn += OnPostPlayerTurn;
        WaveData tempData = GTurnBaseManager.Instance.GetCurrentWaveData();
        if (tempData != null)
            _waveData = Instantiate(tempData);
        // _waveData = GTurnBaseManager.Instance.GetCurrentWaveData().;
    }

    void OnDisable()
    {
        GGameManager.Instance.UpdateIntensity(0); // Reset Intensity
    }
}
