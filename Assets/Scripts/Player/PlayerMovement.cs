using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [Header("Player Variables")]
    [SerializeField] private float movementVelocity; // Movement speed variable
    [SerializeField] private float jumpForce; // Jump height variable
    [SerializeField] private float coyoteTime; // Determines coyote time forgiveness
    [SerializeField] private float climbTimeBuffer; // Time when we can climb again
    [SerializeField] private float wallSlideGravityScale;
    [SerializeField] private float shockwaveDiveGravityScale;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // GameObject attached to player that checks if touching ground
    [SerializeField] private float checkRadius; // Determines radius for ground checks
    [SerializeField] LayerMask groundLayer; // Chosen layer that is recognized as ground in ground checks

    [Header("Ledge and Wall Check")]
    [SerializeField] private bool allowLedgeClimb;
    [SerializeField] private bool allowWallJump;
    [SerializeField] private bool allowCoyoteWallJump; // Allows coyoteTime = coyoteTime / 2 to jump from wall (you can move horizontaly or turn around a small distance off the wall and still jump)
    [SerializeField] private bool allowShockwaveJump;
    [SerializeField] private Transform ledgeCheck; // Point where Ledge Check Occupation Raycast is cast should be close to top of head
    [SerializeField] private Transform wallCheckBody; // Point where Body Check Raycast is cast
    [SerializeField] private Transform wallCheckFeet; // Point where Feet Check Raycast is cast
    [SerializeField] private float checkDistance; // Distance of raycast and ledge ClimbLedge() offset positions
    [SerializeField] private float climbXOffset;
    [SerializeField] private float climbYOffset;

    // State variables
    public float horizontal; // Tracks horizontal input direction
    public float horizontalBuffer; // Tracks horizontal input regardles of canReceiveInputMove bool
    private bool moving = false;
    private bool falling = false;
    private bool jumping = false;
    private bool diving = false;
    private bool climbing = false;
    public bool shockwaveJumping = false;
    public bool isFacingRight = true; // Tracks player sprite direction
    public bool jumpInputReceived = false;

    // Last button press times
    private float? jumpButtonPressedTime; // Saves the time when player presses jump button
    private float? lastGroundedTime;
    private float lastTimeClimbed; // This is used to prevent climbing steplike object instantly to the top from first step
    private float lastWallTouchTime;
    private float? lastDiveTime;
    private float? lastLaunchTime;

    // Boost Plant launch
    private bool launched = false;
    private bool wasLaunched = false;
    private Vector3 posBeforeLaunch;
    private float launchDistance;
    private Vector3 launchDirection;

    // Allowance variables
    private bool canClimb = false;
    public bool canReceiveInputMove;
    public bool canReceiveInputJump;
    private bool canShockwaveJump = false;
    private bool canWallJump; // Tells jump function/event that we can jump this is set in CheckWallJump()
    private float wallJumpDir = 0f; // Keeps track if we jump from the left or right (Mathf.Sign() == -1 jumped from left, == 1 jumped from right, == we havent jumped from wall)

    // References
    private Player playerScript;
    private Rigidbody2D rb;
    private float defaultGravityScale;
    private ShockwaveTool shockwaveTool;

    // Others
    RaycastHit2D ledgeHitOffsetRay;

    private void Start()
    {
        Instance = this;

        rb = gameObject.GetComponent<Rigidbody2D>();
        shockwaveTool = gameObject.GetComponentInChildren<ShockwaveTool>();
        playerScript = gameObject.GetComponent<Player>();
        PlayerCamera.Instance.ChangeCameraOffset(0.2f, false, 1);
        defaultGravityScale = rb.gravityScale;

        EnableInputMove();
        EnableInputJump();

        // Setting these to these values give smoother experience on climbing
        climbYOffset = GetComponent<BoxCollider2D>().size.y;
        climbXOffset = GetComponent<BoxCollider2D>().size.x;
    }

    private void Update()
    {

        // Movement
        if (!canReceiveInputMove && horizontal != 0f)
        {
            horizontal = 0f; // Stop moving because we cant give input to horizontal
        }
        // We were stopped by our attack and our horizontalBuffer was not reseted during attack (we want to continue moving to the direction we were pressing before attack)
        else if(canReceiveInputMove && horizontalBuffer != horizontal)
        {
            // Replace with our desired movement if we were pressing move during !canReceiveInputMove
            horizontal = horizontalBuffer;
        }
        rb.velocity = new Vector2(horizontal * movementVelocity, rb.velocity.y); // Moves the player by horizontal input

        /* DISABLED FOR NOW. Launch checks to use when using directional boost plant launch
        if (launched && horizontal != 0)
            rb.velocity = new Vector2(horizontal * movementVelocity, rb.velocity.y); // Lets the player move when launched
        else if (wasLaunched) 
            rb.velocity = new Vector2(movementVelocity, rb.velocity.y); // Don't let player horizontal take all speed
        else if (canMove)
            rb.velocity = new Vector2(horizontal * movementVelocity, rb.velocity.y); // Moves the player by horizontal input
        */

        // Character flip
        if (!isFacingRight && horizontal > 0f) // Flip when turning right
            Flip();
        else if (isFacingRight && horizontal < 0f) // Flip when turning left
            Flip();

        // Ground check to set state variables
        if (IsGrounded())
        {
            shockwaveTool.CancelShockwaveDive(); // Checks if shockwave dive graphics are on and disables them
            diving = false;
            lastGroundedTime = Time.time;
            rb.gravityScale = defaultGravityScale;
        }
        else
            CheckIfStuck();

        // Update movement based state variables
        moving = rb.velocity.x != 0;
        falling = rb.velocity.y < -0.5f;
        jumping = rb.velocity.y > 0.5;

        // Check for idle, falling, jumping, launched and running states
        //if (rb.velocity == Vector2.zero && !climbing) playerScript.SetCurrentState(Player.State.Idle);
        //if (falling && !diving) playerScript.SetCurrentState(Player.State.Falling);
        //if (jumping && !launched) playerScript.SetCurrentState(Player.State.Jumping);
        //if (launched) playerScript.SetCurrentState(Player.State.Launched);
        if (moving && !falling && !jumping && !launched)
        {
            //playerScript.SetCurrentState(Player.State.Running);
            // Offset camera while moving to create a feeling of momentum
            PlayerCamera.Instance.ChangeCameraOffset(0.2f, falling, isFacingRight ? 0.8f : -0.8f); // Centers camera a little
        }
        else
            PlayerCamera.Instance.ChangeCameraOffset(0.2f, falling, isFacingRight ? 1 : -1); // Offset camera with character front direction
    }

    // Physic based operations should be called in FixedUpdate(), else hardware can affect Physics (frame drops can skip Update() calls)
    private void FixedUpdate()
    {
        CheckLedgeClimb();
        CheckWallJump();
        CheckShockwaveJump();
    }

    // Player can sometimes get stuck and not be able to jump because ground check fails
    // This check eliminates those situations and enables jumping even when not grounded if player is clearly stuck
    private void CheckIfStuck()
    {
        if (!climbing && rb.velocity == Vector2.zero)
        {
            lastGroundedTime = Time.time;
            canShockwaveJump = true;
        }
    }

    // ---- LEDGE ----

    private void CheckLedgeClimb()
    {
        // Allow ledgeclimb or we are currently climbing
        if (allowLedgeClimb && !climbing)
        {
            // Nasty if combo:
            /* IF
             * body OR feet are tounging wall
             * AND ledge isn't occupied
             * AND ( IsGrounded OR Time - lastGroundedTime is greater than climbTimeBuffer )    This is to prevent walking of a cliff and climbing soon as you step off the ledge
             * AND Time - lastTimeClimbed is greater that climbTimeBuffer                       Prevents climbing super fast
             */

            if ((BodyIsTouchingWall() /*|| FeetAreTouchingWall()*/)
                && !LedgeIsOccupied()
                && Time.time - lastTimeClimbed >= climbTimeBuffer
                && horizontal != 0) // Player is moving and wanting to climb if no move input fall

            {
                //Debug.Log("Climb start: " + Time.time);
                canClimb = true;
            }

            // We can climb so we climb
            if (canClimb)
            {
                //playerScript.SetCurrentState(Player.State.Climbing);
                // Do these before animation
                climbing = true;
                rb.velocity = new Vector2(0, 0); // Set velocity here to zero else movement bugs while climbing

                canShockwaveJump = false; // Prevent double jumping

                lastTimeClimbed = Time.time; // We start climbing set time here

                // START CLIMBING ANIMATION HERE FOR DEMO COROUTINE TO STOP MOVEMENT WHILE CLIMBING
                StartCoroutine(Climb());

                // Start this when climbing animation is completed aka not here
                //LedgeClimb();
            }
        }
        else if (climbing)
        {
            //playerScript.SetCurrentState(Player.State.Climbing);
            rb.gravityScale = 0f; // Keep gravity at zero so player stays still until climbing is done
            rb.velocity = Vector2.zero;
        }
    }

    // Simulation of LedgeClimb animation
    private IEnumerator Climb()
    {
        //Debug.Log("Climbing: " + Time.time);
        playerScript.SetCurrentState(Player.State.Climbing);

        yield return new WaitForSecondsRealtime(climbTimeBuffer);
        //Debug.Log("Ended Climbing: " + Time.time);
        LedgeClimb();
    }

    // Moves player instantly on top of the ledge he is climbing
    private void LedgeClimb()
    {
        // Move player for offset amount to X and Y directions. X dir will need localScale.x to track where player is looking
        transform.position = new Vector2(transform.position.x + climbXOffset * transform.localScale.x, transform.position.y + climbYOffset - ledgeHitOffsetRay.distance);
        shockwaveTool.CancelShockwaveDive(); // Checks if shockwave dive graphics are on and disables them
        rb.gravityScale = defaultGravityScale; // Set this to default here
        canWallJump = false; // Prevent wall jumps
        canClimb = false; // We cannot climb after before we have checked Raycasts again with new position
        climbing = false; // We end climbing
    }

    // ---- WALLJUMP ----
    
    // Check if Raycasts hit wall and we are in air to set canWallJump
    private void CheckWallJump()
    {
        // We have wall jumps enabled
        if (allowWallJump)
        {
            // We are in air and not climbing
            if (!IsGrounded() && !climbing)
            {
                // Feet and LedgeOccupation raycasts detect ground layer obj 
                // We check that we are jumping form different wall or we havent walljumped in a while example: we jumped from left now we check we are trying to jump from right and allow walljump
                if (FeetAreTouchingWall() && LedgeIsOccupied() && LedgeIsOccupied()
                    && (!Mathf.Sign(wallJumpDir).Equals(transform.localScale.x) || wallJumpDir == 0f))
                {
                    // If we are sliding down a wall and we have gravityscale as default change gravityscale so it feel like there is kitka :) (JOrava EDIT: friction :D)
                    if (!climbing && rb.velocity.y < 0 && rb.gravityScale == defaultGravityScale)
                    {
                        playerScript.SetCurrentState(Player.State.WallSliding);
                        rb.gravityScale = wallSlideGravityScale;
                    }
                    // We are facing to the wall and we can jump off the wall
                    canWallJump = true;
                    // This is used in Wall Jump Coyote time check
                    lastWallTouchTime = Time.time;
                }
                // If we are in air but Raycasts and wall side tests are not going through
                else if (canWallJump)
                {
                    //playerScript.SetCurrentState(Player.State.Jumping);
                    rb.gravityScale = defaultGravityScale;
                    canWallJump = false;
                }
            }
            // We land set wall jump wall direction tracking to zero
            if (IsGrounded())
            {
                rb.gravityScale = defaultGravityScale;
                wallJumpDir = 0f;
                canWallJump = false;
            }
        }
    }

    // ---- SHOCKWAWE ----

    private void CheckShockwaveJump()
    {
        // If player is grounded, double jump is "reset" by changing ShockwaveJumpUsed state to false
        if (IsGrounded()) shockwaveTool.ResetShockwaveJump();

        // If player has not double jumped since last touching the ground, set shockwaveJumping state to false
        if (!shockwaveTool.ShockwaveJumpUsed) shockwaveJumping = false;

        // Lastly, set canShockwaveJump state to true or false
        // Making sure that they are not grounded or climbing so the ability can only be used when appropriate
        // And also making sure shockwaveJump has not yet been used
        canShockwaveJump = (!IsGrounded() && !climbing && !shockwaveTool.ShockwaveJumpUsed) ? true : false;
    }

    // ---- THOUCH / GROUNDED FUNCTIONS ----

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

    // Returns true if Raycast hits to something or OverlapBox overlaps with groundLayer object aka there is something on top of the wall we might be climbing
    private bool LedgeIsOccupied()
    {
        //Ledge check ray
        //Debug.DrawRay(ledgeCheck.position, transform.right * checkDistance * transform.localScale.x, Color.red);
        // ledgeHitOffsetRayRay
        //Debug.DrawRay(ledgeCheck.position + new Vector3(transform.localScale.x * checkDistance, 0f, 0f), -transform.up * transform.localScale.x * (ledgeCheck.position - wallCheckBody.position).magnitude, Color.green);
        if (!Physics2D.Raycast(ledgeCheck.position, transform.right * transform.localScale.x, checkDistance, groundLayer))
        {
            // Ray FROM end of ledgeCheck ray above TO wallCheckBody ray end if groundLayer object is between ray distance is float between [0 , ~ 0.5]
            ledgeHitOffsetRay = Physics2D.Raycast(ledgeCheck.position + new Vector3(transform.localScale.x * checkDistance, 0f, 0f), -transform.up, (ledgeCheck.position - wallCheckBody.position).magnitude, groundLayer);
           
            // Draws a box in scene if objects from groundLayer are inside this box ledge is occupied use ledgeHitOffsetRay to lower box to jsut above object we are climbing
            Collider2D[] colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + climbXOffset * transform.localScale.x, transform.position.y + climbYOffset - ledgeHitOffsetRay.distance), new Vector2(GetComponent<BoxCollider2D>().size.x, GetComponent<BoxCollider2D>().size.y), 0f, groundLayer);
            // No objects in array aka no overlaps with groundLayer objects
            if (colliders.Length == 0)
            {
                // Can climb only when ledgeCheck ray doesn't hit and box doesn't overlap 
                return false;
            }
            // If goes here there was object on top of what we are climbing
        }

        // Default case there is something that block climbing
        return true;     
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

    // ---- LAUNCH ----

    // Launch when player jumps on Boost Plant. Called by BoostPlant-script
    public void ActivateLaunch(float launchDist, Vector2 launchDir)
    {
        if (Time.time - lastDiveTime <= 3)
        {
            // Dived, so activate higher jump
            StartCoroutine(Launch());
            rb.velocity = launchDir * jumpForce * 1.2f;

            // Stop air dive
            shockwaveTool.CancelShockwaveDive();
            diving = false;
            lastDiveTime = null;
        }
        else
            rb.velocity = launchDir * jumpForce * .6f;

        /* DISABLED FOR NOW. USE THESE FOR LAUNCH DIRECTION
        posBeforeLaunch = transform.position;
        launchDistance = launchDist;
        launchDirection = launchDir;
        launched = true;
        */
    }

    // DISABLED FOR NOW. CALL THIS FUNCTION FROM FIXED UPDATE WHEN USING LAUNCH DIRECTIONS
    private void CheckLaunch()
    {
        // If launchDistance reached
        if (launched && (posBeforeLaunch - transform.position).magnitude >= launchDistance)
        {
            launched = false; // Cancel launch
            wasLaunched = true; // Activate post-launch checks in Update
        }
        else if (launched)
        {
            // Keep launch going
            //playerScript.SetCurrentState(Player.State.Launched);
            rb.velocity = new Vector2(launchDirection.x * jumpForce, launchDirection.y * jumpForce);
        }
        // Stop launch when landing or stopping
        // This does not work yet. For some reason, velocity checks fail and this returns true only when landing
        else if (IsGrounded() || rb.velocity.x == 0 || rb.velocity.y == 0) 
            wasLaunched = false;
    }

    private IEnumerator Launch()
    {
        launched = true;

        float launchTimer = 0;
        while (launchTimer <= 0.5f)
        {
            launchTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        launched = false;
    }

    // ---- INPUTS -----

    // Move action: Called when the Move Action Button is pressed
    public void Move(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        // We cant receive input OR we stop
        if (!canReceiveInputMove || Mathf.Abs(context.ReadValue<Vector2>().x) == 0)
        {
            horizontal = 0f;
        }
        // We can move and our input for horizontal is greater than 0f
        else
        {
            horizontal = Mathf.Round(context.ReadValue<Vector2>().x); // Updates the horizontal input direction
        }

        // We need this when we do not want to stop after attacks
        horizontalBuffer = Mathf.Round(context.ReadValue<Vector2>().x);
    }

    // Jump action: Called when the Jump Action button is pressed
    public void Jump(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        if (climbing || !canReceiveInputJump) return;
        jumpButtonPressedTime = Time.time;

        // -WALLJUMP-

        // If button is pressed and we are in allowed walljump position
        if (context.started && canWallJump)
        {
            //playerScript.SetCurrentState(Player.State.Jumping);

            // Use this commented else if, if we want to give player boost to the left or right when walljumping
            // Jumping from left wall
            //if (Mathf.Sign(transform.localScale.x) == -1)
            //{
            //    rb.velocity = new Vector2(jumpForce, jumpForce); // add x parameter to jump left or right currently jumps straight up
            //}
            //// Jumping from right wall
            //else if (Mathf.Sign(transform.localScale.x) == 1)
            //{
            //    rb.velocity = new Vector2(-jumpForce, jumpForce); // add x parameter to jump left or right currently jumps straight up
            //}

            // Comment this if above is used 
            rb.velocity = new Vector2(jumpForce, jumpForce);
            // Set tracking float here that we jumped from some wall
            wallJumpDir = Mathf.Sign(transform.localScale.x);
        }
        // Coyotetime wall jump 
        else if (context.started && allowCoyoteWallJump
            && (Time.time - lastWallTouchTime <= coyoteTime / 2) // With full coyoteTime handling feels weird
            && !LedgeIsOccupied()) // This Check prevents jumping from wall when there is no ground after the wall object and we slide past wall, Coyote time causes unwanted double jump without
        {
            playerScript.SetCurrentState(Player.State.Jumping);

            // Replace or figure this out if else if is used above
            rb.velocity = new Vector2(jumpForce, jumpForce);
            // wallJumpDir here is opposite of opposite :)
            wallJumpDir = Mathf.Sign(-transform.localScale.x);
        }

        // -AIR DIVE-

        // Air dive while in the air
        else if (context.started && !IsGrounded()
            && playerScript.InputVertical == -1)
        {
            if (!(playerScript.GetCurrentState() == Player.State.Diving)) // First frame of diving
                rb.velocity = new Vector2(0, -jumpForce); // Set velocity downwards

            playerScript.SetCurrentState(Player.State.Diving);
            diving = true;
            lastDiveTime = Time.time;

            // Air Dive functionality here
            shockwaveTool.DoShockwaveDive(); // Activate VFX
            rb.gravityScale = shockwaveDiveGravityScale;
        }

        // -DOUBLE JUMP-

        // Double jump while in the air
        else if (allowShockwaveJump && canShockwaveJump) // Make sure player has acquired Shockwave Jump and that they can currently double jump
        {
            // If button is pressed and player has not yet double jumped
            if (context.started && !shockwaveJumping
                && !(Time.time - lastGroundedTime <= coyoteTime)) // Check if coyote time is online (if yes, no double jump needed)
            {
                playerScript.SetCurrentState(Player.State.Jumping);
                shockwaveTool.CancelShockwaveDive(); // Checks if shockwave dive graphics are on and disables them

                // Activate the event through the ShockwaveTool script and do a double jump
                shockwaveTool.ShockwaveJump(); // Activates VFX
                rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Jump in the air

                // Update ShockwaveJump state variables after the jump
                shockwaveJumping = true;
                canShockwaveJump = false;

                // Reset Jump state variables
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
        }

        // -JUMP FROM GROUND-

        // If button was pressed
        if (context.performed && (Time.time - lastGroundedTime <= coyoteTime) // Check if coyote time is online
            && (Time.time - jumpButtonPressedTime <= coyoteTime) && !climbing) // Check if jump has been buffered
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Keep player in upwards motion
        }

        // If button was released
        if (context.canceled && rb.velocity.y > 0f && !shockwaveTool.ShockwaveJumpUsed && !launched)
        {
            // Check that player is currently not being launched
            if (Time.time - lastLaunchTime > 1 || lastLaunchTime == null)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f); // Slow down player
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
            lastLaunchTime = null;
        }
    }

    // ---- Input Enable/Disable functions ----

    public void EnableInputMove()
    {
        canReceiveInputMove = true;
    }
    public void DisableInputMove()
    {
        canReceiveInputMove = false;
    }
    public void EnableInputJump()
    {
        canReceiveInputJump = true;
    }
    public void DisableInputJump()
    {
        canReceiveInputJump = false;
    }


    // ---- OTHER ----

    // Activated from pick up
    public void AllowWallJump()
    {
        allowWallJump = true;
    }

    public bool getAllowWallJump()
    {
        return allowWallJump;
    }
}
