using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldGrind : MonoBehaviour
{
    public static ShieldGrind Instance;

    [SerializeField] private Transform pipeCheckTransform; // Same transform as groundcheck transform
    [SerializeField] private float checkRadius;
    [SerializeField] LayerMask pipeLayer;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] private float speed; // current speed
    [SerializeField] private float acceleration;
    [SerializeField] private float accMultiplier;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSpeed;
    [SerializeField] private float dashMaxSpeed;

    private bool dashed = false; // Used in calculating speed if we dashed

    private List<Collider2D> disabledColliders = new List<Collider2D>(); // If we disabled colliders via this script we need to save paths to them to enable them again

    private bool grinding = false;
    public bool jumpButtonPressed = false;

    public Rigidbody2D rb; // Player will move with rb.velocity
    public ShockwaveTool shScript; // we need to check dashes
    private float bufferScale = 0f; // Used when we are jumping or changin direction in air

    private void Start()
    {
        Instance = this;
    }

    void FixedUpdate()
    {
        Grind();
    }

    // Checks if player has unlocked ShieldGrind ability and applies velocity to player if he is on top of pipeLayer object
    private void Grind()
    {
        if((PlayerMovement.Instance.IsGrounded() || PlayerMovement.Instance.BodyIsTouchingWall()) && !PipeCheck())
        {
            grinding = false;
            PlayerLeavePipe();
            return;
        }
        // Check if player has it unlocked and check our feet if there is objects
        if (Player.Instance.ShieldGrindUnlocked() && PipeCheck())
        {
            // If we are not grinding before we need to set some parameters on start
            if (!grinding)
            {
                grinding = true;
                speed = minSpeed;
                // Stop other player movement
                if (rb.velocity.x != 0f)
                    rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            // We landed and we need to update our bufferScale and dashed here
            if (bufferScale != transform.localScale.x)
            {
                dashed = false;
                bufferScale = transform.localScale.x;
            }
        }
        // No ability -> Player falls through pipe objects
        else if (!Player.Instance.ShieldGrindUnlocked() && PipeCheck())
        {
            DisableColliders();
        }
        // Stops grinding if we use grappling point in between grinds
        if (PlayerCombat.Instance.getIsPlayerBeingPulled())
            grinding = false;

        // Movement of grinding
        if (grinding)
        {
            // We use dash when grinding
            if (shScript.ShockwaveDashUsed)
            {
                // Dash used in air 
                if (!PlayerMovement.Instance.IsGrounded())
                {
                    speed = minSpeed;
                    dashed = true;
                }
                // Dash used on the pipe
                else if (PlayerMovement.Instance.IsGrounded())
                {
                    speed = dashMaxSpeed;
                    dashed = true;
                }
            }
            // Check if dash effects our grinding
            else if (PlayerMovement.Instance.IsGrounded())
            {
                // Grind some time with dash speed
                if(speed >= maxSpeed)
                {
                    speed -= acceleration * accMultiplier * Time.deltaTime;
                }
                // Normal grind
                else if (speed < maxSpeed)
                    speed += acceleration * Time.deltaTime;

                rb.velocity = new Vector2(bufferScale * speed, rb.velocity.y);
            }
            // We are in air and we havent dashed decrease our speed (air resistance)
            else if (!PlayerMovement.Instance.IsGrounded())
            {
                if (speed > minSpeed && speed < maxSpeed)
                    speed -= acceleration / accMultiplier * Time.deltaTime;
                else if(speed >= maxSpeed)
                    speed -= acceleration * accMultiplier * Time.deltaTime;

                rb.velocity = new Vector2(bufferScale * speed, rb.velocity.y);
            }

            // If our input is down + space + we are grinding we wish to go through the pipe disable colliders
            if (Player.Instance.InputVertical < 0f && jumpButtonPressed && PipeCheck())
            {
                DisableColliders();
            }
        }
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpButtonPressed = true;

        if (context.canceled)
            jumpButtonPressed = false;
    }
    
    // Checks colliders below and disables them and adds them to the list for enabling later
    private void DisableColliders()
    {
        Collider2D pipeCol = Physics2D.OverlapCircle(pipeCheckTransform.position, checkRadius, pipeLayer);
        Collider2D groundCol = Physics2D.OverlapCircle(pipeCheckTransform.position, checkRadius, groundLayer);

        if (!disabledColliders.Contains(pipeCol))
            disabledColliders.Add(pipeCol);

        if (!disabledColliders.Contains(groundCol))
            disabledColliders.Add(groundCol);

        foreach (Collider2D col in disabledColliders)
        {
            col.enabled = false;
        }
    }

    // Check if we are on top of pipeLayerObject
    public bool PipeCheck()
    {
        return Physics2D.OverlapCircle(pipeCheckTransform.position, checkRadius, pipeLayer);
    }

    // Called from ShieldGrindEndPointTrigegr.cs scripts that are positioned to the end points of pipes in scene
    public void PlayerLeavePipe()
    {
        if (disabledColliders.Count > 0f)
        {
            foreach (Collider2D col in disabledColliders)
            {
                col.enabled = true;
            }
            disabledColliders.Clear();
        }
    }

    public bool getGrinding()
    {
        return grinding;
    }
}
