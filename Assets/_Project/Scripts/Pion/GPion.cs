using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class GPion : MonoBehaviour
{
    [ReadOnly]
    public GHexCoordinate coordinate;
    [SerializeField] public int moveDistance = 1;
    [SerializeReference] public List<GAction> actions = new List<GAction>();
    
    public void RequestAction(GAction action)
    {
        if (action == null || !action.IsValid()) return;
        
        //TODO Request action to the turn manager
    }
}