using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player playerScript;
    [SerializeField] private PlayerInput inputScript;

    [Header("Player Properties")]
    [SerializeField] private float movementVelocity;

    // Other references
    public InputActions controls;

    private void Start()
    {
        controls = inputScript.controls;
    }

    private void Update()
    {
        float movement = controls.Player.Movement.ReadValue<float>();
        if (movement < 0) transform.Translate(Vector2.left * Time.deltaTime * movementVelocity);
        if (movement > 0) transform.Translate(Vector2.right * Time.deltaTime * movementVelocity);
    }
}
