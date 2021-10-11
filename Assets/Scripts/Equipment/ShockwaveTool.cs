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

    private float nextShockwave = -1; // Initialize as -1. First time it will always be less than the current time so use it can be used. 

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

    public void CancelShockwaveDive()
    {
        ShockwaveDiveUsed = false;
        if (shockwaveDiveGraphics.activeInHierarchy)
            shockwaveDiveGraphics.SetActive(false);
    }

    // ShockwaveAttack action: Called when the ShockwaveAttack Action Button is pressed
    public void ShockwaveAttack(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        if (context.started && Time.time >= nextShockwave)
        {
            if (energyScript.CheckForEnergy(attackCost))
            {
                nextShockwave = Time.time + attackCooldown; // Sets a time when attack can be used again
                shockwaveAttackEffect.Play();
                energyScript.UseEnergy(attackCost);

                // Shockwave attack functionality here

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
