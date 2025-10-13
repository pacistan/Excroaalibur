using UnityEngine;

public interface GIController
{
    public int actionTokens { get; set; }
    
    public void StartTurn();

    public void StartAction();
    
    public void OnActionOver();
    
    public void EndTurn();
}