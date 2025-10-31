using Stanpac.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


// TODO : Create Wave Data In Scriptable Object ! 
// Creat this in Turn Base Manager ?
// Set the Data for Wave expose in the turn Base manager ?
// Remove Singleton From this Script 
/* Manage the Wave and Spawn of Ennemies */
public class GWaveManager : GSingleton<GWaveManager>
{
    private enum EWaveType
    {
        Finite,
        Endless
    }
    
    [Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("Enemy to spawn")]
        public GAIController enemy;

        [Min(0), Tooltip("Number of enemies to spawn")]
        public int count;
    }

    [Serializable]
    public class SWave
    {
        [Tooltip("At which turn the wave starts")]
        public int turn = 0;
        
        [SerializeField, Tooltip("Enemies to spawn in this wave")]
        private List<EnemySpawnEntry> enemiesToSpawn = new List<EnemySpawnEntry>();
        
        [NonSerialized, ReadOnly, Tooltip("Does the Wave Has been preview by the system (Just before the Player Turn)")] 
        public bool IsPreview = false; 
        
        public int GetSpawnCount()
        {
            int total = 0;
            for (int i = 0; i < enemiesToSpawn.Count; i++)
                total += Mathf.Max(0, enemiesToSpawn[i].count);
            return total;
        }
        
        public List<GAIController> GetEnemiesToSpawn()
        {
            var list = new List<GAIController>();
            for (int i = 0; i < enemiesToSpawn.Count; i++)
            {
                var e = enemiesToSpawn[i];
                if (e.enemy == null || e.count <= 0) continue;
                for (int k = 0; k < e.count; k++)
                    list.Add(e.enemy);
            }
            return list;
        }
    }
    
    [field: SerializeField, ShowIf("@_waveType == EWaveType.Finite"),Tooltip("List of waves to spawn")]
    public List<SWave> waves { get; private set; } = new List<SWave>();
    
    [SerializeField, Tooltip("Type of wave spawning"), PropertyOrder(-1)]
    private EWaveType _waveType = EWaveType.Finite;
    
    [SerializeField, ShowIf("@_waveType == EWaveType.Endless"), Tooltip("Enemy prefab for Endless wave spawning")]
    private GAIController enemyPrefab; 
    
    [HideInEditorMode, ReadOnly, Tooltip("Number of completed waves")]
    private int _WaveCount = 0;
    
    [HideInEditorMode, ReadOnly, Tooltip("Currrent Pool of Enemies to Spawn")]
    private List<GAIController> _ennemiesPool = new List<GAIController>();
    
    [HideInEditorMode, ReadOnly, Tooltip("List of potential spawn cells for enemies")]
    private List<GCell> _spawnCells = new List<GCell>();

    private int SpawningInProcess = 0; 
    
    /** Get the Current Score (Number of completed waves) */
    public int GetScore() => _waveType == EWaveType.Endless ? GetWaveCount() - 1: -1;
    
    public bool IsSpawningInProgress() => HasEnemiesToSpawn() && SpawningInProcess > 0;
    
    private void OnEnemySpawned()
    {
        SpawningInProcess = Mathf.Max(0, SpawningInProcess - 1);
        if (SpawningInProcess > 0) return;
        // TODO : Spawning process finish !
    }
    
    private void OnPrePlayerTurn(int turnCount)
    {
        CheckNextWave(turnCount);
    }
    
    private void OnPostPlayerTurn(int turnCount)
    {
        if (_waveType == EWaveType.Endless)
            CheckNextWave(turnCount);
       
        SpawnNextWave();
    }

    private void CheckNextWave(int turnCount)
    {
        if (HasEnemiesToSpawn()) return; // already have enemies to spawn ! 
        
        if (_waveType == EWaveType.Endless) // Endless wave logic
        {
            if (GTurnBaseManager.Instance.EnemiesCount > 0) return; // Wait until all enemies are dead !
            _ennemiesPool.AddRange(Enumerable.Repeat(enemyPrefab, _WaveCount + 1)); 
        }
        else if (_waveType == EWaveType.Finite) // Finite wave logic
        {
            if (waves == null || waves.Count == 0)  return;
            
            SWave wave = waves.FirstOrDefault();
            if (wave == null || turnCount < wave.turn) return;
            
            wave.IsPreview = true;
            _ennemiesPool.AddRange(wave.GetEnemiesToSpawn());
            waves.RemoveAt(0);
        }

        if (_ennemiesPool.Count > 0)
        {
            UpdateWaveCount();
            if (_ennemiesPool.Count < _spawnCells.Count)
                _spawnCells.Shuffle();
        }
        
        if (_waveType == EWaveType.Endless) return; // Do not preview for endless mode
        
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
        
        SpawningInProcess = 0;
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
            SpawningInProcess++;
            _spawnCells[i].SpawnPawnFinish(pawn, OnEnemySpawned);
            _ennemiesPool.RemoveAt(i);
        }
        
        if (_waveType == EWaveType.Endless) return; // Do not clear the pool for endless mode
        
        _ennemiesPool.Clear();    
    }
    
    private void UpdateWaveCount()
    {
        _WaveCount++;
        GGameManager.Instance.UpdateIntensity(GetWaveCount()); 
    }
    
    private bool HasEnemiesToSpawn() => _ennemiesPool.Count > 0;
    
    /* Get the Actual wave Count */
    private int GetWaveCount() => _waveType == EWaveType.Endless ? _WaveCount : -1;
    
    protected override void Awake()
    {
        base.Awake(); 
        _spawnCells = GGridManager.Instance.GetAllCellsOfType(ETileType.Spawner);
        GTurnBaseManager.Instance.OnPrePlayerTurn += OnPrePlayerTurn;
        GTurnBaseManager.Instance.OnPostPlayerTurn += OnPostPlayerTurn;
    }

    void OnDisable()
    {
        GGameManager.Instance.UpdateIntensity(0); // Reset Intensity
    }
}
