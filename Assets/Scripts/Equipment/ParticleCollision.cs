using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ParticleCollision : MonoBehaviour
{
    [SerializeField] float knockbackForce;

    private bool collisionEnabled = true;

    private void OnParticleCollision(GameObject collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collisionEnabled)
            {
                // knockback enemy
                Knockback(collision.gameObject, gameObject, knockbackForce);
                StartCoroutine(CollisionCooldown(1));
            }
        }

        if (collision.gameObject.CompareTag("MeleeWeapon"))
        {
            if (collisionEnabled)
            {
                // Power Boost the weapon and set it flying forwards
                Knockback(collision.gameObject, gameObject, knockbackForce); // Replace this knock back with a hardcoded cool one
                StartCoroutine(CollisionCooldown(1));
            }
        }
    }

    // Small knockback to the target. Knockback knocks slightly upwards so the friction doesn't stop the target right away.
    private void Knockback(GameObject target, GameObject from, float knockbackForce)
    {
        float pushbackX = target.transform.position.x - from.transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Mathf.Abs(pushbackX / 4)).normalized;
        target.GetComponent<Rigidbody2D>().AddForce(knockbackDirection * knockbackForce);
    }

    private IEnumerator CollisionCooldown(float cooldownTime)
    {
        collisionEnabled = false;

        float collisionTimer = 0;
        while (collisionTimer <= cooldownTime)
        {
            collisionTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        collisionEnabled = true;
    }
}
