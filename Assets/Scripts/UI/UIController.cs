using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    // Scripts
    private Energy energyScript;
    private Health healthScript;
    private PlayerCombat combatScript;

    [Header("UI Elements")]
    public Slider energySlider;
    public List<Image> healthIcons = new List<Image>();
    public Image multitoolImage;

    // Variables to handle showed UI elements
    private GameObject player;
    private float healthRatio;
    private int healthIconsCurrentlyShowed = 1;
    private int healthIconsToShow = 0;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        energyScript = player.GetComponent<Energy>();
        healthScript = player.GetComponent<Health>();
        combatScript = player.GetComponent<PlayerCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEnergySliderValues();

        UpdateHealthIcons();

        UpdateMultitoolWielded();

        // Debug testing
        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    healthScript.TakeDamage(3f);
        //}
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    healthScript.SetHealth(1f);
        //}
    }

    // If we have assigned slider and update only on value change
    private void UpdateEnergySliderValues()
    {
        if (energySlider != null)
        {
            // MaxEnergy value if it is different float update it
            if (energySlider.maxValue != energyScript.getMaxEnergy())
                energySlider.maxValue = energyScript.getMaxEnergy();
            // Fill value
            if (energySlider.value != Mathf.RoundToInt(energyScript.GetEnergy() / energyScript.getMaxEnergy() * 100f))
            {
                energySlider.value = Mathf.RoundToInt(energyScript.GetEnergy() / energyScript.getMaxEnergy() * 100f);
            }
        }
    }

    // If healthIconsToShow is different value than healthIconsCurrentlyShowed update icons
    private void UpdateHealthIcons()
    {
        // Set healthIconsToShow ( x = ratio * y ) rounded
        healthIconsToShow = Mathf.RoundToInt(healthScript.GetHealth() / healthScript.getMaxHealth() * healthIcons.Count);

        if(healthIconsToShow != healthIconsCurrentlyShowed)
        {
            // Enable correct amount of icons/images
            if(healthIconsToShow > healthIconsCurrentlyShowed)
            {
                for (int i = healthIconsCurrentlyShowed - 1; i <= healthIconsToShow - 1; i++)
                {
                    healthIcons[i].enabled = true;
                }
            }
            // Disable correct amount of icon/images
            else
            {
                for (int i = healthIconsCurrentlyShowed - 1; i > healthIconsToShow - 1; i--)
                {
                    healthIcons[i].enabled = false;
                }
            }
            // Update our currently showed image amount
            healthIconsCurrentlyShowed = healthIconsToShow;
        }
    }

    // If booleans differ update image
    private void UpdateMultitoolWielded()
    {
        if(multitoolImage.enabled != combatScript.getWeaponWielded())
        {
            multitoolImage.enabled = combatScript.getWeaponWielded();
        }
    }
}
