using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotSeedController : MonoBehaviour
{
    private GameObject target;
    public GameObject shooter;
    private Rigidbody2D rb;
    private Vector2 direction;
    [SerializeField] private float projectileSpeed = 5;
    [SerializeField] private float projectileDamage = 1;
    private bool reflected = false;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player");
        shooter = GameObject.Find("RiotControlDrone");
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

    private void ReflectBullet()
    {
        target.GetComponent<Shield>().HitWhileParried(); // Tell player parry was successful

        reflected = true;
        target = shooter;
        rb.velocity = Vector2.zero;
        
        direction = (target.transform.position - transform.position).normalized;
        Debug.Log(direction);

        Vector2 force = direction * projectileSpeed;
        rb.AddForce(force, ForceMode2D.Impulse);
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        gameObject.layer = 10;
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
                    target.GetComponent<Health>().TakeDamage(projectileDamage); // Player takes damage
                    Destroy(gameObject);
                }
            }
            else
            {
                target.GetComponent<Health>().TakeDamage(projectileDamage); // Player takes damage
                Destroy(gameObject);
            }
        }
        else if (reflected && collision.collider.CompareTag("Boss"))
        {
            collision.collider.GetComponent<Health>().TakeDamage(projectileDamage); // Enemy takes damage
            Destroy(gameObject);
        }
        else
            Destroy(gameObject);

    }
}
