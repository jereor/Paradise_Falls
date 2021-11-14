using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : Health
{
    // References
    [SerializeField] private Image healthNumberImage;
    [SerializeField] List<Sprite> numberSprites = new List<Sprite>();

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        int newHealth = (int)base.CurrentHealth;
        healthNumberImage.sprite = numberSprites[newHealth];

        HealthRadial.Instance.RemoveSegments(amount);
    }
}
