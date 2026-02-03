using Sirenix.OdinInspector;
using Stanpac.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Wave
{
    /* Scriptable Object to Store Wave Data */
    [CreateAssetMenu(fileName = "WaveData", menuName = "LeJeu/Wave/WaveData", order = 0)]
    public class WaveData : ScriptableObject
    {
        [Serializable]
        public class EnemyEntry
        {
            public GAIController enemyPrefab;

            [Tooltip("Cost of This Enemy for the Wave Budget")]
            public float Cost = 1f;
        }
        
        // [Tooltip("Is This Wave a Tutorial Wave ?")]
        // public bool bIsTutorialWave = false;
        
        [Tooltip("Do We Know Where the Enemies will Spawn ?")]
        public bool bShowPreviewSpawns = false;

        [Tooltip("Curve to Define the Wave Budget over Time (X = Wave Count, Y = Budget)")]
        // [HideIf("bIsTutorialWave")]
        public AnimationCurve WaveBudgetCurve;

        // [HideIf("bIsTutorialWave")]
        public List<EnemyEntry> EnemiesEntries = new List<EnemyEntry>();
        
        private int GetWaveBugdet(int waveCount)
        {
            return (int)WaveBudgetCurve.Evaluate(waveCount);
        }

        // Temporary Simple Logic to Get Enemies for the Wave 
        public void GetEnemiesForWave(int waveCount, int MaxEnemies, in List<GAIController> outEnemies)
        {
            int TotalBudget = GetWaveBugdet(waveCount);
            
            float currentBudget = 0f;
            
            List<EnemyEntry> shuffledEnemies = EnemiesEntries;
            shuffledEnemies.Shuffle();
            
            int SafeGuard = 0;
            while (outEnemies.Count < MaxEnemies && currentBudget < TotalBudget && SafeGuard < 100)
            {
                foreach (var enemyEntry in shuffledEnemies)
                {
                    if (currentBudget + enemyEntry.Cost <= TotalBudget)
                    {
                        outEnemies.Add(enemyEntry.enemyPrefab);
                        currentBudget += enemyEntry.Cost;
                    }
                }
                
                SafeGuard++;
            }
        }
    }
}
