using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class ParticleCollision : MonoBehaviour
{
    [SerializeField] private float knockbackForce;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject hitEffect;

    // State bools
    private bool weaponCollisionEnabled = true; // Bool for checking if collision has already been calculated for the weapon
    private bool enemyCollisionEnabled = true; // Bool for checking if collision has already been calculated for the enemy

    // Collisions
    private ParticleSystem ps;
    private List<ParticleCollisionEvent> collisionEvents;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();   
    }

    private void OnParticleCollision(GameObject other)
    {
        if (enemyCollisionEnabled) // Make sure collision has not yet been activated for the enemy
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                // knockback enemy
                Knockback(other.gameObject, gameObject, knockbackForce);
                StartCoroutine(EnemyCollisionCooldown(1));
            }
        }

        if (weaponCollisionEnabled) // Make sure collision has not yet been activated for the weapon
        {
            if (other.gameObject.CompareTag("MeleeWeapon"))
            {
                MeleeWeapon meleeScript = other.gameObject.GetComponent<MeleeWeapon>();
                if (meleeScript.getBeingPulled()) // Activate weapon power boost only if being pulled
                {
                    var direction = -1 * meleeScript.getDirection();
                    meleeScript.ActivatePowerBoost(direction);

                    // Hit Effect
                    ps.GetCollisionEvents(other, collisionEvents);
                    var dir = meleeScript.getDirection().x > 0 ? Vector3.forward : -Vector3.forward; // Calculate direction based on weapon direction
                    Instantiate(hitEffect, collisionEvents[0].intersection, Quaternion.LookRotation(dir*180)); // Instantiate the particles
                }
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

    // This cooldown makes it so enemy is never affected twice by the same shockwave
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

    // This cooldown makes it so weapon is never affected twice by the same shockwave
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
