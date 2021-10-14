using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpScript : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Usually we are firstly jumping so we can set our state to Jumping if we are Falling OnStateUpdate() will update it instantly 
        Player.Instance.SetCurrentState(Player.State.Ascending);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Player.Instance.GetIsAiming())
            animator.gameObject.transform.localScale = new Vector3(Player.Instance.rb.velocity.normalized.x > 0 ? 1 : -1, 1, 1); // Flip player to face towards the shooting direction

        // State updated according to the velocity y we are currently traveling yVelocity get value from Player.cs HandleStateInputs()
        if (animator.GetFloat("yVelocity") < 0f)
        {
            // Dont update state if our current state is Falling
            if (Player.Instance.GetCurrentState() != Player.State.Falling)
            {
                Player.Instance.SetCurrentState(Player.State.Falling);
            } 
        }
        else if(animator.GetFloat("yVelocity") >= 0f)
        {
            // Dont update state if our current state is Jumping
            if (Player.Instance.GetCurrentState() != Player.State.Ascending)
            {
                Player.Instance.SetCurrentState(Player.State.Ascending);
            }
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
