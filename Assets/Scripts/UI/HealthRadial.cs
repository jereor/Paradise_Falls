using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class HealthRadial : MonoBehaviour
{
    public static HealthRadial Instance { get; private set; }

    // References
    [SerializeField] private PlayerHealth playerHealth;
    private Material healthRadialMaterial;

    // Variables
    [SerializeField] private float maxSegments;
    [SerializeField] private float removedSegments;
    [SerializeField] private Color radialColor;
    private float currentSegments;

    private void Awake()
    {
        Instance = this;
        healthRadialMaterial = GetComponent<Image>().material;
        healthRadialMaterial.SetFloat("_SegmentCount", maxSegments);
        healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments);
        healthRadialMaterial.SetColor("_Color", radialColor);
    }

    public void RemoveSegments(float amount)
    {
        if (currentSegments - amount >= 0 && removedSegments + amount <= maxSegments) // Check that segments don't go negative
            healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments + amount); // Remove the amount
        else
            healthRadialMaterial.SetFloat("_RemovedSegments", maxSegments); // Remove all segments

        UpdateRadial();
    }

    public void AddSegments(float amount)
    {
        if (currentSegments + amount <= maxSegments && removedSegments - amount >= 0) // Check that segments don't go over maximum
            healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments - amount); // Restore the amount
        else
            healthRadialMaterial.SetFloat("_RemovedSegments", 0); // Restore segments to full

        UpdateRadial();
    }

    // Max Segments increasing function
    public void AddMaxSegments(float amount)
    {
        if (currentSegments == maxSegments)
        {
            maxSegments += amount;
            currentSegments = maxSegments;
        }
        else
            maxSegments += amount;

        UpdateRadial();
    }

    // Keep UI updated when changes to health happen. Called from HUDController.cs
    public void UpdateRadial()
    {
        // Update variables
        maxSegments = playerHealth.GetMaxHealth();
        currentSegments = playerHealth.GetHealth();
        removedSegments = playerHealth.GetMaxHealth() - playerHealth.GetHealth();

        // Update color
        if (currentSegments >= Mathf.Abs(maxSegments / 4 * 3)) // At full health or close
            radialColor = Color.green;
        else if (currentSegments >= Mathf.Abs(maxSegments / 2)) // At half health or above
            radialColor = Color.yellow;
        else if (currentSegments <= Mathf.Abs(maxSegments / 4)) // At critical health
            radialColor = Color.red;
        else // Above critical but below half health
            radialColor = new Color(1, 0.64705882352f, 0); // orange color

        // Update shader material properties
        healthRadialMaterial.SetFloat("_SegmentCount", maxSegments);
        healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments);
        healthRadialMaterial.SetColor("_Color", radialColor);

        // Make sure radial looks clean on higher health numbers by removing spacing
        if (playerHealth.GetMaxHealth() > 20)
            healthRadialMaterial.SetFloat("_SegmentSpacing", -0.01f);
        else
            healthRadialMaterial.SetFloat("_SegmentSpacing", 0.002f);
    }

    public int GetSegments()
    {
        return (int)healthRadialMaterial.GetFloat("_SegmentCount");
    }
}
