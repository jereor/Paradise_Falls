using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotShield : MonoBehaviour
{

    // Used to check if the shield hit the ground collider. This happens when riot drone is charging towards the player but they dodge the charge. Set state to "Stunned".
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !GameObject.Find("RiotControlDrone").GetComponent<RiotControlDrone>().getIsEnraged())
        {
            GameObject.Find("RiotControlDrone").GetComponent<RiotControlDrone>().state = RiotControlDrone.RiotState.Stunned;
            Debug.Log("RiotChargedWall");
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Ground") && GameObject.Find("RiotControlDrone").GetComponent<RiotControlDrone>().getIsEnraged())
        {
            GameObject.Find("RiotControlDrone").GetComponent<RiotControlDrone>().state = RiotControlDrone.RiotState.PhaseTwoStun;
            Debug.Log("PhaseTwoStun");
        }
    }
}
