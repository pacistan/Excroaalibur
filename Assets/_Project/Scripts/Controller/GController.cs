using UnityEngine;


public interface IGController
{
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