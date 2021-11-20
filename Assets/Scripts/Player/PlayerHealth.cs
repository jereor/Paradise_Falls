using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PlayerHealth : Health
{ 
    // References
    [SerializeField] private Image healthNumberImage;
    [SerializeField] List<Sprite> numberSprites = new List<Sprite>();

    // Keep UI updated when changes are made
    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
            UpdateHealthNumber();
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        HealthRadial.Instance.RemoveSegments(amount);
        UpdateHealthNumber();
    }

    public override void SetHealth(float amount)
    {
        base.SetHealth(amount);
        HealthRadial.Instance.AddSegments(amount);
        UpdateHealthNumber();
    }

    public override void UpgradeMaxHealth(float amount)
    {
        base.UpgradeMaxHealth(amount);
        HealthRadial.Instance.AddMaxSegments(amount);
        UpdateHealthNumber();
    }

    private void UpdateHealthNumber()
    {
        int newHealth = (int)base.CurrentHealth;
        healthNumberImage.sprite = numberSprites[newHealth];
    }
}