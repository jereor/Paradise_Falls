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
    [SerializeField] private float currentSegments;
    [SerializeField] private float removedSegments;
    [SerializeField] private Color radialColor;

    

    private void Start()
    {
        Instance = this;
        healthRadialMaterial = GetComponent<Image>().material;
        healthRadialMaterial.SetFloat("_SegmentCount", maxSegments);
        healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments);
        healthRadialMaterial.SetColor("_Color", radialColor);
    }

    void Update()
    {
        if (!Application.IsPlaying(gameObject))
            UpdateSegmentsInEdit();
    }

    public void RemoveSegments(float amount)
    {
        if (currentSegments - amount >= 0) // Check that segments don't go under 0
            healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments + amount); // Remove the amount
        else
            healthRadialMaterial.SetFloat("_RemovedSegments", maxSegments); // Remove all segments

        removedSegments += amount;
    }

    public void AddSegments(float amount)
    {
        if (currentSegments + amount >= maxSegments) // Check that segments don't go over maximum
            healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments - amount); // Restore the amount
        else
            healthRadialMaterial.SetFloat("_RemovedSegments", 0); // Restore segments to full

        removedSegments -= amount;
    }

    // Max Segments increasing function. Not needed when update with health is working correctly
    public void AddMaxSegments(float amount)
    {
        maxSegments += amount;
        healthRadialMaterial.SetFloat("_SegmentCount", maxSegments);
        healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments - amount);
    }

    // Keep UI updated when changes are made in edit mode
    public void UpdateSegmentsInEdit()
    {
        // Update variables
        maxSegments = playerHealth.getMaxHealth();
        currentSegments = playerHealth.GetHealth();
        removedSegments = playerHealth.getMaxHealth() - playerHealth.GetHealth();

        // Update shader material properties
        healthRadialMaterial.SetFloat("_SegmentCount", maxSegments);
        healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments);
        healthRadialMaterial.SetColor("_Color", radialColor);
    }

    public int GetSegments()
    {
        return (int)healthRadialMaterial.GetFloat("_SegmentCount");
    }
}
