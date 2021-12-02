using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedController : MonoBehaviour
{
    private Vector2 lookDirection;
    public GameObject shooter;
    public PlayerHealth targetHealth;
    [SerializeField] private float projectileSpeed = 5;
    [SerializeField] private float projectileDamage = 1;
    // Start is called before the first frame update
    void Start()
    {
        shooter = GameObject.Find("PlantBoss");
        targetHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
        lookDirection = (transform.position - shooter.transform.position).normalized;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * Time.deltaTime * projectileSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            targetHealth.TakeDamage(projectileDamage);
            Destroy(gameObject);
        }
        else
            Destroy(gameObject);
    }
}
