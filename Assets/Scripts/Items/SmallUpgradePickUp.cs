using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallUpgradePickUp : Interactable
{
    enum Recource
    {
        LightDmg,
        ThrowDmg,
        PullDmg,
        PowerBoostedDmg,
        ThrowChargeTime,
        MaxHealth,
        MaxEnergy
    }

    [Header("Variables from This script")]
    [SerializeField] private Recource myEnum;
    [SerializeField] private bool playerIsClose;
    [SerializeField] private GameObject playerObject;

    [Header("Upgrade amounts(change these only on prefab, amount that is added when picked up)")]
    [SerializeField] private float lightDamage;
    [SerializeField] private float throwDamage;
    [SerializeField] private float pullDamage;
    [SerializeField] private float powerBoostedDamage;
    [SerializeField] private float throwChargeTime;
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxEnergy;

    void Start()
    {
        UpdateTextBinding();

        HideFloatingText();

        itemEvent.AddListener(Interact);
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

    private void UpgradeEnum()
    {
        switch (myEnum)
        {
            case Recource.LightDmg:
                playerObject.GetComponent<PlayerCombat>().UpgradeLightDamage(lightDamage);
                break;
            case Recource.ThrowDmg:
                playerObject.GetComponent<PlayerCombat>().UpgradeThrowDamage(throwDamage);
                break;
            case Recource.PullDmg:
                playerObject.GetComponent<PlayerCombat>().UpgradePullDamage(pullDamage);
                break;
            case Recource.PowerBoostedDmg:
                playerObject.GetComponent<PlayerCombat>().UpgradePowerBoostedDamage(powerBoostedDamage);
                break;
            case Recource.ThrowChargeTime:
                playerObject.GetComponent<PlayerCombat>().UpgradeThrowMaxChargeTime(throwChargeTime);
                break;
            case Recource.MaxHealth:
                //playerObject.GetComponent<Health>().
                break;
            case Recource.MaxEnergy:
                break;
            default:
                break;
        }
    }

}
