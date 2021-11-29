using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallUpgradePickUp : Interactable
{
    enum Upgrade
    {
        LightDmg,
        Throw,
        MaxHealth,
        EnergyRegen
    }

    [Header("Variables from This script")]
    [SerializeField] private Upgrade myEnum;
    [SerializeField] private bool playerIsClose;
    [SerializeField] private GameObject playerObject;

    [Header("Upgrade amounts(change these only on prefab, amount that is added when picked up)")]
    [SerializeField] private float lightDamage;
    [SerializeField] private float heavyDamage;
    [SerializeField] private float throwMaxChargeDamage;
    [SerializeField] private float throwChargeTime;
    [SerializeField] private float maxHealthAmount;
    [SerializeField] private float energyRegenMultiplier;

    private bool saveBuffer = false;

    void Start()
    {
        UpdateTextBinding();

        HideFloatingText();

        itemEvent.AddListener(Interact);
    }

    private void Update()
    {
        if (saveBuffer && Player.Instance != null)
            UpgradeEnum();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            // Gives persmission to save and gives GameObject that this script is attached so PlayerInteraction know of whos Interact() to Invoke
            collision.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            playerObject = collision.gameObject;
            // Mark for this item that player is close (easier to track interactions when debugging)
            playerIsClose = true;


            ShowFloatingText();
        }
    }

    // Reverse of OnTriggerEnter2D()
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name.Contains("Player"))
        {
            collision.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.GetComponent<PlayerInteractions>().GiveGameObject(null);
            playerObject = null;
            playerIsClose = false;

            HideFloatingText();
        }
    }

    public override void Interact()
    {
        if (playerObject != null)
        {
            // Unlock Multitool for the Player
            UpgradeEnum();
            // Destroy this item from scene
            Destroy(gameObject);
        }
    }

    public void UpgradeEnum()
    {
        switch (myEnum)
        {
            case Upgrade.LightDmg:
                playerObject.GetComponent<PlayerCombat>().UpgradeMeleeDamage(lightDamage, heavyDamage);
                Player.Instance.UpdateMeleePickUps();
                break;
            case Upgrade.Throw:
                playerObject.GetComponent<PlayerCombat>().UpgradeThrowDamage(throwMaxChargeDamage);
                playerObject.GetComponent<PlayerCombat>().UpgradeThrowMaxChargeTime(throwChargeTime);
                Player.Instance.UpdateThrowPickUps();
                break;
            case Upgrade.MaxHealth:
                playerObject.GetComponent<PlayerHealth>().UpgradeMaxHealth(maxHealthAmount);
                playerObject.GetComponent<PlayerHealth>().ResetHealthToMax();
                Player.Instance.UpdateHealthPickUps();
                break;
            case Upgrade.EnergyRegen:
                playerObject.GetComponent<Energy>().UpgradeEnergyRegen(energyRegenMultiplier);
                Player.Instance.UpdateEnergyPickUps();
                break;
            default:
                break;
        }
        Destroy(gameObject);
    }

    public void SetSaveBuffer(bool b, GameObject player)
    {
        saveBuffer = b;
        playerObject = player;
    }
}
