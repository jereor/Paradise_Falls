using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldGrind : MonoBehaviour
{
    [SerializeField] private Transform pipeCheckTransform;
    [SerializeField] private float checkRadius;
    [SerializeField] LayerMask pipeLayer;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] private float speed;

    private List<Collider2D> disabledColliders = new List<Collider2D>();

    public bool shieldGrindUnlocked = false;
    private bool grinding = false;
    public bool jumpButtonPressed = false;

    void FixedUpdate()
    {
        if (shieldGrindUnlocked && PipeCheck())
        {
            PlayerMovement.Instance.DisableInputMove();
            if (!grinding)
                grinding = true;
        }
        if (grinding)
        {
            if (Player.Instance.InputHorizontal > 0f && !PipeCheck())
            {
                PlayerMovement.Instance.setFacingRight(true);
                //PlayerMovement.Instance.DisableInputMove();
            }
            else if (Player.Instance.InputHorizontal < 0f && !PipeCheck())
            {
                PlayerMovement.Instance.setFacingRight(false);
               // PlayerMovement.Instance.DisableInputMove();
            }


            //PlayerMovement.Instance.DisableInputMove();
            transform.Translate(new Vector2(speed, 0f) * transform.localScale.x * Time.deltaTime);
            if(Player.Instance.InputVertical < 0f && jumpButtonPressed && PipeCheck())
            {
                DisableColliders();
            }
        }
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpButtonPressed = true;

        if (context.canceled)
            jumpButtonPressed = false;
    }
    
    private void DisableColliders()
    {
        Collider2D pipeCol = Physics2D.OverlapCircle(pipeCheckTransform.position, checkRadius, pipeLayer);
        Collider2D groundCol = Physics2D.OverlapCircle(pipeCheckTransform.position, checkRadius, groundLayer);

        if (!disabledColliders.Contains(pipeCol))
            disabledColliders.Add(pipeCol);

        if (!disabledColliders.Contains(groundCol))
            disabledColliders.Add(groundCol);

        foreach (Collider2D col in disabledColliders)
        {
            col.enabled = false;
        }

    }

    // Check if we are on top of pipeLayerObject
    public bool PipeCheck()
    {
        return Physics2D.OverlapCircle(pipeCheckTransform.position, checkRadius, pipeLayer);
    }

    // Called from ShieldGrindEndPointTrigegr.cs scripts that are positioned to the end points of pipes in scene
    public void PlayerLeavePipe()
    {
        foreach (Collider2D col in disabledColliders)
        {
            col.enabled = true;
        }
        disabledColliders.Clear();

        grinding = false;
        //PlayerMovement.Instance.EnableInputMove();
    }

    public bool getGrinding()
    {
        return grinding;
    }
}
