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

    public enum State
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Diving,
        WallSliding,
        Climbing,
        Launched,
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
        HandleStateInputs();


        HandleAnimations();
    }

    private void HandleAnimations()
    {
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
        // If we are jumping we check when we land with negative or zero y velo to set jump boolean false aka landed
        if (animator.GetBool("jump") && movementScript.IsGrounded() && rb.velocity.y <= 0f)
        {
            animator.SetBool("jump", false);
        }
        else if (!animator.GetBool("jump") && !movementScript.IsGrounded() && rb.velocity.y != 0f)
        {
            animator.SetBool("jump", true);
        }

        // Aiming
        // UNCOMMENT WHEN getThrowAiming() created in PlayerCombat
        //if (combatScript.getThrowAiming() && combatScript.getWeaponWielded() && !animator.GetBool("isRunning"))
        //{
        //    animator.SetBool("isAiming", true);
        //}
        //else
        //{
        //    animator.SetBool("isAiming", false);
        //}
    }

    private void HandleStateInputs()
    {
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
            case State.Jumping:
                animator.SetFloat("yVelocity", rb.velocity.y);
                break;
            case State.Falling:
                animator.SetFloat("yVelocity", rb.velocity.y);
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
            case State.Launched:
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