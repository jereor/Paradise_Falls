using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;
    public GameObject shooter; // Set by the shooter
    private GameObject target;
    public float bulletSpeed = 5;
    public float bulletDamage = 1f;
    private bool reflected = false;
    public bool staticShot = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.Find("Player");

        if (staticShot)
        {
            Vector3 vectorToTarget = shooter.transform.position - shooter.transform.parent.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = q;
            Vector2 force = shooter.transform.right.normalized * bulletSpeed;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
        else
        {
            Vector3 vectorToTarget = target.transform.position - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = q;
            Vector2 force = (target.transform.position - transform.position).normalized * bulletSpeed;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
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
                    target.GetComponent<Health>().TakeDamage(bulletDamage); // Player takes damage
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log("player hit");
                target.GetComponent<Health>().TakeDamage(bulletDamage); // Player takes damage
                Destroy(gameObject);
            }
        }
        else if (reflected && collision.collider.CompareTag("Enemy"))
        {
            collision.collider.GetComponent<Health>().TakeDamage(bulletDamage); // Enemy takes damage
            Destroy(gameObject);
        }
        else
            Destroy(gameObject);

    }

    private void ReflectBullet()
    {
        target.GetComponent<Shield>().HitWhileParried(); // Tell player parry was successful

        if (shooter == null)
        {
            Destroy(gameObject);
        }
        else
        {
            reflected = true;
            target = shooter;
            transform.Rotate(0, 0, 180);
            rb.velocity = Vector2.zero;
            Vector2 force = (target.transform.position - transform.position).normalized * bulletSpeed;
            rb.AddForce(force, ForceMode2D.Impulse);

            gameObject.layer = 10;
        }
    }
}
