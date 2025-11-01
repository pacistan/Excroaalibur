using UnityEngine;

public class ASI_RandomNextState : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("Random",  Random.Range(0f, 1f));
    }

    
}
