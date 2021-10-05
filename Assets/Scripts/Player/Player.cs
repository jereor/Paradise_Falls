using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float InputHorizontal { get; private set; }
    public float InputVertical { get; private set; }

    private GUIStyle TextStyleEnergy = new GUIStyle();
    private GUIStyle TextStyleHealth = new GUIStyle();
    // This script can be used to control all player sub scripts through one place.
    // Holds player components and state variables that can be accessed from anywhere

    // This script could maybe become useful when starting to make animation controller. 
    // But maybe even then a separate animation controller script could be enough by itself.

    [Header("Player Sub Scripts")]
    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private PlayerCollision collisionScript;
    [SerializeField] private PlayerInteractions interactionsScript;
    [SerializeField] private PlayerCombat combatScript;
    [SerializeField] private Health healthScript;
    [SerializeField] private Energy energyScript;

    // Component references
    private Animator animator;
    private Rigidbody2D rb;

    // Other references
    private string currentState;

    private void Start()
    {
        TextStyleHealth.normal.textColor = Color.green;
        TextStyleEnergy.fontSize = 30;
        TextStyleHealth.fontSize = 30;
        TextStyleEnergy.normal.textColor = Color.red;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 100), "Health: " + healthScript.GetHealth(), TextStyleHealth);
        GUI.Label(new Rect(200, 10, 300, 100), "Energy: " + energyScript.GetEnergy(), TextStyleEnergy);
        GUI.Label(new Rect(400, 10, 300, 100), "Horizontal: " + InputHorizontal, TextStyleHealth);
        GUI.Label(new Rect(600, 10, 300, 100), "Vertical: " + InputVertical, TextStyleEnergy);
    }

    // Move action: Called when the Move Action Button is pressed
    public void Move(InputAction.CallbackContext context) // Context tells the function when the action is triggered
    {
        InputHorizontal = Mathf.Round(context.ReadValue<Vector2>().x); // Updates the horizontal input direction
        InputVertical = Mathf.Round(context.ReadValue<Vector2>().y);// Updates the vertical input direction
    }
}