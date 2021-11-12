using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private float speed;

    [Header("Used if button/lever can shut belt off")]
    [Tooltip("Collider from last right sprite")]
    [SerializeField] private Collider2D rightEndPointCollider; // Only one will be enough

    [SerializeField] private Animator[] animators;

    private bool stopped = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Allows player to jump if landed on last part of belt
        if (collision.gameObject.CompareTag("Player") && !stopped)
            rightEndPointCollider.gameObject.layer = LayerMask.NameToLayer("Ground");
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("MeleeWeapon")) && !stopped)
        {
            // Move collided object with set speed using parents locals scale to change direction smoothly
            if (collision.gameObject.CompareTag("MeleeWeapon"))
            {
                // Weapon will rotate in air we need to check if it points to left or right and apply speed correctly
                if (collision.gameObject.transform.right.x < 0f)
                {
                    collision.transform.Translate(new Vector2(-speed, 0f) * transform.parent.localScale.x * Time.deltaTime);
                }
                else
                {
                    collision.transform.Translate(new Vector2(speed, 0f) * transform.parent.localScale.x * Time.deltaTime);
                }
            }
            else
                collision.transform.Translate(new Vector2(speed, 0f) * transform.parent.localScale.x * Time.deltaTime);
        }
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Change layer back to normal
        if (collision.gameObject.CompareTag("Player") && !stopped)
            rightEndPointCollider.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void Stop()
    {
        // If not stopped stop conveyor belt
        if (!stopped)
        {
            stopped = true;

            // Stop animations by disabling animators
            foreach (Animator anim in animators)
            {
                anim.enabled = false;
            }
            // Set collider object to ground layer since conveyor belt is stopped and we should be able to climb
            rightEndPointCollider.gameObject.layer = LayerMask.NameToLayer("Ground");
        }
        else
        {
            stopped = false;

            foreach (Animator anim in animators)
            {
                anim.enabled = true;
            }
            rightEndPointCollider.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}
