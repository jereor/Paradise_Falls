using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Events;

public class BasicButton : Interactable
{
    [Header("Objects to interact with")]
    public GameObject[] objectsToInteractWith; // Objects the button interacts with.
    public UnityEvent @event;
    private bool isEventInvoked = false;

    [Header("Bools for interaction")]
    public bool isMultiUse = false; // If this button is used multiple times, tap this bool.

    // References
    [SerializeField] private Sprite pressedSprite;

    // State variables
    public bool isUsed = false;
    private bool pressed = false;
    private bool objectIsMoving = false;

    // Player is in the range of the button
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player") && !isUsed)
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            ShowFloatingText();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.name.Contains("Player") && isUsed)
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            HideFloatingText();
        }

    }

    // Player is leaves the range of the button.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            HideFloatingText();
        }
    }

    // Basic interaction function for button. Action happens in the object button is connected to.
    public override void Interact()
    {
        // Button functionality. Call for the attached objects' functionality here.
        if (!isUsed)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = pressedSprite;
            if(objectsToInteractWith != null)
            {
                foreach (GameObject movingObject in objectsToInteractWith)
                {
                    if (movingObject.tag == "Drawbridge")
                    {
                        if (movingObject.GetComponent<DrawBridgeController>().getMoving()) { objectIsMoving = true; }
                    }
                    if (movingObject.tag == "Door")
                    {
                        if (movingObject.GetComponent<DoorController>().getMoving()) { objectIsMoving = true; }
                    }
                }
                if (!objectIsMoving)
                {
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


                }
            }

            if(!isEventInvoked)
            {
                @event.Invoke();
                isEventInvoked = true;
            }
            if (!isMultiUse)
            {
                isUsed = true;
            }
            objectIsMoving = false;

        }
    }

    public bool GetIsUsed()
    {
        return isUsed;
    }
}
