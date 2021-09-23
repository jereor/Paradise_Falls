using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInput inputScript;

    [Header("Player Variables")]
    [SerializeField] private float movementVelocity;
    [SerializeField] private float jumpForce;
    /*[SerializeField] private float jumpTimeCounter;
    [SerializeField] private float jumpTime;
    [SerializeField] private bool isJumping;*/

    [Header("Ground Check")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private Transform feetPos;
    [SerializeField] private float checkRadius;
    [SerializeField] LayerMask whatIsGround;

    // References
    private Rigidbody2D rb;
    public InputActions controls;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        controls = inputScript.controls;
        controls.Player.Jump.performed += _ => Jump();
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(feetPos.position, checkRadius, whatIsGround);

        float movement = controls.Player.Movement.ReadValue<float>();
        if (movement < 0) transform.Translate(Vector2.left * Time.deltaTime * movementVelocity);
        if (movement > 0) transform.Translate(Vector2.right * Time.deltaTime * movementVelocity);
    }

    public void Jump()
    {
        if (isGrounded)
            rb.velocity = Vector2.up * jumpForce;
    }
}
