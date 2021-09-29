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
    [SerializeField] private float climbTimeBuffer; // Time when we can climb again

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // GameObject attached to player that checks if touching ground
    [SerializeField] private float checkRadius; // Determines radius for ground checks
    [SerializeField] LayerMask groundLayer; // Chosen layer that is recognized as ground in ground checks

    [Header("Ledge and Wall Check")]
    [SerializeField] private bool allowLedgeClimb = true;
    [SerializeField] private bool allowWallJump = true;
    [SerializeField] private Transform ledgeCheck; // Point where Ledge Check Occupation Raycast is cast should be close to top of head
    [SerializeField] private Transform wallCheckBody; // Point where Body Check Raycast is cast
    [SerializeField] private Transform wallCheckFeet; // Point where Feet Check Raycast is cast
    [SerializeField] private float checkDistance; // Distance of raycast and ledge ClimbLedge() offset positions
    [SerializeField] private float climbXOffset;
    [SerializeField] private float climbYOffset;

    // State variables
    private float horizontal; // Tracks horizontal input direction
    private bool moving = false;
    private bool falling = false;
    private bool isFacingRight = true; // Tracks player sprite direction
    private float? jumpButtonPressedTime; // Saves the time when player presses jump button
    private float? lastGroundedTime;

    private float lastTimeClimbed; // This is used to prevent climbing steplike object instantly to the top from first step

    private bool canClimb;
    private bool isClimbing;
    private bool canMove = true;

    private bool canWallJump; // Tells jump function/event that we can jump this is set in CheckWallJump()
    private float wallJumpDir = 0f; // Keeps track if we jump from the left or right (Mathf.Sign() == -1 jumped from left, == 1 jumped from right, == we havent jumped from wall)

    // References
    private Rigidbody2D rb;
    private float defaultGravityScale;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        PlayerCamera.Instance.ChangeCameraOffset(0.2f, false, 1);
        defaultGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        if (canMove)
        {
            // Movement
            rb.velocity = new Vector2(horizontal * movementVelocity, rb.velocity.y); // Moves the player by horizontal input

            // Character flip
            if (!isFacingRight && horizontal > 0f) // Flip when turning right
                Flip();
            else if (isFacingRight && horizontal < 0f) // Flip when turning left
                Flip();
        }
        moving = rb.velocity.x != 0;
        falling = rb.velocity.y < 0;
        if (moving)
            PlayerCamera.Instance.ChangeCameraOffset(0.2f, falling, isFacingRight ? 0.8f : -0.8f); // Centers camera a little
        else
            PlayerCamera.Instance.ChangeCameraOffset(0.2f, falling, isFacingRight ? 1 : -1); // Offset camera with character front direction

        // Coyote Time
        if (IsGrounded())
            lastGroundedTime = Time.time;

    }

    // Physic based operations should be called in FixedUpdate(), else hardware can affect Physics (frame drops can skip Update() calls)
    private void FixedUpdate()
    {
        CheckLedgeClimb();
        CheckWallJump();
    }

    private void CheckLedgeClimb()
    {
        // Allow ledgeclimb or we are currently climbing
        if (allowLedgeClimb && !isClimbing)
        {
            // Nasty if combo:
            /* IF
             * body OR feet are tounging wall
             * AND ledge isn't occupied
             * AND ( IsGrounded OR Time - lastGroundedTime is greater than climbTimeBuffer )    This is to prevent walking of a cliff and climbing soon as you step off the ledge
             * AND Time - lastTimeClimbed is greater that climbTimeBuffer                       Prevents climbing super fast
             */

            if ((BodyIsTouchingWall() || FeetAreTouchingWall())
                && !LedgeIsOccupied()
                && Time.time - lastTimeClimbed >= climbTimeBuffer)
            {
                //Debug.Log("Climb start: " + Time.time);
                canClimb = true;
            }

            // We can climb so we climb
            if (canClimb)
            {
                // Do these before animation
                isClimbing = true;
                rb.gravityScale = 0f; // Set to zero because 
                rb.velocity = new Vector2(0, 0); // Set velocity here to zero else movement bugs while climbing

                canMove = false; // Prevent moving while climbing mostly for animations

                lastTimeClimbed = Time.time; // We start climbing set time here

                // START CLIMBING ANIMATION HERE FOR DEMO COROUTINE TO STOP MOVEMENT WHILE CLIMBING
                StartCoroutine(Climb());

                // Start this when climbing animation is completed aka not here
                //LedgeClimb();
            }
        }
    }

    // Simulation of LedgeClimb animation
    private IEnumerator Climb()
    {
        //Debug.Log("Climbing: " + Time.time);
        yield return new WaitForSecondsRealtime(climbTimeBuffer);
        //Debug.Log("Ended Climbing: " + Time.time);
        LedgeClimb();
    }

    // Moves player instantly on top of the ledge he is climbing
    private void LedgeClimb()
    {
        // Move player for offset amount to X and Y directions. X dir will need localScale.x to track where player is looking
        transform.position = new Vector2(transform.position.x + climbXOffset * transform.localScale.x, transform.position.y + climbYOffset);

        rb.gravityScale = defaultGravityScale; // Set this to default here
        canClimb = false; // We cannot climb after before we have checked Raycasts again with new position
        isClimbing = false; // We end climbing
        canMove = true; // We can move again
    }

    // Check if Raycasts hit wall and we are in air to set canWallJump
    private void CheckWallJump()
    {
        // We have wall jumps enabled
        if (allowWallJump)
        {
            // We are in air
            if (!IsGrounded())
            {
                // Feet and LedgeOccupation raycasts detect ground layer obj 
                // We check that we are jumping form different wall or we havent walljumped in a while example: we jumped from left now we check we are trying to jump from right and allow walljump
                if (FeetAreTouchingWall() && LedgeIsOccupied()
                    && (!Mathf.Sign(wallJumpDir).Equals(transform.localScale.x) || wallJumpDir == 0f))
                {
                    canWallJump = true;
                }
                // If we are in air but Raycasts and wall side tests are not going through
                else if (canWallJump)
                {
                    canWallJump = false;
                }
            }
            // We land set wall jump wall direction tracking to zero
            if (IsGrounded())
            {
                wallJumpDir = 0f;
                canWallJump = false;
            }
        }
    }

    // Returns true if Raycast hits to something aka our body is so close to wall that it counts as touching
    private bool BodyIsTouchingWall()
    {
        Debug.DrawRay(wallCheckBody.position, transform.right * checkDistance * transform.localScale.x, Color.red);
        return Physics2D.Raycast(wallCheckBody.position, transform.right * transform.localScale.x, checkDistance, groundLayer); // Raycast from body
    }

    // Returns true if Raycast hits to something aka our feet are so close to wall that it counts as touching
    private bool FeetAreTouchingWall()
    {
        Debug.DrawRay(wallCheckFeet.position, transform.right * checkDistance * transform.localScale.x, Color.red);
        return Physics2D.Raycast(wallCheckFeet.position, transform.right * transform.localScale.x, checkDistance, groundLayer); // Raycast from feet
    }

    // Returns true if Raycast hits to something aka there is something on top of the wall we might be climbing
    private bool LedgeIsOccupied()
    {
        Debug.DrawRay(ledgeCheck.position, transform.right * checkDistance * transform.localScale.x, Color.red);
        // Check if there is something on top of the wall
        return Physics2D.Raycast(ledgeCheck.position, transform.right * transform.localScale.x, checkDistance, groundLayer);
    }

    // Returns true if ground check detects ground
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    // Flips player by changing localScale
    private void Flip()
    {
        // Character flip
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

        if (context.started && canWallJump)
        {
            //Debug.Log("Wall jump");
            // Jumping from left wall
            if (Mathf.Sign(transform.localScale.x) == -1)
            {
                rb.velocity = new Vector2(jumpForce, jumpForce); // add x parameter to jump left or right currently jumps straight up
            }
            // Jumping from right wall
            else if (Mathf.Sign(transform.localScale.x) == 1)
            {
                rb.velocity = new Vector2(-jumpForce, jumpForce); // add x parameter to jump left or right currently jumps straight up
            }
            // Set tracking float here that we jumped from some wall
            wallJumpDir = Mathf.Sign(transform.localScale.x);
        }

        // If button was pressed
        if (context.performed && (Time.time - lastGroundedTime <= coyoteTime) // Check if coyote time is online
            && (Time.time - jumpButtonPressedTime <= coyoteTime) && !isClimbing) // Check if jump has been buffered
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
