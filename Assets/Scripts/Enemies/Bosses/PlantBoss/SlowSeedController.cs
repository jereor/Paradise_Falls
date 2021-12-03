using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SlowSeedController : MonoBehaviour
{
    public GameObject shooter;
    private PlayerHealth targetHealth;
    [SerializeField] private float projectileSpeed = 5;
    [SerializeField] private float projectileDamage = 1;
    [SerializeField] private float rotateSpeed = 1;

    void Start()
    {
        shooter = GameObject.Find("PlantBoss");
        targetHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
        transform.GetChild(0).DOShakePosition(90, 0.2f, 2, 20, false, false);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.rotation = Quaternion.Euler(0, 0, rotateSpeed * Time.deltaTime);
        transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));
        transform.Translate(-Vector2.up * Time.deltaTime * projectileSpeed, Space.World);
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
