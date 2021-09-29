using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Interactable
{
    [Header("Variables from This script")]
    [SerializeField] private float weaponThrowDamage;
    [SerializeField] private bool playerIsClose;
    private bool isFlying;
    private PlayerCombat combatScript;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            // Gives persmission to save and gives GameObject that this script is attached so PlayerInteraction know of whos Interact() to Invoke
            collision.gameObject.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.gameObject.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            combatScript = collision.gameObject.GetComponent<PlayerCombat>();
            // Mark for this item that player is close (easier to track interactions when debugging)
            playerIsClose = true;

            ShowFloatingText();
        }
        // Ground
        else if (collision.gameObject.layer == 6)
        {
            isFlying = false;
        }
        // Enemy
        else if (collision.gameObject.layer == 7)
        {
            if (isFlying)
            {
                collision.gameObject.GetComponent<Health>().TakeDamage(weaponThrowDamage);
            }

            isFlying = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            collision.gameObject.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.gameObject.GetComponent<PlayerInteractions>().GiveGameObject(null);
            playerIsClose = false;

            HideFloatingText();
        }
    }

    public override void Interact()
    {
        combatScript.PickUpWeapon();
        Destroy(gameObject);
    }
}
