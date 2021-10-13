using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunScript : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // If we were running before jumping setBool to false
        // OR we start running in air setBool to false
        // RESULT we run only Jump animation in air (no running in air!)
        if (animator.GetBool("jump"))
        {
            animator.SetBool("isRunning", false);
        }
        // We are really running set state
        else
        {
            Player.Instance.SetCurrentState(Player.State.Running);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Input is false we arent giving input
        if (PlayerMovement.Instance.horizontal == 0f)
        {
            animator.SetBool("isRunning", false);
        }

        // From running to Light attack
        if (PlayerCombat.Instance.meleeInputReceived && !PlayerCombat.Instance.heavyHold && !animator.GetBool("isClimbing"))
        {
            Player.Instance.animator.Play("LAttack1");
        }

        // Throw
        if (PlayerCombat.Instance.throwInputReceived)
        {
            animator.Play("Throw");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{

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
