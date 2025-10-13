using UnityEngine;

public interface IGController
{
    public int actionTokens { get; set; }
    
    public void StartTurn();

    public void StartAction();
    
    public void OnActionOver();
    
    public void EndTurn();
    
    /** Register to the Turn Base Manager (Need to be called in Start or Awake) */
    private void SubscribeToTurnBaseManager()
    {
        GTurnBaseManager.Instance.RegisterController(this);
    }
    
    private void StopTurn()
    {
        GTurnBaseManager.Instance.RequestEndTurn(this);
    }
}