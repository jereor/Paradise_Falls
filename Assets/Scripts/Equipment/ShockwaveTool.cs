using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockwaveTool : MonoBehaviour
{
    // Shockwave action: Called when the Shockwave Action Button is pressed
    public void Shockwave(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        if (context.started)
            Debug.Log("Shockwave!");
    }
}
