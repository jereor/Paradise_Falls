using UnityEngine;

public class Energy : MonoBehaviour
{
    [SerializeField] private float maxEnergy;
    [SerializeField] private float currentEnergy;
    [Tooltip("Give float like 0.5 (50%)")]
    [SerializeField] private float thresholdPercent;
    [SerializeField] private float energyRegenMultiplier;

    //public Slider energySlider;

    // Start is called before the first frame update
    void Start()
    {
        currentEnergy = maxEnergy;
    }

    private void FixedUpdate()
    {
        Regeneration();
    }

    private void Regeneration()
    {
        // Energy drops below threshold value return to threshold energy
        if (currentEnergy / maxEnergy < thresholdPercent)
            currentEnergy += maxEnergy * thresholdPercent * Time.fixedDeltaTime;
        // Normal regeneration
        else if (currentEnergy < maxEnergy)
            currentEnergy += 1f * energyRegenMultiplier * Time.fixedDeltaTime;
        // Prevent energy from going above maxEnergy
        else if (currentEnergy >= maxEnergy)
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
    public void UpgradeEnergyRegen(float amount)
    {
        energyRegenMultiplier += amount;
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
