using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class BasicLever : Interactable
{
    [Header("Objects to interact with")]
    public GameObject[] objectsToInteractWith; // Objects the lever interacts with.
    [SerializeField] private float leverTurnTime;

    [Header("Bools for interaction")]
    public bool isMultiUseLever = false; // If this lever is used multiple times, tap this bool.

    private bool isLeverUsed = false;
    private bool isTurnedToLeft = false;
    private bool turning = false;

    // Player is in the range of lever
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player") && !isLeverUsed)
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            ShowFloatingText();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.name.Contains("Player") && isLeverUsed)
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
        if (!turning && !isLeverUsed)
        {
            StartCoroutine(LeverTurnTime());
            // Currently used only for testing. Replace this move with objects' own functions.
            Debug.Log("PULLED LEVER");
            int i = 0;
            foreach (GameObject movingObject in objectsToInteractWith)
            {
                if (movingObject.tag == "Drawbridge")
                {
                    movingObject.GetComponent<DrawBridgeController>().Work();
                }
                if (movingObject.tag == "Door")
                {
                    movingObject.GetComponent<DoorController>().Work();
                }
                i++;
            }

            if (!isMultiUseLever)
            {
                isLeverUsed = true;
            }
        }
    }

    private IEnumerator LeverTurnTime()
    {
        turning = true;
        Transform child = gameObject.transform.GetChild(2);
        if (!isTurnedToLeft)
        {
            child.DORotate(new Vector3(0, 0, 35), leverTurnTime);
            isTurnedToLeft = true;
        }
        else
        {
            child.DORotate(new Vector3(0, 0, -35), leverTurnTime);
            isTurnedToLeft = false;
        }
        yield return new WaitForSeconds(leverTurnTime);
        turning = false;
    }
}
