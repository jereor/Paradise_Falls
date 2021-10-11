using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ParticleCollision : MonoBehaviour
{
    [SerializeField] float knockbackForce;

    private ParticleSystem ps;
    public List<ParticleCollisionEvent> collisionEvents;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var numCollisionEvents = ps.GetCollisionEvents(collision, collisionEvents);
            var direction = collision.transform.position - transform.position;

            collision.GetComponent<Rigidbody2D>().AddForceAtPosition(direction.normalized * knockbackForce, collisionEvents[0].intersection, ForceMode2D.Impulse);

            if (collision.TryGetComponent(out GroundEnemyAI enemyScript))
            {
                //enemyScript.KnockBackEnemy(transform.gameObject , knockbackForce);
            }

        }
    }
}
