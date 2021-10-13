using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ParticleCollision : MonoBehaviour
{
    [SerializeField] float knockbackForce;

    private bool weaponCollisionEnabled = true; // Bool for checking if collision has already been calculated for the weapon
    private bool enemyCollisionEnabled = true;// Bool for checking if collision has already been calculated for the enemy

    private void OnParticleCollision(GameObject collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (enemyCollisionEnabled) // Make sure collision has not yet been activated for the enemy
            {
                // knockback enemy
                Knockback(collision.gameObject, gameObject, knockbackForce);
                StartCoroutine(EnemyCollisionCooldown(1));
            }
        }

        if (collision.gameObject.CompareTag("MeleeWeapon"))
        {
            if (weaponCollisionEnabled) // Make sure collision has not yet been activated for the weapon
            {
                // Power Boost the weapon and set it flying forwards
                Knockback(collision.gameObject, gameObject, knockbackForce); // Replace this knock back with a hardcoded cool one
                StartCoroutine(WeaponCollisionCooldown(1));
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

    private IEnumerator EnemyCollisionCooldown(float cooldownTime)
    {
        enemyCollisionEnabled = false;

        float collisionTimer = 0;
        while (collisionTimer <= cooldownTime)
        {
            collisionTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        enemyCollisionEnabled = true;
    }

    // This cooldown makes it so objects are never affected twice by the same shockwave
    private IEnumerator WeaponCollisionCooldown(float cooldownTime)
    {
        weaponCollisionEnabled = false;

        float collisionTimer = 0;
        while (collisionTimer <= cooldownTime)
        {
            collisionTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        weaponCollisionEnabled = true;
    }
}
