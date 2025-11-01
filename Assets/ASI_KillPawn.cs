using UnityEngine;
using UnityEngine.Animations;

public class ASI_KillPawn : StateMachineBehaviour
{
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex,
        AnimatorControllerPlayable controller)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
        if (stateInfo.normalizedTime > .99f)
        {
            Destroy(animator.GetComponentInParent<GPawn>().gameObject);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}
