using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockwaveTool : MonoBehaviour
{
    public bool ShockwaveUsed { get; private set; }

    [Header("Shockwave parameters")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float usageCooldown;

    [Header("References")]
    [SerializeField] private Player playerScript;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject shockwaveDirection;
    [SerializeField] private ParticleSystem shockwaveEffect;

    private float nextShockwave = -1; // Initialize as -1. First time it will always be less than the current time so use it can be used. 

    private void Update()
    {
        UpdateDirection();    
    }

    private void UpdateDirection()
    {
        var vertical = playerScript.InputVertical;

        switch (vertical)
        {
            case -1: shockwaveDirection.transform.localPosition = new Vector3(-1, 1, 0); break; // DIVE
            case 1: shockwaveDirection.transform.localPosition = new Vector3(-1, -1, 0); break; // JUMP
        }
    }

    // Shockwave action: Called when the Shockwave Action Button is pressed
    public void Shockwave(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        if (context.started && Time.time >= nextShockwave)
        {
            nextShockwave = Time.time + usageCooldown;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            shockwaveEffect.Play();
            StartCoroutine(ActivateCooldown(usageCooldown));
        }
    }

    private IEnumerator ActivateCooldown(float cooldownTime)
    {
        ShockwaveUsed = true;

        float cdTimer = 0;
        while (cdTimer <= cooldownTime)
        {
            cdTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        ShockwaveUsed = false;
    }
}
