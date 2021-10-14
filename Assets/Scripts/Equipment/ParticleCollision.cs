using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ParticleCollision : MonoBehaviour
{
    [SerializeField] private float knockbackForce;
    [SerializeField] private GameObject collisionDetector;

    // State bools
    private bool weaponCollisionEnabled = true; // Bool for checking if collision has already been calculated for the weapon
    private bool enemyCollisionEnabled = true; // Bool for checking if collision has already been calculated for the enemy
    private bool triggerEnabled = true; // Bool for checking if trigger has already happened

    // References
    private ParticleSystem ps;
    List<ParticleSystem.Particle> enterList = new List<ParticleSystem.Particle>();

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleTrigger()
    {
        // Get trigger enters from particle system
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterList);

        // Iterate
        for (int i = 0; i < numEnter; i++)
        {
            Debug.Log("Instantiate collision detector!");
            ParticleSystem.Particle particle = enterList[i];
            // Instantiate a game object to get what object this particle triggered
            Instantiate(collisionDetector, particle.position, Quaternion.identity);
            enterList[i] = particle;
        }

        // Tell trigger enters to particle system too
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterList);


        /* OLD WAY USING ONCOLLISIONENTER2D
        if (enemyTriggerEnabled) // Make sure trigger has not yet been activated for the enemy
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // knockback enemy
                Knockback(collision.gameObject, gameObject, knockbackForce);
                StartCoroutine(EnemyCollisionCooldown(1));
            }
        }

        if (weaponTriggerEnabled) // Make sure trigger has not yet been activated for the weapon
        {
            MeleeWeapon meleeScript = collision.GetComponent<MeleeWeapon>();
            if (meleeScript.getBeingPulled()) // Activate weapon power boost only if being pulled
            {
                if (collision.gameObject.CompareTag("MeleeWeapon"))
                {
                    meleeScript.ActivatePowerBoost();
                    StartCoroutine(WeaponCollisionCooldown(1));
                }
            }
        }
        */
    }

    // Small knockback to the target. Knockback knocks slightly upwards so the friction doesn't stop the target right away.
    private void Knockback(GameObject target, GameObject from, float knockbackForce)
    {
        float pushbackX = target.transform.position.x - from.transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Mathf.Abs(pushbackX / 4)).normalized;
        target.GetComponent<Rigidbody2D>().AddForce(knockbackDirection * knockbackForce);
    }

    private IEnumerator TriggerCooldown(float cooldownTime)
    {
        triggerEnabled = false;

        float collisionTimer = 0;
        while (collisionTimer <= cooldownTime)
        {
            collisionTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        triggerEnabled = true;
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
