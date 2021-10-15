using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Behaviour is currently mostly same as BulletBehaviour. TODO: Player gets stunned when the object is hit.
public class TaserCollisionController : MonoBehaviour
{
    private GameObject target;
    [SerializeField] private GameObject taserTrail;
    private Rigidbody2D rb;
    private Vector2 direction;
    [SerializeField] private float projectileSpeed = 5;
    [SerializeField] private float projectileDamage = 1;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player");
        rb = GetComponent<Rigidbody2D>();
        direction = (target.transform.position - transform.position).normalized;
        Vector2 force = direction * projectileSpeed;
        rb.AddForce(force, ForceMode2D.Impulse);
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void FixedUpdate()
    {
        if ((target.transform.position - transform.position).magnitude > 20)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        direction = collision.GetContact(0).normal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            target.GetComponent<Health>().TakeDamage(projectileDamage); // Player takes damage
            Destroy(gameObject);
        }
        else
        {
            float collisionAngle = Vector2.SignedAngle(Vector2.right, direction);
            Quaternion q = Quaternion.AngleAxis(collisionAngle, Vector3.forward);
            Instantiate(taserTrail, transform.position, q);
            Destroy(gameObject);
        }

    }
}
