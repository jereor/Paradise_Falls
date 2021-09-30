using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shield : MonoBehaviour
{
    private bool blocking;

    public void Block(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("BLOCKING!");
            blocking = true;
        }
        if (context.canceled)
        {
            Debug.Log("NOT BLOCKING.");
            blocking = false;
        }
    }
}
