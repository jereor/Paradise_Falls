using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shield : MonoBehaviour
{
    public bool Blocking { get; private set; }
    public bool Parrying { get; private set; }
    public float ProtectionAmount { get; private set; }

    [SerializeField] private GameObject shield;
    [SerializeField] private float protectionValue;
    [SerializeField] private float parryTime;

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
        // Blocking cancelled
        if (context.canceled)
        {
            // Parry window activated
            StartCoroutine(ActivateParryWindow());

            shield.SetActive(false);
            Blocking = false;
        }
    }

    // Activated when Shield button is released
    // Other objects in world check if Shield is Parrying and do their own thing
    // For example melee enemies get stunned if they attack and Shield is currently Parrying
    private IEnumerator ActivateParryWindow()
    {
        Debug.Log("Parrying...");
        Parrying = true;

        float parryTimer = 0;
        while (parryTimer <= parryTime)
        {
            parryTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("STOP PARRY");
        Parrying = false;
    }
}
