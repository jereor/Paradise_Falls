using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    [SerializeField] private float maxEnergy;
    [SerializeField] private float currentEnergy;

    // Start is called before the first frame update
    void Start()
    {
        currentEnergy = maxEnergy;
    }

    // This function uses the energy. If current energy amount tries to go below 0, function sets it to 0.
    public void UseEnergy(float amount)
    {
        if (currentEnergy - amount < 0)
            currentEnergy = 0;
        else
            currentEnergy -= amount;
    }

    // Setter for CurrentEnergy
    public void SetEnergy(float amount)
    {
        if (currentEnergy + amount >= maxEnergy)
            currentEnergy = maxEnergy;
        else
            currentEnergy += amount;
    }

    public float GetEnergy()
    {
        return currentEnergy;
    }

    public bool CheckForEnergy(float cost)
    {
        return currentEnergy - cost >= 0;
    }

    public bool OutOfEnergy()
    {
        return currentEnergy == 0;
    }


    // --- UPGRADE ---
    public void UpgradeMaxEnergy(float amount)
    {
        maxEnergy += amount;
    }


    // --- SAVING / LOADING ---
    public float getMaxEnergy()
    {
        return maxEnergy;
    }

    public void setMaxEnergy(float energy)
    {
        maxEnergy = energy;
        // Load game set our current energy to max
        SetEnergy(maxEnergy);
    }
}
