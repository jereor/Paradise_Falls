using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth; // Initializes max health at game start
    [SerializeField] private float currentHealth;

    private SpriteRenderer rndr;
    [SerializeField] private Color blockedColor = Color.blue; // Block indication color.
    [SerializeField] private Color damageColor = Color.red; // Damage indication color.

    [Tooltip("Enable if you want this script to destroy the gameobject after health reaches zero")]
    public bool destroyWhenDead;

    [Tooltip("Functions that are executed when health reaches zero. There can be multiple.")]
    public UnityEvent onDie;

    // Booleans for PlayerPlaySound.cs
    private bool playSoundHurt = false;
    private bool playSoundHurtShielded = false;

    /// <summary>
    /// Invoked when <see cref="TakeDamage"/> is called successfully.
    /// </summary>
    public event EventHandler TakingDamage;

    /// <summary>
    /// Returns true if the current health is above zero.
    /// This can be used if onDie invokes are not enough or the object isn't destroyed after health reaches zero.
    /// </summary>
    public bool IsDead() => currentHealth <= 0;

    // Start is called before the first frame update
    // Prevent assigning health at or below zero at the start
    private void Start()
    {
        blockedColor = Color.blue;
        damageColor = Color.red;
        currentHealth = (maxHealth > 0 ? maxHealth : 1);
        maxHealth = currentHealth;
        rndr = GetComponent<SpriteRenderer>();
        if(rndr == null) // If gameobject has not SpriteRenderer, search from childs.
        {
            rndr = GetComponentInChildren<SpriteRenderer>();
            //if(rndr == null)
            //{
            //    rndr = GetComponentInParent<SpriteRenderer>();
            //}
        }
    }

    /// <summary>
    /// Reduces the health by the given amount. If the amount is negative the effect would heal. If health reaches zero, onDie will be invoked.
    /// <see cref="TakingDamage"/> will be invoked on every successful call.
    /// </summary>
    /// <param name="amount">The amount of damage/heal applied.</param>
    virtual public void TakeDamage(float amount)
    {
        if (IsDead()) return; // If the script is still active, don't invoke onDie more than once

        // If this object has a shield and they are currently blocking, reduce damage
        if (gameObject.TryGetComponent(out Shield shield))
        {
            if (shield.Parrying) amount = 0;
            else if (shield.Blocking)
            {
                amount -= shield.ProtectionAmount;
                playSoundHurtShielded = true;
                StartCoroutine(HitIndication(blockedColor)); // Player blocked the attack.
            }
            else
            {
                playSoundHurt = true;
                StartCoroutine(HitIndication(damageColor)); // Player got hit.
            }

            if (amount < 0) amount = 0;
        }
        else
        {
            StartCoroutine(HitIndication(damageColor)); // Enemy got hit.
        }

        currentHealth -= amount;

        TakingDamage?.Invoke(this, EventArgs.Empty);

        if (currentHealth <= 0)
        {
            onDie.Invoke();

            if (destroyWhenDead)
                Destroy(gameObject);
        }
    }

    IEnumerator HitIndication(Color color)
    {
        if(rndr != null)
        {
            rndr.color = color;
            yield return new WaitForSeconds(0.1f);
            rndr.color = Color.white;
        }
    }

    // Setter for new CurrentHealth amount.
    virtual public void SetHealth(float amount)
    {
        Debug.Log("Health increased by " + amount);
        if (currentHealth + amount >= maxHealth)
            currentHealth = maxHealth;
        else
            currentHealth += amount;
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    virtual public void ResetHealthToMax()
    {
        currentHealth = maxHealth;
    }

    // For PlayerPlaySound to track these states
    public bool getPlaySoundHurt()
    {
        if (playSoundHurt)
        {
            playSoundHurt = false;
            return true;
        }
        else
            return false;
    }
    public bool getPlaySoundHurtShielded()
    {
        if (playSoundHurtShielded)
        {
            playSoundHurtShielded = false;
            return true;
        }
        else
            return false;
    }

    // --- UPGRADE ---
    virtual public void UpgradeMaxHealth(float amount)
    {
        maxHealth += amount;
        SetHealth(amount);
    }


    // --- SAVING / LOADING ---
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        // Load game set our current health to max
        SetHealth(maxHealth);
    }
}
