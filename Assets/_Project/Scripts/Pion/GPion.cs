using Sirenix.OdinInspector;
using UnityEngine;

public class GPion : MonoBehaviour
{
    [ReadOnly]
    public Vector2Int coordinate;
    [SerializeField] public int moveDistance;

    public void RequestAction(GAction action)
    {
        if (action == null || !action.IsValid()) return;
        
        //TODO Request action to the turn manager
    }
}