using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockwaveTool : MonoBehaviour
{
    public bool ShockwaveJumpUsed { get; set; }
    public bool ShockwaveAttackUsed { get; set; }

    [Header("Shockwave parameters")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float usageCooldown;

    [Header("References")]
    [SerializeField] private Player playerScript;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject shockwaveDirection;
    [SerializeField] private ParticleSystem shockwaveJumpEffect;
    [SerializeField] private ParticleSystem shockwaveDiveEffect;
    [SerializeField] private ParticleSystem shockwaveAttackEffect;

    private float nextShockwave = -1; // Initialize as -1. First time it will always be less than the current time so use it can be used. 

    private void Update()
    {
        UpdateDirection();
    }

    private void UpdateDirection()
    {
        // NEW WAY WITH ONLY HORIZONTAL INPUT

        // When not shockwave jumping or diving
        float horizontal = playerScript.IsFacingRight() ? 1 : -1;
        shockwaveDirection.transform.localPosition = new Vector3(0, 0, 0);
        shockwaveDirection.transform.rotation = Quaternion.Euler(0, 0, -90 * horizontal);

        /* OLD WAY WITH VERTICAL INPUT
        float vertical = playerScript.InputVertical;
        switch (vertical)
        {
            case 0:
                float horizontal = playerScript.IsFacingRight() ? 1 : -1;
                shockwaveDirection.transform.localPosition = new Vector3(0, 0, 0);
                shockwaveDirection.transform.rotation = Quaternion.Euler(0, 0, -90 * horizontal);
                break;
            case -1:
                shockwaveDirection.transform.localPosition = new Vector3(-1.5f, -1, 0);
                shockwaveDirection.transform.rotation = Quaternion.Euler(0, 0, 180);
                break; // DIVE
            case 1: 
                shockwaveDirection.transform.localPosition = new Vector3(-1.5f, -1, 0);
                shockwaveDirection.transform.rotation = Quaternion.Euler(0, 0, 0);
                break; // JUMP
        }
        */
    }

    // Shockwave Jump: Triggered when double jumping
    public void ShockwaveJump() // Context tells the function when the action is triggered
    {
        shockwaveJumpEffect.Play();
    }

    public void ShockwaveDive()
    {
        shockwaveDiveEffect.Play();
    }

    // ShockwaveAttack action: Called when the ShockwaveAttack Action Button is pressed
    public void ShockwaveAttack(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        if (context.started && Time.time >= nextShockwave)
        {
            nextShockwave = Time.time + usageCooldown;
            shockwaveAttackEffect.Play();

            // Shockwave attack functionality here

            StartCoroutine(ActivateAttackCooldown(usageCooldown));
        }
    }

    private IEnumerator ActivateJumpCooldown(float cooldownTime)
    {
        ShockwaveJumpUsed = true;

        float cdTimer = 0;
        while (cdTimer <= cooldownTime)
        {
            cdTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        ShockwaveJumpUsed = false;
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
