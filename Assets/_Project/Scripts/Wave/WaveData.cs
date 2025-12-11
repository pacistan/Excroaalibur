using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Wave
{
    /* Scriptable Object to Store Wave Data */
    [CreateAssetMenu(fileName = "WaveData", menuName = "LeJeu/Wave", order = 0)]
    
    public class WaveData : ScriptableObject
    {
        [Serializable]
        public enum EWaveType
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

        [Serializable] //Je tente des trucs
        public struct EndlessEnemyEntry
        {
            public GAIController enemyPrefab;
            [Range(0, 1)] public float spawnWeight; // probabilité
        }

        public List<EndlessEnemyEntry> endlessEnemies;

        [Serializable]
        public class SWave
        {
            [Tooltip("At which turn the wave starts")]
            public int turn = 0;
        
            [SerializeField, Tooltip("Enemies to spawn in this wave")]
            private List<EnemySpawnEntry> enemiesToSpawn = new List<EnemySpawnEntry>();
        
            [NonSerialized, Unity.Collections.ReadOnly, Tooltip("Does the Wave Has been preview by the system (Just before the Player Turn)")] 
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
    
        [Tooltip("Type of wave spawning"), PropertyOrder(-1)]
        public EWaveType _waveType = EWaveType.Finite;
    
        [ShowIf("@_waveType == EWaveType.Endless"), Tooltip("Enemy prefab for Endless wave spawning")]
        public GAIController enemyPrefab; 
    }
}
