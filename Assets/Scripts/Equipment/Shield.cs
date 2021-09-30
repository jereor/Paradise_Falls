using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shield : MonoBehaviour
{
    [SerializeField] private GameObject shield;
    private bool blocking;

    public void Block(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("BLOCKING!");
            blocking = true;
            shield.SetActive(true);
        }
        if (context.canceled)
        {
            shield.SetActive(false);
            Debug.Log("NOT BLOCKING.");
            blocking = false;
        }
    }
}
