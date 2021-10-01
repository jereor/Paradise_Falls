using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    public float debugHealth;

    [Tooltip("Enable if you want this script to destroy the gameobject after health reaches zero")]
    public bool destroyWhenDead;

    [Tooltip("Functions that are executed when health reaches zero. There can be multiple.")]
    public UnityEvent onDie;

    /// <summary>
    /// Invoked when <see cref="TakeDamage"/> is called successfully.
    /// </summary>
    public event EventHandler TakingDamage;

    public float MaxHealth { get; private set; }

    public float CurrentHealth { get; private set; }

    /// <summary>
    /// Returns true if the current health is above zero.
    /// This can be used if onDie invokes are not enough or the object isn't destroyed after health reaches zero.
    /// </summary>
    public bool IsDead() => CurrentHealth <= 0;

    // Start is called before the first frame update
    // Prevent assigning health at or below zero at the start
    private void Start()
    {
        CurrentHealth = (maxHealth > 0 ? maxHealth : 1);
        MaxHealth = CurrentHealth;
        debugHealth = CurrentHealth; // DEBUG: For tracking health in inspector
    }

    /// <summary>
    /// Reduces the health by the given amount. If the amount is negative the effect would heal. If health reaches zero, onDie will be invoked.
    /// <see cref="TakingDamage"/> will be invoked on every successful call.
    /// </summary>
    /// <param name="amount">The amount of damage/heal applied.</param>
    public void TakeDamage(float amount)
    {
        if (IsDead()) return; // If the script is still active, don't invoke onDie more than once

        // If this object has a shield and they are currently blocking, reduce damage
        if (gameObject.TryGetComponent(out Shield shield))
        {
            if (shield.Parrying) amount = 0;
            else if (shield.Blocking) amount -= shield.ProtectionAmount;

            if (amount < 0) amount = 0;
        }

        CurrentHealth -= amount;
        debugHealth = CurrentHealth; // DEBUG: For tracking health in inspector

        TakingDamage?.Invoke(this, EventArgs.Empty);

        if (CurrentHealth <= 0)
        {
            onDie.Invoke();

            if (destroyWhenDead)
                Destroy(gameObject);
        }
    }
}
