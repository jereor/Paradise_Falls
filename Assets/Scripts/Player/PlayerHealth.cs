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

    private void Update()
    {
        UpdateHealthNumber();
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        HealthRadial.Instance.RemoveSegments(amount);
        UpdateHealthNumber();
    }

    public override void UpgradeMaxHealth(float amount)
    {
        base.UpgradeMaxHealth(amount);
        UpdateHealthNumber();
        HealthRadial.Instance.AddMaxSegments(amount);
    }

    private void UpdateHealthNumber()
    {
        int newHealth = (int)base.CurrentHealth;
        healthNumberImage.sprite = numberSprites[newHealth];
    }
}