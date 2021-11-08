using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// This script can be used to control all player sub scripts through one place.
// Holds player components and state variables that can be accessed from anywhere
public class Player : MonoBehaviour
{
    public static Player Instance;
    
    // These inputs can be checked from anywhere. Mainly to be used in PlayerMovement.
    public float InputHorizontal { get; private set; } 
    public float InputVertical { get; private set; }

    private GUIStyle TextStyleEnergy = new GUIStyle();
    private GUIStyle TextStyleHealth = new GUIStyle();

    [Header("Player Sub Scripts")]
    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private PlayerCollision collisionScript;
    [SerializeField] private PlayerInteractions interactionsScript;
    [SerializeField] private PlayerCombat combatScript;
    [SerializeField] private ShockwaveTool shockwaveTool;
    [SerializeField] private Health healthScript;
    [SerializeField] private Energy energyScript;
    [SerializeField] private Shield shieldScript;

    // Unlocked abilities, false by default and unlocked by progressing in the game
    [Header("Player abilities")]
    [SerializeField] private bool shieldUnlocked = false;
    [SerializeField] private bool multitoolUnlocked = false;
    [SerializeField] private bool walljumpUnlocked = false;
    [SerializeField] private bool grapplingUnlocked = false;
    [SerializeField] private bool dashUnlocked = false;
    [SerializeField] private bool shockwaveJumpAndDiveUnlocked = false;
    [SerializeField] private bool shockwaveAttackUnlocked = false;
    [SerializeField] private bool shieldGrindUnlocked = false;

    // Component references
    public Animator animator;
    public Rigidbody2D rb;

    // If state is changed this will be true so we know in FixedUpdate to HandleStateInput() only when we change state
    // If this is not used HandleStateUpdate() will be called every FixedUpdate() call aka Disables and Enables are done each FixedUpdate() -> potatocomputers cannot run our game
    private bool statesChanged = false;

    // SerField for debugs 
    [SerializeField] private bool inputsActive = true; // Boolean to be triggered when Handle

    private Coroutine blockCoroutine;
    [SerializeField] private float blockAnimTimeMultiplier; // this times block anim time = when to activate shield object, good values 1.1 - 1.5

    public enum State
    {
        Idle,
        Running,
        Ascending,
        Falling,
        Landing,
        WallSliding,
        Climbing,
        Attacking,
        AttackTransition,
        HeavyCharge,
        HeavyHold,
        Aiming,
        Throwing,
        Blocking,
        Parrying
    }
    State currentState;
    State previousState;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentState = State.Idle;
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();

