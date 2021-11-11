using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private float speed;

    [Header("Collider from sprites")]
    [SerializeField] private Collider2D rightEndPointCollider; // Only one will be enough

    [SerializeField] private Animator[] animators;

    private bool stopped = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("MeleeWeapon")) && !stopped)
        {
            // Move collided object with set speed using parents locals scale to change direction smoothly
            collision.transform.Translate(new Vector2(speed, 0f) * transform.parent.localScale.x * Time.deltaTime);
        }
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
