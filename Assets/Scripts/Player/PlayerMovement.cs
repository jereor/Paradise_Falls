using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] private float movementVelocity; // Movement speed variable
    [SerializeField] private float jumpForce; // Jump height variable
    [SerializeField] private float coyoteTime; // Determines coyote time forgiveness

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // GameObject attached to player that checks if touching ground
    [SerializeField] private float checkRadius; // Determines radius for ground checks
    [SerializeField] LayerMask groundLayer; // Chosen layer that is recognized as ground in ground checks

    // State variables
    private float horizontal; // Tracks horizontal input direction
    private bool isFacingRight = true; // Tracks player sprite direction
    private float? jumpButtonPressedTime; // Saves the time when player presses jump button
    private float? lastGroundedTime;

    // References
    private Rigidbody2D rb;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Movement
        rb.velocity = new Vector2(horizontal * movementVelocity, rb.velocity.y); // Moves the player by horizontal input

        // Coyote Time
        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
        }

        // Character flip
        if (!isFacingRight && horizontal > 0f) // Flip when turning right
            Flip();
        else if (isFacingRight && horizontal < 0f) // Flip when turning left
            Flip();
    }

    // Returns true if ground check detects ground
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    // Flips player by changing localScale
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // Move action: Called when the Move Action Button is pressed
    public void Move(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        horizontal = context.ReadValue<Vector2>().x; // Updates the horizontal input direction
    }

    // Jump action: Called when the Jump Action button is pressed
    public void Jump(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        jumpButtonPressedTime = Time.time;
        
        // If button was pressed
        if (context.performed && (Time.time - lastGroundedTime <= coyoteTime) // Check if coyote time is online
            && (Time.time - jumpButtonPressedTime <= coyoteTime)) // Check if jump has been buffered
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Keep player in upwards motion
        }

        // If button was released
        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f); // Slow down player
            jumpButtonPressedTime = null;
            lastGroundedTime = null;
        }
    }
}
