using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;
    public GameObject shooter; // Set by the shooter
    private GameObject target;
    public float bulletSpeed = 5;
    private bool reflected = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.Find("Player");

        Physics2D.IgnoreLayerCollision(7, 9, true);

        Vector2 force = (target.transform.position - transform.position).normalized * bulletSpeed;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if((target.transform.position - transform.position).magnitude > 20)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (target.TryGetComponent(out Shield shield))
            {
                if (shield.Parrying)
                    ReflectBullet();
                else
                {
                    target.GetComponent<Health>().TakeDamage(4); // Player takes damage
                    Destroy(gameObject);
                }
            }
            else
            {
                target.GetComponent<Health>().TakeDamage(4); // Player takes damage
                Destroy(gameObject);
            }
        }
        else if (reflected && collision.collider.CompareTag("Enemy"))
        {
            collision.collider.GetComponent<Health>().TakeDamage(4); // Enemy takes damage
            Destroy(gameObject);
        }
        else
            Destroy(gameObject);

    }

    private void ReflectBullet()
    {
        target.GetComponent<Shield>().HitWhileParried(); // Tell player parry was successful

        reflected = true;
        target = shooter;
        rb.velocity = Vector2.zero;
        Vector2 force = (target.transform.position - transform.position).normalized * bulletSpeed;
        rb.AddForce(force, ForceMode2D.Impulse);

        gameObject.layer = 10;
    }
}
