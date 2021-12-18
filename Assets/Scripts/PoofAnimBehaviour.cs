using UnityEngine;

public class PoofAnimBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var poof = animator.GetComponent<Poof>();
        if (!poof) return;

        poof.ExecuteFinishAction();
        Destroy(poof.gameObject);
    }
}