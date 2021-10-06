using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInteraction : MonoBehaviour
{
    public MeleeWeapon weaponScript;

    //private List<Collider2D> collidedAlreadyOnPull = new List<Collider2D>();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If Weapon has landed and Collided with Player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && weaponScript.getLanded())
        {
            weaponScript.Interact(collision.gameObject);
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && weaponScript.getBeingPulled())
        {
            weaponScript.DealDamage(collision);
            weaponScript.Knockback(collision.gameObject, gameObject, weaponScript.knockbackForce);
        }
    }
}
