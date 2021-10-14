using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionDetector : MonoBehaviour
{
    [SerializeField] LayerMask weaponLayer;
    [SerializeField] LayerMask enemyLayer;

    private float timer = 0;
    private bool enemyDetected = false;

    // References
    private Transform player;
    private Collider2D weaponCollider;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        Debug.Log(player.gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1) Destroy(gameObject);

        weaponCollider = Physics2D.OverlapCircle(this.transform.position, 0.5f, weaponLayer);
        if (weaponCollider != null) Debug.Log("Weapon hit!");
            //weaponCollider.transform.gameObject.GetComponent<MeleeWeapon>().ActivatePowerBoost();

        /*
        enemyDetected = Physics2D.OverlapCircle(this.transform.position, 0.5f, enemyLayer);
        if (enemyDetected)
            Debug.Log("Weapon detected!");
        */
    }
}
