using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Enum defini avec prefix E
public enum EGameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

// Struct defini avec prefix S
public struct SPlayerStats
{
    public int health;
    public float speed;
    public bool isInvulnerable;
}

// Classe definie avec prefix G
public class GPlayer : MonoBehaviour
{
    // Variables publiques
    public int score;
    public float playerSpeed = 5f;
    
    // Variables privées avec prefix _
    private int _health;
    private bool _isAlive;
    private Vector3 _lastPosition;
    
    // Propriété raccourcie (getter/setter)
    public int Health { get { return _health; } private set { _health = value; } }
    
    // Méthodes personnalisées avec Majuscule
    public void UpdateScore(int points)
    {
    }
    
    private void TakeDamage(int amount)
    {
	      if (_isAlive == true) {
              _isAlive = false;
              _health = 3;
          }
          else 
          { 
              _health = 2;
              return;
          }
    }
    
    // Méthodes Unity à la fin
    private void Start()
    {
    }
    
    private void Update()
    {
    }
    
    // Méthode Coroutine en bas
    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(2f);
        _isAlive = true;
    }
}