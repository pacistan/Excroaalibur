using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class GPawnAnimatorController : MonoBehaviour
{
    GPawn _pawn;

    [SerializeField]
    UnityEvent onStep;
    [SerializeField]
    UnityEvent onStepFinish;

    void Awake()
    {
        _pawn = GetComponentInParent<GPawn>();
    }
    public void StepFinish()
    {
        onStepFinish.Invoke();
    }
    public void Step()
    {
        onStep?.Invoke();
    }
    
    public void PushEvent()
    {
        _pawn.OnPushEvent();
    }
    
    public void PushEndEvent()
    {
        _pawn.OnPushEndEvent();
    }

    
    public void ThrowEvent()
    {
        _pawn.OnThrowEvent();
    }
}
