using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shield : MonoBehaviour
{
    public bool Blocking { get; set; }
    public bool Parrying { get; set; }
    public float ProtectionAmount { get; private set; }

    [SerializeField] public GameObject shield;

    [Header("Shield variables")]
    [SerializeField] private float protectionValue;
    [SerializeField] private float parryTimeWindow = 0.5f;
   
    private bool hitParried = false; // State variable for tracking successful parries
    private float nextParry = -1; // Initialize as -1. First time it will always be less than the current time and will allow to parry. 
    private float parryCooldown = 0.6f;

    private bool canReceiveInputBlock;

    // References
    private Player playerScript;

    // Booleans for PlayerPlaySound.cs
    private bool playBlockActivation = false;
    private bool playParry = false;

    private void Start()
    {
        playerScript = gameObject.GetComponent<Player>();
        ProtectionAmount = protectionValue;

        EnableInputBlock();
    }

    // Shield action: Called when the Shield Action button is pressed
    public void Block(InputAction.CallbackContext context)
    {
        if (!canReceiveInputBlock || !playerScript.ShieldUnlocked()) return;

        // Blocking
        if (context.started && (Time.time >= nextParry || hitParried))
        {
            nextParry = Time.time + parryCooldown; // Sets a time when parry can be used again
            hitParried = false; // Reset hitParried state

            playBlockActivation = true;
            //shield.SetActive(true); // Activate shield graphics EDIT: JereT replaced to be activated from Player.cs
            Blocking = true;
        }
        // Blocking cancelled
        if (context.canceled && Blocking)
        {
            StartCoroutine(ActivateParryWindow()); // Parry window activated
            //shield.SetActive(false); // Disable shield graphics
            Blocking = false;
        }
    }

    public void HitWhileParried()
    {
        playParry = true;
        hitParried = true;

        GetComponent<Health>().PlayParryParticles();
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


    // For PlayerPlaySound to track these states
    public bool getPlaySoundBlockActivate()
    {
        if (playBlockActivation)
        {
            playBlockActivation = false;
            return true;
        }
        else
            return false;
    }

    public bool getPlaySoundParry()
    {
        if (playParry)
        {
            playParry = false;
            return true;
        }
        else
            return false;
    }

}
