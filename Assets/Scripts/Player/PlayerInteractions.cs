using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{
    // Updated true when player is close to object that is interactable
    [SerializeField] private bool canInteract;              // If object has given persmission to interact with
    [SerializeField] private GameObject objectToInteract;   // Object that we will interact with

    public void Interact(InputAction.CallbackContext context)
    {
        // context.started = onButtonDown only event is invoked only once
        if (context.started && canInteract && objectToInteract != null)
        {
            Debug.Log("Trying to interact");
            objectToInteract.GetComponent<Interactable>().itemEvent.Invoke();   // Invoke virtual function event call this virtual function is modified in item scripts to do something 
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
}
