using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{
    // Updated true when player is close to object that is interactable
    [SerializeField] private bool canInteract;              // If object has given persmission to interact with
    [SerializeField] private GameObject objectToInteract;   // Object that we will interact with
    [SerializeField] private bool isNPC = false;
    [SerializeField] private bool isInteractingWithNPC = false;
    [SerializeField] private bool allowTextAdvance = false;

    public void Interact(InputAction.CallbackContext context)
    {
        // context.started = onButtonDown only event is invoked only once
        if (context.started && canInteract && objectToInteract != null && !PauseMenuController.GameIsPaused && !InventoryPanelController.InventoryIsActive)
        {
            if (objectToInteract.GetComponent<Lever>() // Return if the object is a lever and player does not have multitool
                && (!Player.Instance.MultitoolUnlocked() || !PlayerCombat.Instance.getWeaponWielded())) return;
            objectToInteract.GetComponent<Interactable>().itemEvent.Invoke();   // Invoke virtual function event call this virtual function is modified in item scripts to do something 
        }
        else if(context.started && isNPC && objectToInteract != null && !PauseMenuController.GameIsPaused && !InventoryPanelController.InventoryIsActive)
        {
            objectToInteract.GetComponent<ExplorerDroneController>().Interact();
            isInteractingWithNPC = true;
        }
    }

    // Called from item that we could be interacting with item gives and takes permission to player to interact
    public void AllowInteract(bool b)
    {
        canInteract = b;
    }

    // Called from item that we could be interacting with item gives itself to player so we know whos Interact() Event we are Invoking
    // Item should null this too when player leaves proximity
    public void GiveGameObject(GameObject obj)
    {
        objectToInteract = obj;
    }

    // Same keybind with melee attack currently but does not go into function execution unless talking to NPC.
    // Calls the NPC function AdvanceText() to go through the NPC text.
    public void AdvanceText(InputAction.CallbackContext context)
    {
        if(context.started && allowTextAdvance && objectToInteract != null)
        {
            objectToInteract.GetComponent<ExplorerDroneController>().AdvanceText();
        }
    }

    // Allows the text advancing while talking to an NPC.
    public void AllowTextAdvance(bool b)
    {
        allowTextAdvance = b;
    }

    public void IsNPC(bool b)
    {
        isNPC = b;
    }

    public void SetIsInteractingWithNPC(bool b)
    {
        isInteractingWithNPC = b;
    }

    public bool GetisInteractingWithNPC()
    {
        return isInteractingWithNPC;
    }
}