        TextStyleHealth.normal.textColor = Color.green;
        TextStyleEnergy.fontSize = 30;
        TextStyleHealth.fontSize = 30;
        TextStyleEnergy.normal.textColor = Color.red;
    }

    private void FixedUpdate()
    {
        // If game is not paused and state has changed handle state inputs
        if(inputsActive && statesChanged)
            HandleStateInputs();

        HandleAnimations();
    }

    // Most of the animations are started from this function some are in JumpScript, Jump animation blendtree
    private void HandleAnimations()
    {
        // When currentState is Idle
        if(currentState == State.Idle)
        {
            // Player melees light attack and we arent currently climbing and combo is not on cooldown
            if (combatScript.meleeInputReceived && !combatScript.heavyHold && !animator.GetBool("isClimbing") && !animator.GetBool("hCharging") && !combatScript.getComboOnCooldown())
            {
                // Combo is not active we set it to active since we attack
                if (!combatScript.getComboActive())
                    combatScript.setComboActive(true);

                switch (combatScript.getCurrentComboIndex())
                {
                    case 0:
                        animator.Play("LAttack1");       
                        break;
                    case 1:
                        animator.Play("LAttack2");
                        // Stop this for the duration of LAttack2 clip LTran2 will start timer again when clip starts
                        combatScript.StopComboTimer();
                        break;
                    case 2:
                        animator.Play("LAttack3");
                        // Stop this so cooldown starts after LTran3 clip has "played" or should have played if we started running during the clip
                        combatScript.StopComboTimer();
                        break;
                    default:
                        break;
                }
            }
            // Heavy attack
            if (combatScript.meleeInputReceived && combatScript.heavyHold && !animator.GetBool("isClimbing") && !animator.GetBool("hCharging") && movementScript.IsGrounded())
            {
                animator.Play("HCharge");
            }
            // Player starts moving
            if (PlayerMovement.Instance.horizontal != 0f)
            {
                animator.SetBool("isRunning", true);
            }
            // Throw
            if (PlayerCombat.Instance.throwInputReceived)
            {
                animator.Play("Throw");
            }
        }

        // When currentState is Running
        if (currentState == State.Running)
        {
            // Input is false we arent giving input
            if (PlayerMovement.Instance.horizontal == 0f)
            {
                animator.SetBool("isRunning", false);
            }
            // From running to Light attack
            if (combatScript.meleeInputReceived && !combatScript.heavyHold && !animator.GetBool("isClimbing") && !combatScript.getComboOnCooldown())
            {
                if (!combatScript.getComboActive())
                    combatScript.setComboActive(true);

                switch (combatScript.getCurrentComboIndex())
                {
                    case 0:
                        animator.Play("LAttack1");
                        break;
                    case 1:
                        animator.Play("LAttack2");
                        combatScript.StopComboTimer();
                        break;
                    case 2:
                        animator.Play("LAttack3");
                        combatScript.StopComboTimer();
                        break;
                    default:
                        break;
                }
            }
            // Heavy attack
            if (combatScript.meleeInputReceived && combatScript.heavyHold && !animator.GetBool("isClimbing") && !animator.GetBool("hCharging") && movementScript.IsGrounded())
            {
                animator.Play("HCharge");
            }
            // Throw
            if (PlayerCombat.Instance.throwInputReceived)
            {
                animator.Play("Throw");
            }
        }

        // When current state is attack transition
        if(currentState == State.AttackTransition)
        {
            // Player starts moving
            if (PlayerMovement.Instance.horizontal != 0f)
            {
                animator.SetBool("isRunning", true);
            }
            // From transition to Light attack
            if (combatScript.meleeInputReceived && !combatScript.heavyHold && !animator.GetBool("isClimbing") && !combatScript.getComboOnCooldown())
            {
                if (!combatScript.getComboActive())
                    combatScript.setComboActive(true);

                switch (combatScript.getCurrentComboIndex())
                {
                    case 0:
                        animator.Play("LAttack1");
                        break;
                    case 1:
                        animator.Play("LAttack2");
                        combatScript.StopComboTimer();
                        break;
                    case 2:
                        animator.Play("LAttack3");
                        combatScript.StopComboTimer();
                        break;
                    default:
                        break;
                }
            }
            // Heavy attack
            if (combatScript.meleeInputReceived && combatScript.heavyHold && !animator.GetBool("isClimbing") && !animator.GetBool("hCharging") && movementScript.IsGrounded())
            {
                animator.Play("HCharge");
            }
        }

        // When the current state is HeavyCharge
        if(currentState == State.HeavyCharge)
        {
            // We charged until Heavy attack is charged PlayerCombat.cs heavyChargeTime
            if (combatScript.getHeavyBeingCharged() && combatScript.getHeavyCharged())
            {
                animator.Play("HeavyHold");
            }
            // Released alt or mouse right
            else if(!combatScript.getHeavyBeingCharged())
            {
                combatScript.meleeInputReceived = false;
                animator.SetBool("hCharging", false);
            }
        }
        
        // When the current state is HeavyHold
        if (currentState == State.HeavyHold)
        {
            // Relesed alt
            if (!combatScript.heavyHold)
            {
                combatScript.meleeInputReceived = false;
                combatScript.setHeavyCharged(false);
                animator.SetBool("hCharging", false);
            }
            // Released mouse right
            else if (!combatScript.getHeavyBeingCharged() /*&& animator.GetBool("hCharging")*/ && combatScript.getHeavyCharged())
            {
                animator.Play("HAttack");
                animator.SetBool("hCharging", false);
            }

        }


        // When the current state is Ascending or Falling
        if (currentState == State.Ascending || currentState == State.Falling)
        {
            // From ascending or falling to Light attack
            if (combatScript.meleeInputReceived && !combatScript.heavyHold && !animator.GetBool("isClimbing") && !combatScript.getComboOnCooldown())
            {
                if (!combatScript.getComboActive())
                    combatScript.setComboActive(true);

                switch (combatScript.getCurrentComboIndex())
                {
                    case 0:
                        animator.Play("LAttack1");
                        break;
                    case 1:
                        animator.Play("LAttack2");
                        combatScript.StopComboTimer();
                        break;
                    case 2:
                        animator.Play("LAttack3");
                        combatScript.StopComboTimer();
                        break;
                    default:
                        break;
                }

                //if (currentState == State.Ascending)
                //    animator.Play("LAttack2");
                //else if (currentState == State.Falling)
                //    animator.Play("LAttack3");
            }

            // Throw
            if (PlayerCombat.Instance.throwInputReceived)
            {
                animator.Play("Throw");
            }
        }

        // When the current state is Blocking or Parrying
        if (currentState == State.Blocking || currentState == State.Parrying)
        {
            // Player starts moving
            if (PlayerMovement.Instance.horizontal != 0f)
            {
                animator.SetBool("isRunning", true);
            }
        }

        // When the current state is Attacking
        if(currentState == State.Attacking)
        {
            if (movementScript.IsGrounded())
                movementScript.DisableInputMove();
            else
                movementScript.EnableInputMove();
        }

        // When the current state is WallSliding
        if(currentState == State.WallSliding)
        {
            // If we are aiming allow melee to throw
            if (combatScript.getThrowAiming())
                combatScript.EnableInputMelee();
            // If me are not aiming disable melee inputs so we will not melee wall
            else
                combatScript.DisableInputMelee();
        }

        // LedgeClimb animation
        // LedgeChecks return true
        if (movementScript.getClimbing() && !animator.GetBool("isAttacking") && !animator.GetBool("hCharging") && !animator.GetBool("isBlocking") && !animator.GetBool("isParrying"))
        {
            if (animator.GetBool("willLand"))
            {
                animator.SetBool("willLand", false);
                movementScript.setWillLand(false);
            }

            animator.SetBool("isClimbing", true);
        }

        // WallSlide animation
        if (movementScript.getWallSliding() && !movementScript.getClimbing())
        {
            animator.SetBool("isWallSliding", true);
        }
        else
        {
            animator.SetBool("isWallSliding", false);
        }
 
        // Jump / Fall animation
        // We are in air and we land with rb velocity downwards or zero OR we are on moving platform
        if (animator.GetBool("jump") && movementScript.IsGrounded() && rb.velocity.y >= -0.2f && rb.velocity.y <= 0.2f || movementScript.getIfOnMovingPlatform())
        {
            animator.SetBool("jump", false);
            // Landing animation
            if (movementScript.getWillLand())
                animator.SetBool("willLand", true);
            else if (!movementScript.getWillLand() && animator.GetBool("willLand"))
                animator.SetBool("willLand", false);
        }
        // We are in air and we are currently moving upwards or downwards
        else if (!movementScript.IsGrounded() && (rb.velocity.y <= -0.2f || rb.velocity.y >= 0.2f))
        {
            // If this is false set it to true since we are either jumping or falling
            if(!animator.GetBool("jump"))
                animator.SetBool("jump", true);
            // Update float yVelocity to be used in blend tree 
            // if negative value -> falling(decreasing) animation OR if positive/zero -> jumping(ascending) animation 
            animator.SetFloat("yVelocity", rb.velocity.y);
        }

        // Aiming
        if (combatScript.getThrowAiming() && combatScript.getWeaponWielded() && !animator.GetBool("isRunning") && !animator.GetBool("isClimbing") && !animator.GetBool("isBlocking") && !animator.GetBool("isParrying"))
        {
            animator.SetBool("isAiming", true);
        }
        else
        {
            animator.SetBool("isAiming", false);
        }

        // Parry
        if (shieldScript.Parrying && !animator.GetBool("isParrying") && !animator.GetBool("jump") && !animator.GetBool("isAttacking") && !animator.GetBool("isAiming") && !animator.GetBool("isThrowing"))
        {
            float multiplier = GetClipAnimTime("Parry") / shieldScript.getParryTimeWindow();
            // parrySpeedMultiplier is used to scale Parry animation lenght with our set parry time window
            // Multiplier < 1 = animation slows down length increases and Multiplier > 1 animation speeds up length decreses
            // Calculation: x = y * multi -> multi = x / y   || x current clip time , y desired clip time
            animator.SetFloat("parrySpeedMultiplier", multiplier);

            animator.Play("Parry");
            // If we start running during parry we need to keep track of time when we arent parrying anymore since time is set from Shield.cs
            StartCoroutine(ParryCounter(GetClipAnimTime("Parry") * (1 / multiplier)));
        }

        // Blocking 
        if (shieldScript.Blocking && !animator.GetBool("isAttacking") && !animator.GetBool("isAiming") && !animator.GetBool("isThrowing"))
        {
            if (animator.GetBool("isRunning"))
            {
                animator.SetBool("isRunning", false);
            }
            animator.SetBool("isBlocking", true);
            // Shield is not active and coroutine is not started yet
            if(!shieldScript.shield.activeInHierarchy && blockCoroutine == null)
                blockCoroutine = StartCoroutine(ShowBlockObject(GetClipAnimTime("Block") * blockAnimTimeMultiplier));
        }
        else if (!shieldScript.Blocking)
        {
            animator.SetBool("isBlocking", false);
            blockCoroutine = null;
            shieldScript.shield.SetActive(false);
        }
    }

    // Enables and Disables inputs
    private void HandleStateInputs()
    {
        //Debug.Log("State update frequency");
        switch (currentState)
        {
            // ---- IDLE ----
            case State.Idle:
                // Combat
                combatScript.EnableInputMelee();
                combatScript.EnableInputThrowAim();

                shieldScript.EnableInputBlock();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;

            // ---- RUNNING ----
            case State.Running:
                // Combat
                combatScript.EnableInputMelee();
                combatScript.EnableInputThrowAim();

                shieldScript.EnableInputBlock();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;

            // ---- ASCENDING ----
            case State.Ascending:
                // Combat
                combatScript.EnableInputMelee();
                combatScript.EnableInputThrowAim();

                shieldScript.EnableInputBlock();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;

            // ---- FALLING ----
            case State.Falling:
                // Combat
                combatScript.EnableInputMelee();
                combatScript.EnableInputThrowAim();

                shieldScript.EnableInputBlock();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;

            // ---- LANDING ----
            case State.Landing:
                // Combat
                combatScript.DisableInputMelee();
                combatScript.DisableInputThrowAim();

                shieldScript.DisableInputBlock();

                // Movement
                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;

            // ---- WALLSLIDING ----
            case State.WallSliding:
                // Combat
                combatScript.DisableInputMelee();
                combatScript.EnableInputThrowAim();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;

            // ---- CLIMBING ----
            case State.Climbing:
                // Combat
                combatScript.DisableInputMelee();

                shieldScript.DisableInputBlock();

                // Movement
                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;

            // ---- ATTACKING ----
            case State.Attacking:
                // Combat
                combatScript.DisableInputThrowAim();

                // Movement
                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;

            // ---- ATTACKTRANSITION ----
            case State.AttackTransition:
                // Combat
                combatScript.EnableInputMelee();
                combatScript.EnableInputThrowAim();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;

            // ---- HEAVY CHARGE ----
            case State.HeavyCharge:
                // Combat
                combatScript.DisableInputThrowAim();

                shieldScript.DisableInputBlock();

                // Movement
                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;

            // ---- HEAVY HOLD ----
            case State.HeavyHold:
                // Combat
                combatScript.DisableInputThrowAim();

                shieldScript.DisableInputBlock();

                // Movement
                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;

            // ---- AIMING ----
            case State.Aiming:
                break;

            // ---- THROWING ----
            case State.Throwing:
                break;

            // ---- BLOCKING ----
            case State.Blocking:
                // Combat
                combatScript.DisableInputMelee();
                combatScript.DisableInputThrowAim();

                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;

            // ---- PARRYING ----
            case State.Parrying:
                // Combat
                combatScript.DisableInputMelee();
                combatScript.DisableInputThrowAim();

                movementScript.EnableInputMove();
                break;
            default:
                break;
        }
        // We updated states -> we can set this to false -> no need to check again if we stay in same state
        statesChanged = false;
    }

    // Called from PauseMenuController on Pause() and Resume()
    // Add enables and disables as they are made in scripts ---- CURRENT: melee, aim, block, jump, move
    public void HandleAllPlayerControlInputs(bool activate)
    {
        // IF false deactivate and bool to false...
        inputsActive = activate;
        if (!activate)
        {
            // Combat
            combatScript.DisableInputMelee();
            combatScript.DisableInputThrowAim();

            shieldScript.DisableInputBlock();

            // Movement
            movementScript.DisableInputJump();
            movementScript.DisableInputMove();
        }
        else
        {
            // Combat
            combatScript.EnableInputMelee();
            combatScript.EnableInputThrowAim();

            shieldScript.EnableInputBlock();

            // Movement
            movementScript.EnableInputJump();
            movementScript.EnableInputMove();
        }
    }

    // --- GET / SET STATES ---

    public State GetCurrentState()
    {
        return currentState;
    }

    public State GetPreviousState()
    {
        return currentState;
    }

    public void SetCurrentState(State newState)
    {
        if (newState != currentState)
        {
            previousState = currentState;
        }
        // Set this to true so we know in FixedUpdate() that we have to call HandleStateInputs()
        statesChanged = true;
        currentState = newState;
    }

    public bool IsFacingRight()
    {
        return movementScript.isFacingRight;
    }

    public bool IsShockwaveJumping()
    {
        return movementScript.shockwaveJumping;
    }

    public void SetWillLand(bool b)
    {
        animator.SetBool("willLand", b);
    }
    public bool GetWillLand()
    {
        return animator.GetBool("willLand");
    }

    // Used in climbing to check if we are attackin we cant climb
    public bool GetIsAttacking()
    {
        return animator.GetBool("isAttacking");
    }

    public bool GetHCharging()
    {
        return animator.GetBool("hCharging");
    }

    public bool GetIsAiming()
    {
        return combatScript.getThrowAiming();
    }

    public bool GetIsRunning()
    {
        return animator.GetBool("isRunning");
    }

    public bool GetLaunching()
    {
        return animator.GetBool("isLaunching");
    }

    public bool GetIsBlocking()
    {
        return animator.GetBool("isBlocking");
    }

    public bool GetIsParrying()
    {
        return animator.GetBool("isParrying");
    }

    // Returs the lenght of given clip if found
    public float GetClipAnimTime(string name)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == name)
                return clip.length;
        }
        return 0f;
    }

    private IEnumerator ShowBlockObject(float blockAnimTime)
    {
        yield return new WaitForSeconds(blockAnimTime);
        shieldScript.shield.SetActive(true);
    }

    private IEnumerator ParryCounter(float parryTime)
    {
        yield return new WaitForSeconds(parryTime);
        animator.SetBool("isParrying", false);
    }

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(10, 10, 300, 100), "Health: " + healthScript.GetHealth(), TextStyleHealth);
    //    GUI.Label(new Rect(200, 10, 300, 100), "Energy: " + energyScript.GetEnergy(), TextStyleEnergy);

    //    GUI.Label(new Rect(700, 10, 300, 100), "Currently: " + currentState, TextStyleHealth);
    //    GUI.Label(new Rect(1000, 10, 300, 100), "Previously: " + previousState, TextStyleEnergy);

    //    GUI.Label(new Rect(1500, 10, 300, 100), "Gravity: " + rb.gravityScale, TextStyleEnergy);
    //    //GUI.Label(new Rect(1500, 10, 300, 100), "Horizontal: " + movementScript.horizontal, TextStyleEnergy);
    //}

    // Move action: Called when the Move Action Button is pressed
    public void Move(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        InputHorizontal = Mathf.Round(context.ReadValue<Vector2>().x); // Updates the horizontal input direction
        InputVertical = Mathf.Round(context.ReadValue<Vector2>().y);// Updates the vertical input direction
    }

    // --- GET / SET ABILITY UNLOCKS ---
    public bool ShieldUnlocked()
    {
        return shieldUnlocked;
    }
    public void UnlockShield()
    {
        shieldUnlocked = true;
    }

    public bool MultitoolUnlocked()
    {
        return multitoolUnlocked;
    }
    public void UnlockMultitool()
    {
        multitoolUnlocked = true;
    }

    public bool WalljumpUnlocked()
    {
        return walljumpUnlocked;
    }
    public void UnlockWalljump()
    {
        walljumpUnlocked = true;
    }

    public bool GrapplingUnlocked()
    {
        return grapplingUnlocked;
    }
    public void UnlockGrappling()
    {
        grapplingUnlocked = true;
    }

    public bool DashUnlocked()
    {
        return dashUnlocked;
    }
    public void UnlockDash()
    {
        dashUnlocked = true;
    }

    public bool ShockwaveJumpAndDiveUnlocked()
    {
        return shockwaveJumpAndDiveUnlocked;
    }
    public void UnlockShockwaveTool()
    {
        shockwaveJumpAndDiveUnlocked = true;
    }

    public bool ShockwaveAttackUnlocked()
    {
        return shockwaveAttackUnlocked;
    }
    public void UnlockShockwaveAttack()
    {
        shockwaveAttackUnlocked = true;
    }
    public bool ShieldGrindUnlocked()
    {
        return shieldGrindUnlocked;
    }
    public void UnlockShieldGrind()
    {
        shieldGrindUnlocked = true;
    }
}