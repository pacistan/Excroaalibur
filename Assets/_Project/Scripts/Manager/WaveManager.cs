using System;
using System.Collections.Generic;
using UnityEngine;

/* Responsable de la gestion des vagues d'ennemis */
public class WaveManager : GSingleton<WaveManager>
{
    [Serializable]
    public struct SWave 
    {
        [Tooltip("At which turn the wave starts")]
        public int turn;
        
        [Tooltip("Enemies to spawn in this wave")]
        private Dictionary<GAIController, int> _enemiesToSpawn;
        
        public SWave(int inTurn)
        {
            turn = inTurn;
            _enemiesToSpawn = new Dictionary<GAIController, int>();
        }
    }
    
    [field: SerializeField, Tooltip("List of waves to spawn")]
    public List<SWave> _waves { get; private set; } = new List<SWave>();
    
    [field: SerializeField, Tooltip("List of potential spawn cells for enemies")]
    private List<GCell> _spawnCells = new List<GCell>();
    
    
    
}
