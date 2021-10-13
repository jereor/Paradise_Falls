using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionDetector : MonoBehaviour
{
    [SerializeField] LayerMask weaponLayer;
    [SerializeField] LayerMask enemyLayer;

    private float timer = 0;
    private bool weaponDetected = false;
    private bool enemyDetected = false;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1) Destroy(gameObject);

        weaponDetected = Physics2D.OverlapCircle(this.transform.position, 0.5f, weaponLayer);
        if (weaponDetected)
            Debug.Log("Weapon detected!");

        /*
        enemyDetected = Physics2D.OverlapCircle(this.transform.position, 0.5f, enemyLayer);
        if (enemyDetected)
            Debug.Log("Weapon detected!");
        */
    }
}
