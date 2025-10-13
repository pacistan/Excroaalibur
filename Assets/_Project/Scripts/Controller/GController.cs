using UnityEngine;

public class GController : MonoBehaviour
{
    public int actionTokens { get; set; }
    
    public virtual void StartTurn() {}

    public virtual void StartAction() {}
    
    public virtual void OnActionOver() {}
    
    public virtual void EndTurn() {}
    
    protected void StopTurn()
    {
        GTurnBaseManager.Instance.RequestEndTurn(this);
    }
}