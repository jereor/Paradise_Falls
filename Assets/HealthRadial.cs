using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthRadial : MonoBehaviour
{
    public static HealthRadial Instance { get; private set; }

    // References
    private Material healthRadialMaterial;

    // Variables
    [SerializeField] private float maxSegments;
    [SerializeField] private float currentSegments;
    [SerializeField] private float removedSegments;
    [SerializeField] private Color radialColor;

    // Use this for initialization
    void Awake()
    {
        Instance = this;
        healthRadialMaterial = GetComponent<Image>().material;
        maxSegments = healthRadialMaterial.GetFloat("_SegmentCount");
        currentSegments = maxSegments;
        removedSegments = 0;
    }

    private void Start()
    {
        healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments);
        healthRadialMaterial.SetColor("_Color", radialColor);
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
        if (currentSegments + amount <= maxSegments) // Check that segments don't go over maximum
            healthRadialMaterial.SetFloat("_RemovedSegments", removedSegments - amount); // Restore the amount
        else
            healthRadialMaterial.SetFloat("_RemovedSegments", 0); // Restore segments to full

        removedSegments -= amount;
    }
}
