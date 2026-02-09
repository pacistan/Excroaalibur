using Sirenix.OdinInspector;
using Stanpac.Utilities;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


/* Scriptable Object to Store Wave Data */
[CreateAssetMenu(fileName = "WaveData", menuName = "LeJeu/Wave/WaveData", order = 0)]
public class GWaveData : ScriptableObject
{
    [Serializable]
    public class EnemyEntry
    {
        [field: SerializeField] 
        public GAIController enemyPrefab { get; private set; }

        [Range(0, 1)]
        [Tooltip("Weigh of this Enemy in the Wave Generation Process (Higher = More Chance to be Spawn)")]
        public float Weigh = 1f;
        
        [SerializeField, ReadOnly, Tooltip("Probability of this Enemy to be choose (Calculated from the Weigh)")]
        public float Probability = 0f;
    }
    
    [Serializable]
    public class BuffEntry
    {
        [SerializeField]
        private String Name;
        
        [field: SerializeField] 
        public GSOUpgrade Upgrades { get; private set; }

        [Min(0)]
        [Tooltip("How many Buffs there is in the deck")]
        public float Count = 1f;
        
        [SerializeField, ReadOnly, Tooltip("Percentage of this buffs in the deck"), ]
        public float Percentage = 0f;
    }

    [Serializable]
    public enum ERounded
    {
        RoundUp,
        RoundDown,
    }
    
    // [Tooltip("Is This Wave a Tutorial Wave ?")]
    // public bool bIsTutorialWave = false;
    
    [Tooltip("Do We Know Where the Enemies will Spawn ?")]
    public bool bShowPreviewSpawns = false;

    [Range(0, 1), BoxGroup("ClassicWave"), Tooltip("Probability that the Wave is just one type of Enemy (Only for Non Tutorial Wave)")]
    public float SameEnemyProbability = 0.15f;
        
    [BoxGroup("ClassicWave")]
    public ERounded EnemiesRounded = ERounded.RoundUp;
    
    [BoxGroup("ClassicWave"), Tooltip("Multiplier applied to calculate How many Enemies to Spawn, the result is rounded with EnemiesRounded")]
    public float EnemiesCountMultiplier = 0.5f;
    
    // [HideIf("bIsTutorialWave")]
    [SerializeField, BoxGroup("ClassicWave"), Tooltip("Enemies that will be Spawn during the Wave")]
    private List<EnemyEntry> EnemiesEntries = new List<EnemyEntry>();
    
    // [HideIf("bIsTutorialWave")]
    [Min(1), BoxGroup("BossWave"), Tooltip("Modulo of the Wave Count to Spawn Boss Enemies (Ex: If 5, a Boss will be Spawn every 5 Waves)")]
    public int WaveCountForBoss = 5;
    
    // [HideIf("bIsTutorialWave")]
    [BoxGroup("BossWave"), Tooltip("Multiplier applied to calculate How many Boss to Spawn, the result is rounded up to the nearest integer")]
    public float BossCountMultiplier = 0.5f;
    
    // [HideIf("bIsTutorialWave")]
    [BoxGroup("BossWave"), Tooltip("Multiplier applied to Calculate how much buffs the Boss will have, the result is rounded up to the nearest integer")]
    public float BossBuffMultiplier = 0.5f;
    
    // [HideIf("bIsTutorialWave")]
    [SerializeField, BoxGroup("BossWave"), Tooltip("Boss Enemies that will be Spawn each X Waves (Defined by the WaveCountForBoss)")]
    private List<EnemyEntry> BossEntries = new List<EnemyEntry>();
    
    // [HideIf("bIsTutorialWave")]
    [SerializeField, BoxGroup("Buffs"), Tooltip("Buffs that can be applied to Enemies")]
    private List<BuffEntry> BuffsEntries = new List<BuffEntry>();
    
    // [HideIf("bIsTutorialWave")]
    [BoxGroup("Buffs"), Tooltip("Multiplier applied to Calculate how much buffs the Enemies will have")]
    public float BuffMultiplier = 0.5f;
    
    // [HideIf("bIsTutorialWave")]
    [BoxGroup("Buffs")]
    public ERounded BuffRounded = ERounded.RoundUp;
    
    public WeightedSelection<EnemyEntry> _EnemiesWeightedSelection;
    public WeightedSelection<EnemyEntry> _BossWeightedSelection;
    public WeightedSelection<BuffEntry> _BuffsWeightedSelection;

    [Button("Generate Weighted Selections", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f), HideInEditorMode]
    public void GenerateWeightedSelection()
    {
        _EnemiesWeightedSelection = new WeightedSelection<EnemyEntry>();
        foreach (var enemyEntry in EnemiesEntries)
        {
            _EnemiesWeightedSelection.AddChoice(enemyEntry, enemyEntry.Weigh);
        }
        
        _BossWeightedSelection = new WeightedSelection<EnemyEntry>();
        foreach (var enemyEntry in BossEntries)
        {
            _BossWeightedSelection.AddChoice(enemyEntry, enemyEntry.Weigh);
        }
        
        _BuffsWeightedSelection = new WeightedSelection<BuffEntry>();
        foreach (var buffEntry in BuffsEntries)
        {
            _BuffsWeightedSelection.AddChoice(buffEntry, buffEntry.Count);
        }
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        // EnemyEntry Probability Calculation
        float totalWeigh = 0f;
        foreach (var enemyEntry in EnemiesEntries)
        {
            totalWeigh += enemyEntry.Weigh;
        }
        
        foreach (var enemyEntry in EnemiesEntries)
        {
            enemyEntry.Probability = totalWeigh > 0 ? enemyEntry.Weigh / totalWeigh * 100 : 0f;
        }
        
        // Boss enemyEntry Probability Calculation
        totalWeigh = 0f;
        foreach (var enemyEntry in BossEntries)
        {
            totalWeigh += enemyEntry.Weigh;
        }

        foreach (var enemyEntry in BossEntries)
        {
            enemyEntry.Probability = totalWeigh > 0 ? enemyEntry.Weigh / totalWeigh * 100 : 0f;
        }
        
        // BuffEntry Percentage Calculation
        float totalCount = 0f;
        foreach (var buffEntry in BuffsEntries)
        {
            totalCount += buffEntry.Count;
        }
         
        foreach (var buffEntry in BuffsEntries)
        {
            buffEntry.Percentage = totalCount > 0 ? buffEntry.Count / totalCount * 100 : 0f;
        }
    }
#endif
}
