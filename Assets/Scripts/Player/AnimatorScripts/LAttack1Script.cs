using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LAttack1Script : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Set this to false so when we enter LTran1 we can start LAttack2 if we press melee button
        // If not set here melee continues until this is set false
        PlayerCombat.Instance.meleeInputReceived = false;
        // Set state to Player.cs
        Player.Instance.SetCurrentState(Player.State.Attacking);
        // Set bool for controller we are currently attacking includes LAttack1 and LTran1
        animator.SetBool("isAttacking", true);
        // Small dash forward PlayerCombat.cs
        PlayerCombat.Instance.AttackDash();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Deal damage when we hit enemy
        PlayerCombat.Instance.DealDamage(1, false);
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
