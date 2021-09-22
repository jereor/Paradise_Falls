using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Player playerScript;

    // Component references
    private Rigidbody2D rb;

    // Other references
    public InputActions controls;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new InputActions();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        controls.Player.Jump.performed += _ => Jump();
    }

    public void Jump()
    {
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
    }
}
