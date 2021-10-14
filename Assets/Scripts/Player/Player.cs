using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// This script can be used to control all player sub scripts through one place.
// Holds player components and state variables that can be accessed from anywhere
public class Player : MonoBehaviour
{
    public static Player Instance;

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

    // Component references
    public Animator animator;
    public Rigidbody2D rb;

    // If state is changed this will be true so we know in FixedUpdate to HandleStateInput() only when we change state
    // If this is not used HandleStateUpdate() will be called every FixedUpdate() call aka Disables and Enables are done each FixedUpdate() -> potatocomputers cannot run our game
    private bool statesChanged = false;

    public enum State
    {
        Idle,
        Running,
        Ascending,
        Falling,
        Landing,
        Diving,
        WallSliding,
        Climbing,
        Attacking,
        AttackTransition,
        Aiming,
        Throwing
    }
    State currentState;
    State previousState;

    private void Start()
    {
        Instance = this;

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
        if(!PauseMenuController.GameIsPaused && statesChanged)
            HandleStateInputs();

        HandleAnimations();
    }

    // Most of the animations are started from this function some are in JumpScript, Jump animation blendtree
    private void HandleAnimations()
    {

        // When currentState is Idle
        if(currentState == State.Idle)
        {
            // Player melees light attack and we arent currently climbing
            if (PlayerCombat.Instance.meleeInputReceived && !PlayerCombat.Instance.heavyHold && !animator.GetBool("isClimbing"))
            {
                animator.Play("LAttack1");
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
            if (PlayerCombat.Instance.meleeInputReceived && !PlayerCombat.Instance.heavyHold && !animator.GetBool("isClimbing"))
            {
                animator.Play("LAttack1");
            }
            // Throw
            if (PlayerCombat.Instance.throwInputReceived)
            {
                animator.Play("Throw");
            }
        }

        // LedgeClimb animation
        // LedgeChecks return true
        if (movementScript.getClimbing() && !animator.GetBool("isAttacking") && !animator.GetBool("isAiming"))
        {
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
        // We are in air and we land with rb velocity downwards or zero 
        if (animator.GetBool("jump") && movementScript.IsGrounded() && rb.velocity.y <= 0f)
        {
            animator.SetBool("jump", false);
        }
        // We are in air and we are currently moving upwards or downwards
        else if (!movementScript.IsGrounded() && rb.velocity.y != 0f)
        {
            // If this is false set it to true since we are either jumping or falling
            if(!animator.GetBool("jump"))
                animator.SetBool("jump", true);
            // Update float yVelocity to be used in blend tree 
            // if negative value -> falling(decreasing) animation OR if positive/zero -> jumping(ascending) animation 
            animator.SetFloat("yVelocity", rb.velocity.y);
        }

        // Aiming
        //UNCOMMENT WHEN getThrowAiming() created in PlayerCombat
        if (combatScript.getThrowAiming() && combatScript.getWeaponWielded() && !animator.GetBool("isRunning") && !animator.GetBool("isClimbing"))
        {
            animator.SetBool("isAiming", true);
        }
        else
        {
            animator.SetBool("isAiming", false);
        }
    }

    // Enables and Disables inputs
    private void HandleStateInputs()
    {
        //Debug.Log("State update frequency");
        switch (currentState)
        {
            case State.Idle:
                // Combat
                combatScript.EnableInputMelee();
                combatScript.EnableInputThrowAim();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;
            case State.Running:
                // Combat
                combatScript.EnableInputMelee();
                combatScript.EnableInputThrowAim();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;
            case State.Ascending:
                break;
            case State.Falling:
                break;
            case State.Landing:
                // Combat
                combatScript.DisableInputMelee();
                combatScript.DisableInputThrowAim();

                // Movement
                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;
            case State.Diving:
                break;
            case State.WallSliding:
                // Combat
                combatScript.DisableInputMelee();
                combatScript.EnableInputThrowAim();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;
            case State.Climbing:
                // Combat
                combatScript.DisableInputMelee();
                combatScript.DisableInputThrowAim();

                // Movement
                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;
            case State.Attacking:
                // Combat
                combatScript.DisableInputThrowAim();

                // Movement
                movementScript.DisableInputJump();
                movementScript.DisableInputMove();
                break;
            case State.AttackTransition:
                // Combat
                combatScript.EnableInputMelee();
                combatScript.EnableInputThrowAim();

                // Movement
                movementScript.EnableInputJump();
                movementScript.EnableInputMove();
                break;
            case State.Aiming:
                break;
            case State.Throwing:
                break;
            default:
                break;
        }
        // We updated states -> we can set this to false -> no need to check again if we stay in same state
        statesChanged = false;
    }

    // Called from PauseMenuController on Pause() and Resume()
    // Add enables and disables as they are made in scripts ---- CURRENT: melee, aim, jump, move
    public void HandleAllPlayerControlInputs(bool activate)
    {
        if (!activate)
        {
            // Combat
            combatScript.DisableInputMelee();
            combatScript.DisableInputThrowAim();

            // Movement
            movementScript.DisableInputJump();
            movementScript.DisableInputMove();
        }
        else
        {
            // Combat
            combatScript.EnableInputMelee();
            combatScript.EnableInputThrowAim();

            // Movement
            movementScript.EnableInputJump();
            movementScript.EnableInputMove();
        }
    }

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

    public bool IsFacingRight()
    {
        return movementScript.isFacingRight;
    }

    public bool IsShockwaveJumping()
    {
        return movementScript.shockwaveJumping;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 100), "Health: " + healthScript.GetHealth(), TextStyleHealth);
        GUI.Label(new Rect(200, 10, 300, 100), "Energy: " + energyScript.GetEnergy(), TextStyleEnergy);

        GUI.Label(new Rect(700, 10, 300, 100), "Currently: " + currentState, TextStyleHealth);
        GUI.Label(new Rect(1000, 10, 300, 100), "Previously: " + previousState, TextStyleEnergy);

        GUI.Label(new Rect(1500, 10, 300, 100), "Horizontal: " + movementScript.horizontal, TextStyleEnergy);
    }

    // Move action: Called when the Move Action Button is pressed
    public void Move(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        InputHorizontal = Mathf.Round(context.ReadValue<Vector2>().x); // Updates the horizontal input direction
        InputVertical = Mathf.Round(context.ReadValue<Vector2>().y);// Updates the vertical input direction
    }
}