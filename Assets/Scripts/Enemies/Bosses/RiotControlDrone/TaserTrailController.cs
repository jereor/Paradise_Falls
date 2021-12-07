using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaserTrailController : MonoBehaviour
{
    private GameObject target;
    private float lifeTimer = 0;
    [SerializeField] private float lifeTime = 1;
    [SerializeField] private float damageToPlayer = 1;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player");
    }

    void FixedUpdate()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            target.GetComponent<Health>().TakeDamage(damageToPlayer); // Player takes damage
            Destroy(gameObject);
        }

    }
}
