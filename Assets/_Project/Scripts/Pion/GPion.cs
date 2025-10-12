using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GPion : MonoBehaviour
{
    [ReadOnly]
    public GHexCoordinate coordinate;
    [SerializeField] public int moveDistance = 1;
    [SerializeReference] public List<GAction> actions = new List<GAction>();
    public bool isPlayer;

    int _stunTurn = 0;
    
    public void RequestAction(GAction action)
    {
        if (action == null || !action.IsValid()) return;
        
        //TODO Request action to the turn manager
    }
}