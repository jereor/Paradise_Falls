using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] private float horizontal; // Tracks horizontal input direction
    [SerializeField] private float movementVelocity; // Movement speed variable
    [SerializeField] private float jumpForce; // Jump height variable
    [SerializeField] private bool isFacingRight = true; // Tracks player sprite direction
    [SerializeField] private float coyoteTime; // Determines coyote time forgiveness

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // GameObject attached to player that checks if touching ground
    [SerializeField] private float checkRadius; // Radius for ground checks
    [SerializeField] LayerMask groundLayer; // Chosen layer that is recognized as ground in ground checks

    public float coyoteTimer; // Timer for coyote jumps

    // References
    private Rigidbody2D rb;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Timers
        coyoteTimer += Time.deltaTime;

        // Movement
        rb.velocity = new Vector2(horizontal * movementVelocity, rb.velocity.y); // Moves the player by horizontal input

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
        //var coyote = coyoteTimer < coyoteTime; // Check if enough time has passed since last coyote jump
        //if (!IsGrounded() || Time.timeScale != 1) return; // Return if time is stopped so players can't abuse pause menu

        if (context.performed && IsGrounded()) // If button was pressed
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Keep player in upwards motion

        if (context.canceled && rb.velocity.y > 0f) // If button was released
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f); // Slow down player
    }
}
