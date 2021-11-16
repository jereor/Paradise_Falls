using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : StateMachineBehaviour
{
    public AnimationClip transitionAnimation;
    private int comboIndex;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.SetCurrentState(Player.State.Blocking);

        //PlayerCombat.Instance.UpdateCombo(PlayerCombat.Instance.getCurrentComboIndex(), transitionAnimation.length);
        if (PlayerCombat.Instance.getComboActive())
        {
            // Save comboIndex since stopcombotimer sets it to zero
            comboIndex = PlayerCombat.Instance.getCurrentComboIndex();
            PlayerCombat.Instance.StopComboTimer();
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Transition length is 0.5 s parry after this animation is 0.5 s so we need doubled
        PlayerCombat.Instance.UpdateCombo(comboIndex, transitionAnimation.length * 2f);
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
