using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Sub Scripts")]
    [SerializeField] private PlayerInput inputScript;
    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private PlayerCollision collisionScript;
    [SerializeField] private PlayerInteractions interactionsScript;

    // Component references
    private Animator animator;
    private Rigidbody2D rb;

    // Other references
    private string currentState;


}
