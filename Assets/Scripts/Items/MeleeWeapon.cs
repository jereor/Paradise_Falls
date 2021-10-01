using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeleeWeapon : Interactable
{
    [Header("Variables from This script")]
    [SerializeField] private float weaponThrowDamage; // Damage dealt if hits enemy
    [SerializeField] private float rotAngle; // Rotation angle to spin when thowing
    [SerializeField] private float ricochetForce; // Force of hit ricochet on enemies and gorund elements
    [SerializeField] private float enemyHitRicochetY; // Float parameter if we want to ricochet weapon slightly upward feels better and tell player that we hit and dealt damage to something
    [SerializeField] private float ricochetTimer; // Time we need to wait if ricochet happens to pull again
    [SerializeField] private float maxTimeStuck; // Maximum time weapon can be stuck
    [SerializeField] private float pullForce; // Force we are pulling
    [SerializeField] private float maxDistance; // Max distance to travel with gravityscale 0 and deal damage
    [SerializeField] private int maxRicochetTimes; // Maximum amount of ricochets when pulling


    // Other variables
    private Rigidbody2D myRB;
    private float defaultGravityScale;
    private PlayerCombat combatScript; // Players combat script got from collision with player

    private Vector3 startPoint; // Used to calculate maximum distance to travel
    private bool canDealDamage; // If weapon can deal damage

    private GameObject pullingObject; // Object that is pulling given in PullWeapon()
    private bool beingPulled;

    private Vector2 stuckParameters; // X and Y values when we are checking if weapon is stuck while we are pulling
    private float stuckCounter = 0f; // Counter used in CheckStuck()

    private bool ricochetCooldown; // True if ricochet is on cooldown
    private float lastRicochetTime = 0f;
    private int timesRicochet; // Times weapon has ricochet in one instance

    private void Start()
    {
        // Since Interactable Start() is not run when instantiating a prefab these need to be run here
        HideFloatingText();
        // We need to AddThisLister here because this is not Instantiated on Scene load
        itemEvent.AddListener(Interact);
        UpdateTextBinding();


        myRB = GetComponent<Rigidbody2D>();

        defaultGravityScale = myRB.gravityScale;
        // Feels better to set gravity scale to 0 when throwing in 2D Game
        myRB.gravityScale = 0f;
        // Set our throw start point
        startPoint = transform.position;

        // Weapon can deal damage since it is just thrown
        canDealDamage = true;
    }
    private void FixedUpdate()
    {
        ItemPull();
        WeaponThrow();
        RicochetCooldown();
    }

    private void WeaponThrow()
    {
        if (canDealDamage)
        {
            transform.Rotate(new Vector3(0f, 0f, -rotAngle * Time.deltaTime));
        }
        if ((transform.position - startPoint).magnitude >= maxDistance && !beingPulled)
        {
            myRB.gravityScale = defaultGravityScale;

            // Stop weapon from moving in x direction
            myRB.velocity = new Vector2(0f, myRB.velocity.y);
            canDealDamage = false;
        }
    }

    private void ItemPull()
    {
        if (beingPulled && !ricochetCooldown)
        {
            // Rotating object to point player
            Vector3 vectorToTarget = pullingObject.transform.position - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * pullForce);

            // Moving object to player
            myRB.velocity = vectorToTarget.normalized * pullForce * Time.deltaTime;

            // While being pulled check if weapon is stuck
            CheckStuck();

            // Check if we need to ignore layer collision maximum amount of ricochets has happened
            CheckRicochet();
        }
    }

    private void CheckStuck()
    {
        stuckCounter += Time.deltaTime;

        if (stuckCounter >= maxTimeStuck)
        {
            // Weapon havent moved in X or Y position since last check ignore layer collision and pull Weapon straight back
            if (Mathf.Abs(stuckParameters.x - transform.position.x) < 1 || Mathf.Abs(stuckParameters.x - transform.position.x) < 1)
            {
                Debug.Log("Weapon is stuck");
                // Ignore layer collision set back on Interact()
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("MeleeWeapon"), true);
            }
            // Set new parameters now to check position when stuckCounter hits maxTimeStuck next time
            stuckParameters = transform.position;
            stuckCounter = 0f;
        }
    }

    private void RicochetCooldown()
    {
        // Check if we want to delay pulling when ricochet happens
        if (ricochetCooldown)
        {
            if (Time.time - lastRicochetTime > ricochetTimer)
            {
                ricochetCooldown = false;
            }
        }
    }
    private void CheckRicochet()
    {
        // If we ricochet maxRicochetTimes ignore meleeWeapon and Ground layers so we can actually get the weapon back
        if (timesRicochet == maxRicochetTimes)
        {      
            // Ignore layer collision set back on Interact()
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("MeleeWeapon"), true);
        }
    }

    private void RotateText()
    {
        if (floatingText.transform.rotation.z != 0f)
        {
            floatingText.transform.rotation = new Quaternion(floatingText.transform.rotation.x, floatingText.transform.rotation.y, 0f, floatingText.transform.rotation.w);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Gives persmission to save and gives GameObject that this script is attached so PlayerInteraction know of whos Interact() to Invoke
            collision.gameObject.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.gameObject.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            combatScript = collision.gameObject.GetComponent<PlayerCombat>();

            // If we pulled and hit player -> pick up
            if (beingPulled)
            {
                Interact();
            }

            ShowFloatingText();
            RotateText();
        }
        // Ground
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // While weapon is beingPulled we hit object on ground layer
            if (beingPulled)
            {
                // We set this on cooldown not pulling until cooldown completed
                ricochetCooldown = true;
                // We need to set this here for timer calculations CheckRicochet()
                lastRicochetTime = Time.time;

                // Ricochet quickmaths
                Vector2 objectNormal = collision.contacts[0].normal;
                Vector2 tmp = Vector2.Reflect(myRB.velocity, objectNormal).normalized * ricochetForce;
                myRB.velocity = tmp;

                // Increase times ricochet here
                timesRicochet++;

                beingPulled = false;
            }
            // We hit ground while we beingPulled = false -> we lose momentum -> no damage
            else
            {
                myRB.gravityScale = defaultGravityScale;
                canDealDamage = false;
            }
        }
        // Enemy
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {          
            // Hits enemy when can deal damage
            if (canDealDamage)
            {
                // Ricochet quickmaths
                Vector2 tmp = new Vector2(collision.contacts[0].point.x - collision.transform.position.x , collision.contacts[0].point.y - collision.transform.position.y);
                myRB.velocity = tmp.normalized + new Vector2(0, enemyHitRicochetY) * ricochetForce;

                // If this is somehow not default set it here to be sure
                if(myRB.gravityScale != defaultGravityScale)
                    myRB.gravityScale = defaultGravityScale;

                // Deal damage last if we kill enemy we lose collision parameter
                collision.gameObject.GetComponent<Health>().TakeDamage(weaponThrowDamage);
                // Cant deal damage twice
                canDealDamage = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            collision.gameObject.GetComponent<PlayerInteractions>().AllowInteract(false);
            collision.gameObject.GetComponent<PlayerInteractions>().GiveGameObject(null);

            HideFloatingText();
        }
    }

    // Called from PlayerMeleeCombat
    public void PullWeapon(GameObject objectThatPulls)
    {
        // Weapon cannot deal damage aka hit enemy or ground
        if (!canDealDamage)
        {
            myRB.velocity = Vector2.zero; // Stop moving at the start of pulling physics bugs

            pullingObject = objectThatPulls;
            beingPulled = true;           
        }
    }

    // Called as event from player if interacted or when weapon is pulled and hits player
    public override void Interact()
    {
        Debug.Log("Pick up");
        // Set collision detection back if this was set to ignore
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("MeleeWeapon"), false);
        // Inform Player to pickup
        combatScript.PickUpWeapon();
        // Destroy instance form scene
        Destroy(gameObject);
    }
}