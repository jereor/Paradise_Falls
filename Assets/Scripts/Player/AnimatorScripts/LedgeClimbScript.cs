using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeClimbScript : StateMachineBehaviour
{
    private float yOffset;
    public float yOffsetFixValue; // This is value will smooth the positions of hands
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.SetCurrentState(Player.State.Climbing);
        yOffset = PlayerMovement.Instance.getLedgeHitOffsetRay().distance;

        // Move gameobject slighly lower so animations hands will be positioned on the ledge
        animator.gameObject.transform.position = animator.gameObject.transform.position + new Vector3(0f, -(yOffset - yOffsetFixValue), 0f);

        // Activate camera smoothing
        PlayerCamera.Instance.SmoothFollow(.7f);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerMovement.Instance.LedgeClimb();
        animator.SetBool("isClimbing", false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
