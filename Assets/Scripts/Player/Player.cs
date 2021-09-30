using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // This script can be used to control all player sub scripts through one place.
    // Not in use right now because the new Unity Input System does all the work for us. (Input does not need to be tracked separately)

    // This script could maybe become useful when starting to make animation controller. 
    // But maybe even then a separate animation controller script could be enough by itself.

    [Header("Player Sub Scripts")]
    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private PlayerCollision collisionScript;
    [SerializeField] private PlayerInteractions interactionsScript;
    [SerializeField] private PlayerCombat combatScript;

    // Component references
    private Animator animator;
    private Rigidbody2D rb;

    // Other references
    private string currentState;


}