using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Lever : Interactable
{
    [Header("Objects to interact with")]
    public GameObject[] objectsToInteractWith; // Objects the lever interacts with.
    public Vector2[] endPosition;
    public float moveTime = 1f;

    [Header("Bools for interaction")]
    public bool isMultiUseLever = false; // If this lever is used multiple times, tap this bool.
    public bool canUseMultitoolForLevers = true; // Does the player character have the tool for using levers?

    private bool isLeverUsed = false;
    private PlayerInteractions playerInteractions;


    // Draws simple gizmos from object locations to their designated spots.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        int i = 0;
        foreach (GameObject movingObject in objectsToInteractWith)
        {

            Gizmos.DrawRay(movingObject.transform.position, new Vector2(endPosition[i].x, endPosition[i].y));
            i++;
        }
            //Gizmos.DrawRay(movingObject.transform.position, new Vector2(movingObject.endPosition.x, endPosition.y));
    }

    // Player is in the range of lever
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name.Contains("Player") && canUseMultitoolForLevers && !isLeverUsed)
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            ShowFloatingText();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isLeverUsed)
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            HideFloatingText();
        }

    }

    // Player is out of range of lever.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            HideFloatingText();
        }
    }

    // Basic interaction function for levers. Action happens in the object lever is pointing at.
    public override void Interact()
    {
        // Lever functionality. Call for the attached objects' functionality here.
        // If player doesn't have the tool for using lever spots, return.
        if (isLeverUsed || !canUseMultitoolForLevers){ return; }
        else
        {
            // Currently used only for testing. Replace this move with objects' own functions.
            Debug.Log("PULLED LEVER");
            int i = 0;
            foreach(GameObject movingObject in objectsToInteractWith)
            {
                if(movingObject.tag == "Drawbridge")
                {
                    movingObject.GetComponent<DrawBridgeController>().Work();
                }
                if(movingObject.tag == "Door")
                {
                    movingObject.transform.DOMove(new Vector2(movingObject.transform.position.x + endPosition[i].x, movingObject.transform.position.y + endPosition[i].y), moveTime);
                }
                //movingObject.transform.DOMove(new Vector2(movingObject.transform.position.x + endPosition[i].x, movingObject.transform.position.y + endPosition[i].y), moveTime);
                i++;
            }

            
            if(!isMultiUseLever)
            {
                isLeverUsed = true;
            }
        }    
    }
}
