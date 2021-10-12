using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleScript : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCombat.Instance.meleeInputReceived = false;
        Player.Instance.SetCurrentState(Player.State.Idle);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Player melees
        if (PlayerCombat.Instance.meleeInputReceived && !PlayerCombat.Instance.heavyHold)
        {
            Player.Instance.animator.Play("LAttack1");
        }

        // Player starts moving
        if (PlayerMovement.Instance.horizontal != 0f)
        {
            animator.SetBool("isRunning", true);
        }

        // Jumping
        if (PlayerMovement.Instance.jumpInputReceived)
        {
            animator.SetBool("jump", true);
        }

        //if (PlayerMovement.Instance.IsGrounded())
        //{
        //    animator.SetBool("jump", false);
        //    PlayerMovement.Instance.jumpInputReceived = false;
        //}
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCombat.Instance.meleeInputReceived = false;
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
