using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockwaveTool : MonoBehaviour
{
    public bool ShockwaveJumpUsed { get; private set; }
    public bool ShockwaveAttackUsed { get; private set; }
    public bool ShockwaveDiveUsed { get; private set; }

    [Header("Shockwave parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackCost;

    [Header("References")]
    [SerializeField] private ParticleSystem shockwaveAttackEffect;
    [SerializeField] private ParticleSystem shockwaveJumpEffect;
    [SerializeField] private GameObject shockwaveDiveGraphics;
    [SerializeField] private Energy energyScript;

    // Other references
    private Player playerScript;

    private float nextShockwave = -1; // Initialize as -1. First time it will always be less than the current time so use it can be used. 

    private void Start()
    {
        playerScript = gameObject.transform.parent.GetComponent<Player>();
    }

    // Shockwave Jump: Triggered from PlayerMovement when double jumping
    public void ShockwaveJump()
    {
        ShockwaveJumpUsed = true;
        shockwaveJumpEffect.Play();
    }

    public void ResetShockwaveJump()
    {
        ShockwaveJumpUsed = false;
    }

    // Shockwave Dive: Triggered from PlayerMovement when activating dive
    public void DoShockwaveDive()
    {
        ShockwaveDiveUsed = true;
        shockwaveDiveGraphics.SetActive(true);
    }

    // Cancel Shockwave Dive: Triggered from PlayerMovement when deactivating dive
    public void CancelShockwaveDive()
    {
        // Set dive used to false and disable dive graphics
        ShockwaveDiveUsed = false;
        if (shockwaveDiveGraphics.activeInHierarchy)
            shockwaveDiveGraphics.SetActive(false);
    }

    // ShockwaveAttack action: Called when the ShockwaveAttack Action Button is pressed
    public void ShockwaveAttack(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        if (!playerScript.ShockwaveToolUnlocked()) return;

        if (context.started && Time.time >= nextShockwave)
        {
            if (energyScript.CheckForEnergy(attackCost)) // Check if player has enough energy for the attack
            {
                nextShockwave = Time.time + attackCooldown; // Sets a time when attack can be used again
                shockwaveAttackEffect.Play(); // Activate particle system and let it handle collisions
                energyScript.UseEnergy(attackCost); // Use the required energy
                StartCoroutine(ActivateAttackCooldown(attackCooldown));
            }
            else
                Debug.Log("Not enough energy!");
        }
    }

    private IEnumerator ActivateAttackCooldown(float cooldownTime)
    {
        ShockwaveAttackUsed = true;

        float cdTimer = 0;
        while (cdTimer <= cooldownTime)
        {
            cdTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        ShockwaveAttackUsed = false;
    }
}