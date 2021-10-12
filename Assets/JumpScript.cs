using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpScript : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.SetCurrentState(Player.State.Jumping);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // State updated according to the velocity y we are currently traveling yVelocity get value from Player.cs HandleStateInputs()
        if (animator.GetFloat("yVelocity") < 0f)
        {
            Player.Instance.SetCurrentState(Player.State.Falling);
        }
        else if(animator.GetFloat("yVelocity") >= 0f)
        {
            Player.Instance.SetCurrentState(Player.State.Jumping);
        }


        // From jumping or falling to Light attack
        if (PlayerCombat.Instance.meleeInputReceived && !PlayerCombat.Instance.heavyHold)
        {
            Player.Instance.animator.Play("LAttack1");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animation stop when we land we can jump again set this to false
        PlayerMovement.Instance.jumpInputReceived = false;
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
