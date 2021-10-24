using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shield : MonoBehaviour
{
    public bool Blocking { get; private set; }
    public bool Parrying { get; private set; }
    public float ProtectionAmount { get; private set; }

    [SerializeField] public GameObject shield;
    [SerializeField] private bool shieldUnlocked = false;

    [Header("Shield variables")]
    [SerializeField] private float protectionValue;
    [SerializeField] private float parryTimeWindow = 0.5f;
   
    private bool hitParried = false; // State variable for tracking successful parries
    private float nextParry = -1; // Initialize as -1. First time it will always be less than the current time and will allow to parry. 
    private float parryCooldown = 0.6f;

    private bool canReceiveInputBlock;
    
    private void Start()
    {
        ProtectionAmount = protectionValue;

        EnableInputBlock();
    }

    // Shield action: Called when the Shield Action button is pressed
    public void Block(InputAction.CallbackContext context)
    {
        if (!canReceiveInputBlock || !shieldUnlocked)
            return;

        // Blocking
        if (context.started && (Time.time >= nextParry || hitParried))
        {
            nextParry = Time.time + parryCooldown; // Sets a time when parry can be used again
            hitParried = false; // Reset hitParried state

            //shield.SetActive(true); // Activate shield graphics EDIT: JereT replaced to be activated from Player.cs
            Blocking = true;
        }
        // Blocking cancelled
        if (context.canceled)
        {
            StartCoroutine(ActivateParryWindow()); // Parry window activated
            //shield.SetActive(false); // Disable shield graphics
            Blocking = false;
        }
    }

    public void HitWhileParried()
    {
        hitParried = true;
    }

    // Activated when Shield button is released
    // Other objects in world check if Shield is Parrying and do their own thing
    // For example melee enemies get stunned if they attack and Shield is currently Parrying
    private IEnumerator ActivateParryWindow()
    {
        Parrying = true;

        float parryTimer = 0;
        while (parryTimer <= parryTimeWindow)
        {
            parryTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Parrying = false;
    }

    public float getParryTimeWindow()
    {
        return parryTimeWindow;
    }

    public void EnableInputBlock()
    {
        canReceiveInputBlock = true;
    }
    public void DisableInputBlock()
    {
        canReceiveInputBlock = false;
    }

    // Unlock shield through ShieldPickUp-script when Shield picked up in game
    public void ShieldUnlock()
    {
        shieldUnlocked = true;
    }
}
