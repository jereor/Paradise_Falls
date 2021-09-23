using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInput inputScript;

    [Header("Player Variables")]
    [SerializeField] private float horizontal;
    [SerializeField] private float movementVelocity;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool isFacingRight = true;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius;
    [SerializeField] LayerMask groundLayer;

    // References
    private Rigidbody2D rb;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        rb.velocity = new Vector2(horizontal * movementVelocity, rb.velocity.y);

        if (!isFacingRight && horizontal > 0f)
            Flip();
        else if (isFacingRight && horizontal < 0f)
            Flip();
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        if (context.canceled && rb.velocity.y > 0f)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
    }
}
