using Sirenix.OdinInspector;
using UnityEngine;

public class ASI_LoopBehaviour : StateMachineBehaviour
{
    [SerializeField, Range(0f, 1f)]
    float _breakChance = .1f;
    
    [SerializeField, Range(0f, 1f)]
    float _break1Chance = .5f;
    
    bool _hasLoopReset;
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if (stateInfo.normalizedTime % 1 > .9f && !_hasLoopReset) 
        {
            _hasLoopReset = true;
            OnLoopReset(animator);
        }
        
        if(stateInfo.normalizedTime % 1 < .1f && _hasLoopReset)
            _hasLoopReset = false;
    }

    private void OnLoopReset(Animator animator)
    {
        float randValue = UnityEngine.Random.Range(0f, 1f);
        if (randValue < _breakChance)
        {
            randValue = UnityEngine.Random.Range(0f, 1f);
            animator.SetBool("BreakIdle", true);
            animator.SetBool("IsBreak1", randValue > _break1Chance);
        }
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("BreakIdle", false);
        animator.SetBool("IsBreak1", false);
    }


}
