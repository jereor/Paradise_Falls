using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Events;

public class Lever : Interactable
{
    [Header("Objects to interact with")]
    public GameObject[] objectsToInteractWith; // Objects the lever interacts with.
    [SerializeField] private string ifRequiresMultitoolText = "Wield Multitool to interact.";
    [SerializeField] private float leverTurnTime;

    [Header("Interaction")]
    public bool isMultiUseLever = false; // If this lever is used multiple times, tap this bool.
    public UnityEvent @event;

    private bool isLeverUsed = false;
    private bool isTurnedToLeft = false;
    private bool turning = false;
    private bool objectIsMoving = false;
    private bool isEventInvoked = false;

    // Player is in the range of lever
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name.Contains("Player") && !isLeverUsed)
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

    public override void ShowFloatingText()
    {
        if (GameObject.Find("Player").GetComponent<PlayerCombat>().getWeaponWielded())
            base.ShowFloatingText();
        else
            floatingText.text = ifRequiresMultitoolText;

    }

    // Basic interaction function for levers. Action happens in the object lever is pointing at.
    public override void Interact()
    {
        // Lever functionality. Call for the attached objects' functionality here.
        // If player doesn't have the tool for using lever spots, return.
        if (!GameObject.Find("Player").GetComponent<PlayerCombat>().getWeaponWielded()){ return; }
        if(!turning && !isLeverUsed)
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
                if (!isEventInvoked)
                {
                    @event.Invoke();
                    isEventInvoked = true;
                }
            }
            objectIsMoving = false;
        }
    }

    private IEnumerator LeverTurnTime()
    {
        HideFloatingText();
        turning = true;
        Transform child = gameObject.transform.GetChild(2);
        child.gameObject.SetActive(true);
        if(!isTurnedToLeft)
        {
            child.DORotate(new Vector3(0, 0, 35), leverTurnTime);
            isTurnedToLeft = true;
        }
        else
        {
            child.DORotate(new Vector3(0, 0, -35), leverTurnTime);
            isTurnedToLeft = false;
        }

        GameObject.Find("Player").GetComponent<PlayerCombat>().setWeaponWielded(false);
        yield return new WaitForSeconds(leverTurnTime);
        GameObject.Find("Player").GetComponent<PlayerCombat>().setWeaponWielded(true);

        child.gameObject.SetActive(false);
        turning = false;
        ShowFloatingText();
    }

    public bool GetIsUsed()
    {
        return isLeverUsed;
    }
}
