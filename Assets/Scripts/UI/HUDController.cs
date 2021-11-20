using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Use this script to order edit mode updates of the HUD

[ExecuteAlways]
public class HUDController : MonoBehaviour
{
    // Scripts
    [SerializeField] private Energy energyScript;
    [SerializeField] private PlayerHealth healthScript;
    [SerializeField] private HealthRadial healthRadialScript;
    [SerializeField] private EnergyRadial energyRadialScript;

    private void Start()
    {
        if (Application.isPlaying)
        {
            healthScript.ResetHealthToMax();
            healthRadialScript.UpdateRadial();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            healthScript.UpdateHealthInEdit();
            healthRadialScript.UpdateRadial();
        }
    }
}
