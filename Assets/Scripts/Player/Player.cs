using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Sub Scripts")]
    [SerializeField] private PlayerInput inputScript;
    [SerializeField] private PlayerMovement movementScript;
    [SerializeField] private PlayerCollision collisionScript;

    [Header("Player Properties")]
    [SerializeField] private float movementSpeed;

    [Header("Component References")]
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Other References")]
    private string currentState;


}
