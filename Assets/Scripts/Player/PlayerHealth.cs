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

    public override void ResetHealthToMax()
    {
        base.ResetHealthToMax();
        HealthRadial.Instance.UpdateRadial();
        UpdateHealthNumber();
    }

    // Keep UI updated when changes are made in edit mode. Called from HUDController.cs
    public void UpdateHealthInEdit()
    {
        ResetHealthToMax(); // Keep health always at maximum in edit mode
        UpdateHealthNumber(); // Keep health number always updated in edit mode
    }

    public void UpdateHealthNumber()
    {
        int newHealth = (int)GetHealth();
        healthNumberImage.sprite = numberSprites[newHealth];
    }
}