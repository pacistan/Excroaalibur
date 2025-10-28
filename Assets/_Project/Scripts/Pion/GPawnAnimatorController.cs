using UnityEngine;

public class GPawnAnimatorController : MonoBehaviour
{
    GPawn _pawn;

    void Awake()
    {
        _pawn = GetComponentInParent<GPawn>();
    }

    public void PushEvent()
    {
        _pawn.OnPushEvent();
    }
    
    public void ThrowEvent()
    {
        _pawn.OnThrowEvent();
    }
}
