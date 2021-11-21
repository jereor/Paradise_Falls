using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    // Scripts
    [SerializeField] private Energy energyScript;
    [SerializeField] private PlayerHealth healthScript;
    [SerializeField] private HealthRadial healthRadialScript;
    [SerializeField] private EnergyRadial energyRadialScript;
    [SerializeField] private Image multitoolImage;

    private void Awake()
    {
        Instance = this;
    }

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
        // Order edit mode updates of the HUD
        if (!Application.IsPlaying(gameObject))
        {
            healthScript.UpdateHealthInEdit();
            healthRadialScript.UpdateRadial();
        }
    }

    // Update HUD multitool image. Called from PlayerCombat.cs
    public void SetMultitoolImage(bool available)
    {
        multitoolImage.enabled = available;
    }
}
