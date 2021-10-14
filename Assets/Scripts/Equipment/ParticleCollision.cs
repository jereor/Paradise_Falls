using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class ParticleCollision : MonoBehaviour
{
    [SerializeField] private float knockbackForce;
    [SerializeField] private Transform player;
    // [SerializeField] private GameObject collisionDetector;

    // State bools
    private bool weaponCollisionEnabled = true; // Bool for checking if collision has already been calculated for the weapon
    private bool enemyCollisionEnabled = true; // Bool for checking if collision has already been calculated for the enemy
    // private bool triggerEnabled = true; // Bool for checking if trigger has already happened

    // References
    // List<ParticleSystem.Particle> enterList = new List<ParticleSystem.Particle>();

    private void OnParticleCollision(GameObject other)
    {
        if (enemyCollisionEnabled) // Make sure trigger has not yet been activated for the enemy
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                // knockback enemy
                Knockback(other.gameObject, gameObject, knockbackForce);
                StartCoroutine(EnemyCollisionCooldown(1));
            }
        }

        if (weaponCollisionEnabled) // Make sure trigger has not yet been activated for the weapon
        {
            if (other.gameObject.CompareTag("MeleeWeapon"))
            {
                MeleeWeapon meleeScript = other.gameObject.GetComponent<MeleeWeapon>();
                if (meleeScript.getBeingPulled()) // Activate weapon power boost only if being pulled
                {
                    meleeScript.ActivatePowerBoost(new Vector2(1, 0));
                }
                StartCoroutine(WeaponCollisionCooldown(1));
            }
        }
    }

    
    //private void OnParticleTrigger()
    //{
        /* LATEST ATTEMPT. FOR SOME REASON THIS ALWAYS RETURNS THE MELEEWEAPON SCRIPT
        ParticleSystem ps = GetComponent<ParticleSystem>();
        Component component = ps.trigger.GetCollider(0);

        if (component.transform.parent.TryGetComponent(out MeleeWeapon weapon))
        {
            Debug.Log("Weapon hit!");
            weapon.ActivatePowerBoost();
        }
        */

        /* USING TRIGGER PARTICLES WAS TOO HARD SO ABANDONED THEM FOR NOW
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterList); // DOES NOT WORK

        // Iterate
        for (int i = 0; i < numEnter; i++)
        {
            // THE ITERATION NEVER EVEN COMES HERE BECAUSE WHEN GETTING TRIGGER PARTICLES THE LIST NEVER GETS ANY TRIGGERS
            ParticleSystem.Particle particle = enterList[i];
            // Instantiate a game object to get what object this particle triggered
            Instantiate(collisionDetector, particle.position, Quaternion.identity);
            enterList[i] = particle;
        }

        // Tell trigger enters to particle system too
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterList);
        */
    //}


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

    // MeleeWeapon needs to know if collision has been done so it doesn't trigger twice
    public bool WeaponCollisionEnabled()
    {
        return weaponCollisionEnabled;
    }
}
