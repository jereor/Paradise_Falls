using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockwaveTool : MonoBehaviour
{
    public bool ShockwaveJumpUsed { get; private set; }
    public bool ShockwaveAttackUsed { get; private set; }
    public bool ShockwaveDiveUsed { get; private set; }
    public bool ShockwaveDashUsed { get; private set; }


    [Header("Shockwave parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackCost;

    [Header("Dash parameters")]
    [SerializeField] private float dashSpeed; // Velocity for dash if 0 no dash 
    [SerializeField] private float dashDistance; // Distance of dash
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashCost;
    [SerializeField] private float dashLift;

    [Header("References")]
    [SerializeField] private ParticleSystem shockwaveAttackEffect;
    [SerializeField] private ParticleSystem shockwaveJumpEffect;
    [SerializeField] private ParticleSystem shockwaveDashEffect;
    [SerializeField] private GameObject shockwaveDiveGraphics;
    [SerializeField] private Energy energyScript;

    private bool lifted = true; // used in dash lift

    // Other references
    private Player playerScript;
    private PlayerMovement playerMovementScript;
    private Rigidbody2D rb;

    private Vector3 posBeforeDash;
    private float horizontal = 0f;

    private float nextShockwave = -1; // Initialize as -1. First time it will always be less than the current time so use it can be used. 
    private float nextDash = -1;
    private float timeDashed = -1;

    private bool canReceiveInputSHAttack = false;
    private bool canReceiveInputDash = false;

    // Booleans for PlayerPlaySound.cs
    private bool playSoundDive = false;
    private bool playSoundJump = false;
    private bool playSoundDash = false;

    private void Start()
    {
        playerScript = gameObject.transform.parent.GetComponent<Player>();
        playerMovementScript = gameObject.transform.parent.GetComponent<PlayerMovement>();
        rb = gameObject.transform.parent.GetComponent<Rigidbody2D>();

        EnableInputSHAttack();
        EnableInputDash();
    }


    private void FixedUpdate()
    {
        Dash();
    }

    // Shockwave Jump: Triggered from PlayerMovement when double jumping
    public void ShockwaveJump()
    {
        ShockwaveJumpUsed = true;
        shockwaveJumpEffect.Play();
        playSoundJump = true;
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
        playSoundDive = true;
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
        if (!playerScript.ShockwaveAttackUnlocked() || !canReceiveInputSHAttack) return;

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


    public void DashInput(InputAction.CallbackContext context)
    {
        if (!playerScript.DashUnlocked() || !canReceiveInputDash) return;

        if (context.started && Time.time >= nextDash && !playerMovementScript.getClimbing() && !playerScript.GetIsBlocking() && !playerScript.GetIsAttacking() && !PlayerCombat.Instance.getHeavyBeingCharged())
        {
            if (energyScript.CheckForEnergy(dashCost))
            {
               //rb.position = new Vector2(rb.position.x, rb.position.y + dashLift);

                nextDash = Time.time + dashCooldown;
                timeDashed = Time.time;
                shockwaveDashEffect.Play();
                energyScript.UseEnergy(dashCost);
                lifted = false;
                ShockwaveDashUsed = true;
                posBeforeDash = transform.position;
                horizontal = playerMovementScript.horizontal;

                playSoundDash = true;
            }
            else
                Debug.Log("Not enough energy!");
        }
    }
    
    private void Dash()
    {
        // Give player a lift from ground so we can jump in polygon collider
        if (!lifted)
        {
            rb.gameObject.transform.position = new Vector2(rb.gameObject.transform.position.x, rb.gameObject.transform.position.y + dashLift);
            lifted = true;
        }
        // We reach the dashDistance OR time it would take to move to max dis is reached OR body OR feet are touching wall
        else if (ShockwaveDashUsed && ((posBeforeDash - transform.position).magnitude >= dashDistance || Time.time - timeDashed >= dashDistance / dashSpeed || playerMovementScript.BodyIsTouchingWall() || playerMovementScript.FeetAreTouchingWall()))
        {
            rb.velocity = Vector2.zero;
            ShockwaveDashUsed = false;
        }
        // Dash needs continuous input since inactive
        else if (ShockwaveDashUsed)
        {
            if(horizontal != 0)
                rb.velocity = new Vector2(transform.localScale.x * dashSpeed * horizontal, 0f);
            else if(playerScript.IsFacingRight())
                rb.velocity = new Vector2(transform.localScale.x * dashSpeed * 1f, 0f);
            else
                rb.velocity = new Vector2(transform.localScale.x * dashSpeed * -1f, 0f);
            // Add static force to rb to keep it floating in same height
            rb.AddForce(new Vector2(0f, Physics2D.gravity.y * rb.mass), ForceMode2D.Force);
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

    public void ResetDash()
    {
        ShockwaveDashUsed = false;
        nextDash = Time.time;
    }


    // For PlayerPlaySound to track these states
    public bool getPlaySoundDive()
    {
        if (playSoundDive)
        {
            playSoundDive = false;
            return true;
        }
        else
            return false;
    }

    public bool getPlaySoundJump()
    {
        if (playSoundJump)
        {
            playSoundJump = false;
            return true;
        }
        else
            return false;
    }

    public bool getPlaySoundDash()
    {
        if (playSoundDash)
        {
            playSoundDash = false;
            return true;
        }
        else
            return false;
    }

    public void EnableInputSHAttack()
    {
        canReceiveInputSHAttack = true;
    }

    public void DisableInputSHAttack()
    {
        canReceiveInputSHAttack = false;
    }

    public void EnableInputDash()
    {
        canReceiveInputDash = true;
    }

    public void DisableInputDash()
    {
        canReceiveInputDash = false;
    }
}