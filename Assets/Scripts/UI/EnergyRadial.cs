using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class EnergyRadial : MonoBehaviour
{
    public static EnergyRadial Instance { get; private set; }

    // References
    [SerializeField] private Energy playerEnergy;
    private Material energyRadialMaterial;

    // Variables
    [SerializeField] private float maxSegments;
    [SerializeField] private float currentSegments;
    [SerializeField] private float removedSegments;
    [SerializeField] private Color radialColor;

    private void Start()
    {
        Instance = this;
        energyRadialMaterial = GetComponent<Image>().material;
        energyRadialMaterial.SetFloat("_SegmentCount", maxSegments / 100 * 2);
        energyRadialMaterial.SetFloat("_RemovedSegments", removedSegments);
        energyRadialMaterial.SetColor("_Color", radialColor);
    }

    void Update()
    {
        // Keep UI updated when changes are made in edit mode
        if (!Application.IsPlaying(gameObject))
            UpdateSegments();
    }

    public void RemoveSegments(float amount)
    {
        if (currentSegments - amount >= 0) // Check that segments don't go under 0
            energyRadialMaterial.SetFloat("_RemovedSegments", removedSegments + amount); // Remove the amount
        else
            energyRadialMaterial.SetFloat("_RemovedSegments", maxSegments); // Remove all segments

        removedSegments += amount;
    }

    public void AddSegments(float amount)
    {
        if (currentSegments + amount <= maxSegments) // Check that segments don't go over maximum
            energyRadialMaterial.SetFloat("_RemovedSegments", removedSegments - amount); // Restore the amount
        else
            energyRadialMaterial.SetFloat("_RemovedSegments", 0); // Restore segments to full

        removedSegments -= amount;
    }

    // Max Segments increasing function. Not needed when update with health is working correctly
    public void AddMaxSegments(float amount)
    {
        maxSegments += amount;
        energyRadialMaterial.SetFloat("_SegmentCount", maxSegments);
        energyRadialMaterial.SetFloat("_RemovedSegments", removedSegments - amount);
    }

    public void UpdateSegments()
    {
        // Update variables
        currentSegments = playerEnergy.GetEnergy() / 50;
        removedSegments = (playerEnergy.getMaxEnergy() - playerEnergy.GetEnergy()) / 50;

        // Update shader material properties
        energyRadialMaterial.SetFloat("_SegmentCount", maxSegments);
        energyRadialMaterial.SetFloat("_RemovedSegments", removedSegments);
        energyRadialMaterial.SetColor("_Color", radialColor);
    }

    public int GetSegments()
    {
        return (int)energyRadialMaterial.GetFloat("_SegmentCount");
    }
}
