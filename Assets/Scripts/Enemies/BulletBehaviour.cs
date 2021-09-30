using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject target;
    public float bulletSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.Find("Player");

        Physics2D.IgnoreLayerCollision(7, 9);

        Vector2 force = (target.transform.position - transform.position).normalized * bulletSpeed;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            target.GetComponent<Health>().TakeDamage(1);
            Destroy(gameObject);
        }
        else
            Destroy(gameObject);

        
    }
}
