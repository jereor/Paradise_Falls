using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerDroneController : Interactable
{
    [Header("Objects to interact with")]
    public GameObject[] objectsToInteractWith; // Objects the lever interacts with.
    [SerializeField] private string ifRequiresMultitoolText = "Wield Multitool to interact.";
    [SerializeField] private float leverTurnTime;

    [Header("Interaction")]
    public bool isMultiUseLever = false; // If this lever is used multiple times, tap this bool.

    private bool isLeverUsed = false;
    private bool isTurnedToLeft = false;
    private bool turning = false;
    private bool objectIsMoving = false;
    private bool isEventInvoked = false;

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
        Debug.Log("Hello!");
        HideFloatingText();
        // Disable player inputs
        // Enable created canvas
        // Only specific keys are usable
    }
}
