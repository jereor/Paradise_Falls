using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockwaveTool : MonoBehaviour
{
    public bool ShockwaveJumpUsed { get; set; }
    public bool ShockwaveAttackUsed { get; set; }

    [Header("Shockwave parameters")]
    [SerializeField] private float usageCooldown;

    [Header("References")]
    [SerializeField] private ParticleSystem shockwaveJumpEffect;
    [SerializeField] private ParticleSystem shockwaveDiveEffect;
    [SerializeField] private ParticleSystem shockwaveAttackEffect;

    private float nextShockwave = -1; // Initialize as -1. First time it will always be less than the current time so use it can be used. 

    // Shockwave Jump: Triggered from PlayerMovement when double jumping
    public void ShockwaveJump() // Context tells the function when the action is triggered
    {
        shockwaveJumpEffect.Play();
    }

    // Shockwave Dive: Triggered from PlayerMovement when activating dive
    public void ShockwaveDive()
    {
        shockwaveDiveEffect.Play();
    }

    // ShockwaveAttack action: Called when the ShockwaveAttack Action Button is pressed
    public void ShockwaveAttack(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        if (context.started && Time.time >= nextShockwave)
        {
            nextShockwave = Time.time + usageCooldown; // Sets a time when attack can be used again
            shockwaveAttackEffect.Play();

            // Shockwave attack functionality here

            StartCoroutine(ActivateAttackCooldown(usageCooldown));
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
