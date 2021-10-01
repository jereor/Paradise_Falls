using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shield : MonoBehaviour
{
    public static Shield Instance { get; private set; }

    [SerializeField] private GameObject shield;
    [SerializeField] private float protectionValue;

    public bool Blocking { get; private set; }
    public float ProtectionAmount { get; private set; }

    private void Start()
    {
        ProtectionAmount = protectionValue;
    }

    // Shield action: Called when the Shield Action button is pressed
    public void Block(InputAction.CallbackContext context)
    {
        // Blocking
        if (context.started)
        {
            Blocking = true;
            shield.SetActive(true);
        }
        // Not blocking
        if (context.canceled)
        {
            shield.SetActive(false);
            Blocking = false;
        }
    }
}
