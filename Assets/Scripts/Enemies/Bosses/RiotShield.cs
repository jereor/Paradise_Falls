using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotShield : MonoBehaviour
{
    private RiotControlDrone drone;
    private Health shieldHealth;

    private void Start()
    {
        drone = GetComponentInParent<RiotControlDrone>();
        shieldHealth = GetComponent<Health>();
        shieldHealth.TakeDamage(2);
    }

    private void FixedUpdate()
    {
        //if(shieldHealth.CurrentHealth == 2)
        //{
        //    gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        //}
        //if (shieldHealth.CurrentHealth == 0)
        //{
        //    gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
        //}
    }
    // Used to check if the shield hit the ground collider. This happens when riot drone is charging towards the player but they dodge the charge. Set state to "Stunned".
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(drone.state == RiotControlDrone.RiotState.Stunned)
        {
            drone.PlayerPushback();
        }
        if (drone.state == RiotControlDrone.RiotState.ShieldCharge && collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !drone.getIsEnraged())
        {

            drone.state = RiotControlDrone.RiotState.Stunned;
        }
        else if (drone.state == RiotControlDrone.RiotState.PhaseTwoRun && collision.gameObject.layer == LayerMask.NameToLayer("Ground") && drone.getIsEnraged())
        {
            drone.Flip();
            drone.state = RiotControlDrone.RiotState.PhaseThreeStun;
        }
        if (drone.state == RiotControlDrone.RiotState.ShieldCharge && collision.gameObject.tag == "Box" && !drone.getIsEnraged())
        {
            shieldHealth.TakeDamage(1);
            drone.state = RiotControlDrone.RiotState.Stunned;
        }
    }
}
