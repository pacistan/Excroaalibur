using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/* Manage the Wave and Spawn of Ennemies */
public class WaveManager : GSingleton<WaveManager>
{
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
        
        [NonSerialized] 
        public bool hasSpawned = false; 
        
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
    
    [field: SerializeField, Tooltip("List of waves to spawn")]
    public List<SWave> waves { get; private set; } = new List<SWave>();
    
    [HideInEditorMode, ReadOnly, Tooltip("List of potential spawn cells for enemies")]
    private List<GCell> _spawnCells = new List<GCell>();
    
    private readonly List<GCell> _cachedNextSpawnCells = new List<GCell>();
    private SWave _cachedNextWave = null;
    
    public bool HasNextWave() => _cachedNextWave != null && _cachedNextSpawnCells.Count > 0;

    public void CheckNextWave(int turnCount)
    {
        if (HasNextWave()) return; // already have a wave cached
        
        for (int i = 0; i < waves.Count; i++)
        {
            var wave = waves[i];
            if (wave == null || wave.hasSpawned) continue;
            if (turnCount < wave.turn) continue;

            PrepareWave(wave);
            Debug.Log($"[WaveManager] Prepared wave at turn {wave.turn}");
            break; // Only one wave per turn
        }
    }
    
    private void PrepareWave(SWave wave)
    {
        _cachedNextWave = wave;
        _cachedNextSpawnCells.Clear();

        int spawnCount = Mathf.Min(wave.GetSpawnCount(), _spawnCells.Count);
        if (spawnCount <= 0) return;
        
        _spawnCells.Shuffle();
        
        for (int i = 0; i < spawnCount; i++)
            _cachedNextSpawnCells.Add(_spawnCells[i]);
    }

    public void SpawnNextWave()
    {
        if (waves.Count == 0) return;

        var wave = _cachedNextWave;
        var enemies = wave.GetEnemiesToSpawn();
        
        int spawnable = Mathf.Min(enemies.Count, _cachedNextSpawnCells.Count);
        if (spawnable <= 0)
        {
            Debug.LogWarning("[WaveManager] No spawn possible (no enemies or no cells).");
            CleanupCachedWave();
            return;
        }
        
        Debug.Log($"[WaveManager] Spawning wave (turn {wave.turn}) with {spawnable}/{enemies.Count} enemies.");

        for (int i = 0; i < spawnable; i++)
        {
            var controllerPrefab = enemies[i];
            if (controllerPrefab == null) continue;

            GController controller = Instantiate(controllerPrefab);
            GTurnBaseManager.Instance.RegisterController(controller);

            GPawn pawn = controller ? controller.GetComponent<GPawn>() : null;
            if (pawn == null)
            {
                Debug.LogWarning("[WaveManager] Spawned controller has no GPawn component.");
                continue;
            }

            // TODO: gérer anims/sons/VFX ici si besoin
            _cachedNextSpawnCells[i].RegisterGridObject(pawn);
            _cachedNextSpawnCells[i].UpdateGridObject();
        }

        wave.hasSpawned = true;
        CleanupCachedWave();
    }
    
    private void CleanupCachedWave()
    {
        _cachedNextWave = null;
        _cachedNextSpawnCells.Clear();
    }

    protected override void Awake()
    {
        base.Awake(); 
        _spawnCells = GGridManager.Instance.GetAllCellsOfType(ETileType.Spawner);
    }
}
