using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{
    // Updated true when player is close to object that is interactable
    [SerializeField] private bool canInteract;
    [SerializeField] private GameObject objectToInteract;   // Object that we will interact with

    public void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("Trying to interact"); 
        if (canInteract && objectToInteract != null)
        {
            objectToInteract.GetComponent<Interactable>().itemEvent.Invoke();   // Invoke virtual function event call this virtual function is modified in item scripts to do something 
        }
    }

    public void AllowInteract(bool b)
    {
        canInteract = b;
    }

    public void GiveGameObject(GameObject obj)
    {
        objectToInteract = obj;
    }
}
