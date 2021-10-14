using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimScript : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.SetCurrentState(Player.State.Aiming);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // If we need to flip we setFacingRight correctly
        PlayerMovement.Instance.setFacingRight(PlayerCombat.Instance.getVectorToMouse().normalized.x > 0 ? true : false);
        // Flip local scale if aiming is to the other x
        animator.gameObject.transform.localScale = new Vector3(PlayerCombat.Instance.getVectorToMouse().normalized.x > 0 ? 1 : -1, 1, 1); // Flip player to face towards the shooting direction

        // Player starts moving
        if (PlayerMovement.Instance.horizontal != 0f)
        {
            animator.SetBool("isRunning", true);
        }

        if (PlayerCombat.Instance.throwInputReceived)
        {
            animator.Play("Throw");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
