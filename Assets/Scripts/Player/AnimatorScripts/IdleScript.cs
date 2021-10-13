using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleScript : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.SetCurrentState(Player.State.Idle);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Player melees light attack and we arent currently climbing
        if (PlayerCombat.Instance.meleeInputReceived && !PlayerCombat.Instance.heavyHold && !animator.GetBool("isClimbing"))
        {
            Player.Instance.animator.Play("LAttack1");
        }

        // Player starts moving
        if (PlayerMovement.Instance.horizontal != 0f)
        {
            animator.SetBool("isRunning", true);
        }

        // Throw
        if (PlayerCombat.Instance.throwInputReceived)
        {
            animator.Play("Throw");
        }

        // Jump Launch
        if (PlayerMovement.Instance.jumpInputReceived)
        {
            animator.Play("Launch");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

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
