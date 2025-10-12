
using Sirenix.Reflection.Editor;
using System.Collections.Generic;
using UnityEngine;

public class GTurnBaseManager
{
    // Manager The Turn Order 
    public GameObject _currentTurnEntity { get; private set; }
    public bool isTurnActive { get; private set; }
    public bool isActionAuthorized { get; private set; }
    
    
    // Queue Order of The Entity
    private Queue<GameObject> _turnOrderEntityQueue = new Queue<GameObject>();
    
    public void Start() 
    { 
        _turnOrderEntityQueue.Clear();
        // TODO : Initialize the Queue with the Entity in the Game
        // TODO : Order the Queue based on the Param
    }

    public void Update()
    {
        if (!isTurnActive) return;
        
    }
    
    public void End()
    {
        // TODO : Clean the Queue 
        _turnOrderEntityQueue.Clear();
    }
    
    public void RequestEndTurn()
    {
        isTurnActive = false;
        // TODO : End the Turn of the Current Player and Start the Next Player Turn
    }
}

