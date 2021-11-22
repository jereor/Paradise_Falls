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
    [SerializeField] private float landingMinHeight;
    [SerializeField] private float coyoteTime; // Determines coyote time forgiveness
    [SerializeField] private float climbTimeBuffer; // Time when we can climb again
    [SerializeField] private float wallSlideGravityScale;
    [SerializeField] private float shockwaveDiveGravityScale;
    [SerializeField] private float jumpAndDiveCost;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // GameObject attached to player that checks if touching ground
    [SerializeField] private float checkRadius; // Determines radius for ground checks
    [SerializeField] LayerMask groundLayer; // Chosen layer that is recognized as ground in ground checks
    [SerializeField] LayerMask grapplePointLayer;

    [Header("Ledge and Wall Check")]
    [SerializeField] private bool allowLedgeClimb;
    [SerializeField] private bool allowCoyoteWallJump; // Allows coyoteTime = coyoteTime / 2 to jump from wall (you can move horizontaly or turn around a small distance off the wall and still jump)
    [SerializeField] private Transform ledgeCheck; // Point where Ledge Check Occupation Raycast is cast should be close to top of head
    [SerializeField] private Transform wallCheckBody; // Point where Body Check Raycast is cast
    [SerializeField] private Transform wallCheckFeet; // Point where Feet Check Raycast is cast
    [SerializeField] private float checkDistance; // Distance of raycast and ledge ClimbLedge() offset positions
    [Tooltip("Start update values from boxcollider size")]
    [SerializeField] private float climbXOffset;
    [SerializeField] private float climbYOffset;
    [Tooltip("Assurance value to addt to Y offset to prevent climbing inside a wall")]
    [SerializeField] private float climbYInsuranceValue;

    // State variables
    public float horizontal; // Tracks horizontal input direction
    public float horizontalBuffer; // Tracks horizontal input regardles of canReceiveInputMove bool
    private Vector3 highestPointOfJump = Vector3.zero;

    private bool moving = false;
    private bool falling = false;
    private bool jumping = false;
    private bool diving = false;
    private bool climbing = false;
    private bool currentlyWallSliding = false;
    private bool willLand = false;
    public bool shockwaveJumping = false;
    public bool isFacingRight = true; // Tracks player sprite direction

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
    private Energy energyScript;

    // Others
    RaycastHit2D ledgeHitOffsetRay;
    private Rigidbody2D movingPlatformRB;
    private RaycastHit2D movingPlatformRaycastHit = new RaycastHit2D();

    private void Start()
    {
        Instance = this;

        rb = gameObject.GetComponent<Rigidbody2D>();
        shockwaveTool = gameObject.GetComponentInChildren<ShockwaveTool>();
        playerScript = gameObject.GetComponent<Player>();
        energyScript = gameObject.GetComponent<Energy>();
        PlayerCamera.Instance.ChangeCameraOffset(0.2f, false, 1);
        defaultGravityScale = rb.gravityScale;

        EnableInputMove();
        EnableInputJump();

        // Setting these to these values give smoother experience on climbing
        climbYOffset = GetComponent<BoxCollider2D>().size.y + climbYInsuranceValue;
        climbXOffset = GetComponent<BoxCollider2D>().size.x;
    }

    private void FixedUpdate()
    {
        // Movement
        // We cannot move and horizontal is something else than zero 
        if (!canReceiveInputMove && horizontal != 0f)
        {
            horizontal = 0f; // Stop moving because we update rb.velocity every frame with horizontal (if this logic is changed other scripts might need tweaking -> RB. force, velocity etc.)
        }
        // We were stopped by our attack and our horizontalBuffer was not reseted during attack (we want to continue moving to the direction we were pressing before attack)
        else if (canReceiveInputMove && horizontalBuffer != horizontal)
        {
            // Replace with our desired movement if we were pressing move during !canReceiveInputMove or before action that stops movement
            horizontal = horizontalBuffer;
        }
  
        // Basic movement
        // PlatformMovement() modifies rv.velcity and returns true if player is on platform
        if(!MovingPlatformMovement() && !shockwaveTool.ShockwaveDashUsed && !PlayerCombat.Instance.getIsPlayerBeingPulled() && !ShieldGrind.Instance.getGrinding())
        {
            rb.velocity = new Vector2(horizontal * movementVelocity, rb.velocity.y); // Moves the player by horizontal input
        }

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
            lastGroundedTime = Time.time;
        //else
        //    CheckIfStuck();

        // Update movement based state variables
        moving = rb.velocity.x != 0;
        falling = rb.velocity.y < -0.5f;
        jumping = rb.velocity.y > 0.5;

        CheckIfHardLanding();

        if (moving && !falling && !jumping && !launched)
        {
            // Offset camera while moving to create a feeling of momentum
            PlayerCamera.Instance.ChangeCameraOffset(0.2f, falling, isFacingRight ? 0.8f : -0.8f); // Centers camera a little
        }
        else
            PlayerCamera.Instance.ChangeCameraOffset(0.2f, falling, isFacingRight ? 1 : -1); // Offset camera with character front direction

        CheckLedgeClimb();
        CheckWallJump();
        CheckShockwaveJump();
        CheckShockwaveDive();
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
            rb.velocity = new Vector2(jumpForce, jumpForce);
            // Set tracking float here that we jumped from some wall
            wallJumpDir = Mathf.Sign(transform.localScale.x);
        }
        // Coyotetime wall jump 
        else if (context.started && allowCoyoteWallJump
            && (Time.time - lastWallTouchTime <= coyoteTime / 2) // With full coyoteTime handling feels weird
            && !LedgeIsOccupied()) // This Check prevents jumping from wall when there is no ground after the wall object and we slide past wall, Coyote time causes unwanted double jump without
        {
            // Replace or figure this out if else if is used above
            rb.velocity = new Vector2(jumpForce, jumpForce);
            // wallJumpDir here is opposite of opposite :)
            wallJumpDir = Mathf.Sign(-transform.localScale.x);
        }

        // -AIR DIVE-

        // Air dive while in the air
        else if (context.started && !IsGrounded() && playerScript.ShockwaveJumpAndDiveUnlocked() // Grounded and shockwave tool is unlocked
            && playerScript.InputVertical == -1 // Pressing downwards
            && (Time.time - lastLaunchTime > 0.2f || lastLaunchTime == null)
            && energyScript.CheckForEnergy(jumpAndDiveCost)) // Not just launched
        {
            energyScript.UseEnergy(jumpAndDiveCost);
            shockwaveTool.DoShockwaveDive(); // Activate VFX
            rb.gravityScale = shockwaveDiveGravityScale;
            diving = true;
            lastDiveTime = Time.time;

            if (!(playerScript.GetCurrentState() == Player.State.Falling)) // First frame of diving
                rb.velocity = new Vector2(0, -jumpForce); // Set velocity downwards
        }

        // -DOUBLE JUMP-

        // Double jump while in the air
        else if (playerScript.ShockwaveJumpAndDiveUnlocked() && canShockwaveJump && !diving && energyScript.CheckForEnergy(jumpAndDiveCost) && !shockwaveTool.ShockwaveDashUsed) // Make sure player has acquired Shockwave Jump and that they can currently double jump
        {
            // If button is pressed and player has not yet double jumped
            if (context.started && !shockwaveJumping
                && !(Time.time - lastGroundedTime <= coyoteTime)) // Check if coyote time is online (if yes, no double jump needed)
            {
                energyScript.UseEnergy(jumpAndDiveCost);
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
            && (Time.time - jumpButtonPressedTime <= coyoteTime) && !climbing // Check if jump has been buffered
            && (playerScript.InputVertical != -1 || !playerScript.ShockwaveJumpAndDiveUnlocked())) // Not diving or not able to dive
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Jump!
        }

        // If button was released
        if (context.canceled && rb.velocity.y > 0f && !shockwaveTool.ShockwaveJumpUsed && !launched)
        {
            // Check that player is currently not being launched
            if (Time.time - lastLaunchTime > 1 || lastLaunchTime == null)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f); // Slow down player to start falling
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
            lastLaunchTime = null;
        }
    }

    // Ground check sphere
    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(groundCheck.position, checkRadius);
    //}

    // Check when to update willLand bool to Player.cs can play animation when needed
    private void CheckIfHardLanding()
    {
        // We need new point highest point since: in air, and we are at highest point when velocity.y is between -0.2 and 0.2 we set new position of highestPoint
        // Normal: when jump is at highest point
        // DoubleJump: when we use doublejump and when we reach the highest point of our second upward motion
        if(!IsGrounded() && !climbing && (highestPointOfJump == Vector3.zero || rb.velocity.y >= -0.2f && rb.velocity.y <= 0.2f))
        {
            highestPointOfJump = transform.position;
        }
        // Landed we put this float to 0f
        else if (IsGrounded())
        {
            highestPointOfJump = Vector3.zero;
        }

        // We are diving -> we will land hard
        if (diving)
            willLand = true;

        

        // We double jump to soften our landing -> no land animation
        if (willLand && shockwaveJumping && !IsGrounded() && landingMinHeight > Mathf.Abs((transform.position - highestPointOfJump).y) || currentlyWallSliding)
        {
            willLand = false;
        }
        // HardLanding from dropping from higher than landingMinHeight 3 or greater so we dont land with every jump
        else if (!willLand && highestPointOfJump != Vector3.zero && !currentlyWallSliding && landingMinHeight <= Mathf.Abs((transform.position - highestPointOfJump).y))
        {
            willLand = true;
        }
        
    }

    // ---- Moving Platform movement ----

    // Checks if we are interacting with moving platform(standing, jumping, climbing) and modifies velocity and returns true ELSE does nothing to velocity and returns false
    private bool MovingPlatformMovement()
    {
        // Check if we are on moving platform currently
        if (getMovingPlatformRigidbody() != null && !shockwaveTool.ShockwaveDashUsed)
        {
            // Climbing the platform
            if (getIfClimbingMovingPlatform())
            {
                if (rb.gravityScale != movingPlatformRB.gravityScale)
                {
                    rb.gravityScale = movingPlatformRB.gravityScale;
                }
                rb.velocity = movingPlatformRB.velocity;
            }
            // Jump from platform we need to subtract Y values
            else if (jumpButtonPressedTime != null)
                rb.velocity = movingPlatformRB.velocity + new Vector2(horizontal * movementVelocity, rb.velocity.y - movingPlatformRB.velocity.y);
            // Standing still and moving on moving platform
            else if (IsGrounded())
                rb.velocity = movingPlatformRB.velocity + new Vector2(horizontal * movementVelocity, 0f);
            // Return true to let FixedUpdate() know we did move with platform
            return true;
        }

        // We are not interaction with moving platform return false and move normally
        return false;
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

    // Diving needs to be number one priority, so gravity scale stays the same throughout the whole dive until touching ground again
    private void CheckShockwaveDive()
    {
        // Check if dive is in use and no longer going downwards
        if (rb.velocity.y >= 0 && shockwaveTool.ShockwaveDiveUsed)
        {
            // Cancel dive
            shockwaveTool.CancelShockwaveDive();
            diving = false;
            rb.gravityScale = defaultGravityScale;
        }
    }

    // ---- LEDGE ----

    private void CheckLedgeClimb()
    {
        // Allow ledgeclimb or we are currently climbing
        if (allowLedgeClimb && !climbing)
        {
            // Nasty if combo:
            if ((BodyIsTouchingWall())
                && !LedgeIsOccupied()                                       // Player sees(raycasts) ledge and space is vacant to climb
                && Time.time - lastTimeClimbed >= climbTimeBuffer           // We have to climb stairlike object step by step not instantly to the top
                && horizontal != 0                                          // Player is moving and wanting to climb if no move input fall
                && CheckPlayerFitPlatform()                                 // Check if player fit on top of platform if this is the case
                && !Player.Instance.GetIsAttacking()                        // If we are attacking we cannot climb at the same time
                && !Player.Instance.GetHCharging()
                && !Player.Instance.GetIsBlocking()
                && !Player.Instance.GetIsParrying())
            {
                canClimb = true;
            }

            // We can climb so we climb
            if (canClimb)
            {
                climbing = true;
                rb.velocity = new Vector2(0, 0); // Set velocity here to zero else movement bugs while climbing

                canShockwaveJump = false; // Prevent double jumping

                lastTimeClimbed = Time.time; // We start climbing set time here
            }
        }
        else if (climbing && !getIfClimbingMovingPlatform())
        {
            rb.gravityScale = 0f; // Keep gravity at zero so player stays still until climbing animation is done
            rb.velocity = Vector2.zero;
        }
    }

    // Check if we will fit on top of the platform example: moving upward and ceiling would block our climb or we would be squashed
    public bool CheckPlayerFitPlatform()
    {
        movingPlatformRaycastHit = Physics2D.Raycast(wallCheckBody.position, transform.right * transform.localScale.x, checkDistance + 1f, groundLayer);
        // Check if player fits on top of the moving platform
        if (movingPlatformRaycastHit && movingPlatformRaycastHit.transform.gameObject.CompareTag("MovingPlatform"))
        {
            // 4 different scripts, if script is found check from correct script if not return false
            if(movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out StaticMovingBox staticScript))
            {
                return staticScript.getWillPlayerFit();
            }
            else if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out BackAndForthMovingBox movingScript))
            {
                return movingScript.getWillPlayerFit();
            }
            else if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out StaticMovingPlatform platformScript))
            {
                return platformScript.getWillPlayerFit();
            }
            // Did not find these components we don't really know what we are climbing
            return false;
        }
        // Check grapple point if these before didn't pass
        movingPlatformRaycastHit = Physics2D.Raycast(wallCheckBody.position, transform.right * transform.localScale.x, checkDistance + 1f, grapplePointLayer);

        if (movingPlatformRaycastHit && movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out MovingGrapplePoint grapplePointScript))
        {
            return grapplePointScript.getWillPlayerFit();
        }
        // We are climbing normal ground object we return true
        return true;
    }

    // Moves player instantly on top of the ledge he is climbing
    // Called from LedgeClimbScript OnStateExit() aka animation end
    public void LedgeClimb()
    {
        // Moving platform climb
        if (movingPlatformRaycastHit && movingPlatformRaycastHit.transform.gameObject.CompareTag("MovingPlatform"))
        {
            // Climbing to left side of platform
            if (isFacingRight)
            {
                // 4 different scripts, if script is found check from correct script if not return false
                if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out StaticMovingBox staticScript))
                    transform.position = staticScript.getLeftClimbPos();
                else if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out BackAndForthMovingBox movingScript))
                    transform.position = movingScript.getLeftClimbPos();
                else if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out StaticMovingPlatform platformScript))
                    transform.position = platformScript.getLeftClimbPos();
                else if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out MovingGrapplePoint grapplePointScript))
                    transform.position = grapplePointScript.getLeftClimbPos();
            }
            // Right
            else
            {
                // 4 different scripts, if script is found check from correct script if not return false
                if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out StaticMovingBox staticScript))
                    transform.position = staticScript.getRightClimbPos();
                else if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out BackAndForthMovingBox movingScript))
                    transform.position = movingScript.getRightClimbPos();
                else if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out StaticMovingPlatform platformScript))
                    transform.position = platformScript.getRightClimbPos();
                else if (movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out MovingGrapplePoint grapplePointScript))
                    transform.position = grapplePointScript.getRightClimbPos();
            }
            // Reset so we know next time if we are climbin moving platform (set again in CheckPlayerFitPlatform())
            movingPlatformRaycastHit = new RaycastHit2D();
        }
        else
        {
            // Move player for offset amount to X and Y directions. X dir will need localScale.x to track where player is looking
            transform.position = new Vector2(transform.position.x + climbXOffset * transform.localScale.x, transform.position.y + climbYOffset - ledgeHitOffsetRay.distance);
        }
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
        if (Player.Instance.WalljumpUnlocked())
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
                        // Stop movement so it feels like we are gripping the wall to slow our vertical speed
                        rb.velocity = Vector2.zero;
                        // New scale to drag as slowly downwards, accelerates if wallsliding on same wall for long
                        rb.gravityScale = wallSlideGravityScale;
                    }
                    currentlyWallSliding = true;
                    // We are facing to the wall and we can jump off the wall
                    canWallJump = true;
                    // This is used in Wall Jump Coyote time check
                    lastWallTouchTime = Time.time;
                    // We drop from wall we have momemntun either downwards or we jumped (downward this is our highest point AND jump we will get highest point when we reach it)
                    highestPointOfJump = transform.position;
                }
                // If we are in air but Raycasts and wall side tests are not going through
                else if (canWallJump)
                {
                    currentlyWallSliding = false;
                    if (rb.gravityScale != defaultGravityScale)
                        rb.gravityScale = defaultGravityScale;
                    canWallJump = false;
                }
            }
            // We land set wall jump wall direction tracking to zero
            if (IsGrounded() && !climbing)
            {
                currentlyWallSliding = false;
                if(rb.gravityScale != defaultGravityScale)
                    rb.gravityScale = defaultGravityScale;
                wallJumpDir = 0f;
                canWallJump = false;
            }
        }
    }

    // ---- SHOCKWAVE ----

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
    public bool BodyIsTouchingWall()
    {
        // return true if hit is on groundLayer or grapplePointLayer
        return Physics2D.Raycast(wallCheckBody.position, transform.right * transform.localScale.x, checkDistance, groundLayer) || Physics2D.Raycast(wallCheckBody.position, transform.right * transform.localScale.x, checkDistance, grapplePointLayer) ? true : false; // Raycast from body
    }

    // Returns true if Raycast hits to something aka our feet are so close to wall that it counts as touching
    public bool FeetAreTouchingWall()
    {
        // return true if hit is on groundLayer or grapplePointLayer
        return Physics2D.Raycast(wallCheckFeet.position, transform.right * transform.localScale.x, checkDistance, groundLayer) || Physics2D.Raycast(wallCheckFeet.position, transform.right * transform.localScale.x, checkDistance, grapplePointLayer) ? true : false; // Raycast from feet
    }

    // Returns true if Raycast hits to something or OverlapBox overlaps with groundLayer object aka there is something on top of the wall we might be climbing
    public bool LedgeIsOccupied()
    {
        // DEBUG RAYS IF CHECK BREAKS (takes time to calculate again)
        //Ledge check ray
        //Debug.DrawRay(ledgeCheck.position, transform.right * checkDistance * transform.localScale.x, Color.red);
        // ledgeHitOffsetRayRay
        //Debug.DrawRay(ledgeCheck.position + new Vector3(transform.localScale.x * checkDistance, 0f, 0f), -transform.up * transform.localScale.x * (ledgeCheck.position - wallCheckBody.position).magnitude, Color.green);

        if (!Physics2D.Raycast(ledgeCheck.position, transform.right * transform.localScale.x, checkDistance, groundLayer) || !Physics2D.Raycast(ledgeCheck.position, transform.right * transform.localScale.x, checkDistance, grapplePointLayer))
        {
            // Ray FROM end of ledgeCheck ray above TO wallCheckBody ray end if groundLayer object is between ray distance is float between [0 , ~ 0.5]
            ledgeHitOffsetRay = Physics2D.Raycast(ledgeCheck.position + new Vector3(transform.localScale.x * checkDistance, 0f, 0f), -transform.up, (ledgeCheck.position - wallCheckBody.position).magnitude, groundLayer);
            // If it didn't hit ground object it should hit grapplepoint since these two are only layers you can climb if it will not hit grapplePoint object god help us.
            if(ledgeHitOffsetRay.distance == 0f)
                ledgeHitOffsetRay = Physics2D.Raycast(ledgeCheck.position + new Vector3(transform.localScale.x * checkDistance, 0f, 0f), -transform.up, (ledgeCheck.position - wallCheckBody.position).magnitude, grapplePointLayer);
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

    // Returns true if ground check detects ground + stores information of moving platform gameobject if grounded on it
    public bool IsGrounded()
    {
        Collider2D objGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        Collider2D objGrapplePoint = Physics2D.OverlapCircle(groundCheck.position, checkRadius, grapplePointLayer);
        if (objGround && objGround.gameObject.CompareTag("MovingPlatform"))
            movingPlatformRB = objGround.GetComponent<Rigidbody2D>();
        else if (objGrapplePoint && objGrapplePoint.gameObject.CompareTag("MovingPlatform"))
            movingPlatformRB = objGrapplePoint.GetComponent<Rigidbody2D>();
        else if (!climbing)
            movingPlatformRB = null;

        return objGround || objGrapplePoint ? true : false;
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
        rb.gravityScale = defaultGravityScale; // Reset gravity
        lastLaunchTime = Time.time;

        if (shockwaveTool.ShockwaveDiveUsed)
        {
            // Dived, so activate higher jump
            StartCoroutine(Launch());
            rb.velocity = launchDir * jumpForce * 2;

            // Stop air dive
            shockwaveTool.CancelShockwaveDive();
            diving = false;
            lastDiveTime = null;
        }
        else
            rb.velocity = launchDir * jumpForce;

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


    // ---- OTHERS ---

    public bool getIfOnMovingPlatform()
    {
        return movingPlatformRB;
    }

    public bool getIfClimbingMovingPlatform()
    {
        if (movingPlatformRaycastHit.transform != null && movingPlatformRaycastHit.transform.gameObject.CompareTag("MovingPlatform"))
            return true;

        return false;
    }

    public Rigidbody2D getMovingPlatformRigidbody()
    {
        if (movingPlatformRaycastHit.transform != null && movingPlatformRaycastHit.transform.gameObject.CompareTag("MovingPlatform") && movingPlatformRaycastHit.transform.gameObject.TryGetComponent(out Rigidbody2D rb))
            movingPlatformRB = rb;

        return movingPlatformRB;
    }

    public bool getWillLand()
    {
        return willLand;
    }

    public void setWillLand(bool b)
    {
        if (b)
            highestPointOfJump = Vector3.zero;
        willLand = b;
    }

    public bool getClimbing()
    {
        return climbing;
    }

    public bool getWallSliding()
    {
        return currentlyWallSliding;
    }

    public void setFacingRight(bool b)
    {
        isFacingRight = b;
    }

    public RaycastHit2D getLedgeHitOffsetRay()
    {
        return ledgeHitOffsetRay;
    }
}
