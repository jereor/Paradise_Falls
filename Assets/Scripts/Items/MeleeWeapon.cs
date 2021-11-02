using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Variables from This script")]
    [SerializeField] private float weaponThrowDamage; // Damage dealt if hits enemy
    [SerializeField] private float powerBoostedDamage; // Damage dealt if hits enemy power boosted
    [SerializeField] private float weaponPullDamage;
    [SerializeField] private float weakPointMultiplier;
    [SerializeField] private float rotSpeed; // Rotation angle to spin when thowing
    [SerializeField] private float ricochetImpulseForce; // Force of hit ricochet on enemies and gorund elements
    [SerializeField] private float ricochetYImpulse; // Float parameter if we want to ricochet weapon slightly upward feels better and tell player that we hit and dealt damage to something
    [SerializeField] private float pullForce; // Force we are pulling
    [SerializeField] private float powerBoostForce; // Force of shockwave power boost
    [SerializeField] private float maxDistance; // Max distance to travel with gravityscale 0
    [SerializeField] private float meleeWeaponGrapplingDistance;

    [SerializeField] private bool worldPickUp;

    public float knockbackForce;

    // Other variables
    private Rigidbody2D myRB;
    private float defaultGravityScale;

    public SpriteRenderer mySpriteRenderer;

    private Vector3 startPoint; // Used to calculate maximum distance to travel
    private bool landed; // If weapon can deal damage

    private GameObject pullingObject; // Object that is pulling given in PullWeapon()
    public bool beingPulled;
    private bool attachedToGrapplePoint = false;
    public bool powerBoosted = false;


    private void Start()
    {
        myRB = GetComponent<Rigidbody2D>();

        defaultGravityScale = myRB.gravityScale;
        // Feels better to set gravity scale to 0 when throwing in 2D Game
        myRB.gravityScale = 0f;
        // Set our throw start point
        startPoint = transform.position;

        if (worldPickUp)
        {
            landed = true;
            SetEnemyIgnoresOnLand();
        }
        else
            // Weapon is not landed since it is just thrown
            landed = false;
    }
    private void FixedUpdate()
    {
        ItemPull();
        WeaponThrow();
        //PlayerPull();
    }

    private void WeaponThrow()
    {
        // Rotation if weapon is still in air
        if (!landed)
        {
            if (myRB.velocity.x > 0)
                transform.Rotate(new Vector3(0f, 0f, -rotSpeed * Time.fixedDeltaTime));
            else if (myRB.velocity.x < 0)
                transform.Rotate(new Vector3(0f, 0f, rotSpeed * Time.fixedDeltaTime));
        }
        // Calculation when we reach end point
        if ((transform.position - startPoint).magnitude >= maxDistance && !beingPulled && !attachedToGrapplePoint)
        {
            myRB.gravityScale = defaultGravityScale;

            // Stop weapon from moving in x direction
            //myRB.velocity = new Vector2(0f, myRB.velocity.y);
        }
    }

    private void ItemPull()
    {
        if (beingPulled)
        {
            Physics2D.IgnoreLayerCollision(3, 13);
            myRB.constraints = ~RigidbodyConstraints2D.FreezePosition;
            // Rotating object to point player
            Vector3 vectorToTarget = pullingObject.transform.position - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * pullForce);

            // Moving object to player
            myRB.velocity = vectorToTarget.normalized * pullForce * Time.deltaTime;
            attachedToGrapplePoint = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ground
        Debug.Log(collision.collider.gameObject.name);
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!landed)
            {
                myRB.gravityScale = defaultGravityScale;
                // Ricochet quickmaths
                Vector2 tmp = new Vector2(collision.contacts[0].point.x - collision.transform.position.x, collision.contacts[0].point.y - collision.transform.position.y);
                myRB.velocity = tmp.normalized + new Vector2(0, ricochetYImpulse) * ricochetImpulseForce;

                landed = true;
                SetEnemyIgnoresOnLand();
            }
        }
        // Enemy
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("Enemy hit");
            // Hits enemy when can deal damage
            if (!landed)
            {
                // Ricochet quickmaths
                Vector2 tmp = new Vector2(collision.contacts[0].point.x - collision.transform.position.x , collision.contacts[0].point.y - collision.transform.position.y);
                myRB.velocity = tmp.normalized + new Vector2(0, ricochetYImpulse) * ricochetImpulseForce;

                // If this is somehow not default set it here to be sure
                if(myRB.gravityScale != defaultGravityScale)
                    myRB.gravityScale = defaultGravityScale;

                // Knockback
                Knockback(gameObject, collision.gameObject, knockbackForce);
                // Deal damage last if we kill enemy we lose collision parameter
                DealDamage(collision.collider);
                // Cant deal damage twice
                landed = true;
                // Ignore enemy collisions
                SetEnemyIgnoresOnLand();
            }      
        }
        // Boss
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Boss"))
        {
            Debug.Log("Boss");
            // Hits enemy when can deal damage
            if (!landed)
            {
                // Ricochet quickmaths
                Vector2 tmp = new Vector2(collision.contacts[0].point.x - collision.transform.position.x, collision.contacts[0].point.y - collision.transform.position.y);
                myRB.velocity = tmp.normalized + new Vector2(0, ricochetYImpulse) * ricochetImpulseForce;

                // If this is somehow not default set it here to be sure
                if (myRB.gravityScale != defaultGravityScale)
                    myRB.gravityScale = defaultGravityScale;

                // Deal damage last if we kill enemy we lose collision parameter
                DealDamage(collision.collider);
                // Cant deal damage twice
                landed = true;
                // Ignore enemy collisions
                SetEnemyIgnoresOnLand();
            }
        }
        //// WeakPoint
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("BossWeakPoint"))
        {
            Debug.Log("WeakPoint");
            // Hits enemy when can deal damage
            if (!landed)
            {
                // Ricochet quickmaths
                Vector2 tmp = new Vector2(collision.contacts[0].point.x - collision.transform.position.x, collision.contacts[0].point.y - collision.transform.position.y);
                myRB.velocity = tmp.normalized + new Vector2(0, ricochetYImpulse) * ricochetImpulseForce;

                // If this is somehow not default set it here to be sure
                if (myRB.gravityScale != defaultGravityScale)
                    myRB.gravityScale = defaultGravityScale;

                // Deal damage last if we kill enemy we lose collision parameter
                DealDamage(collision.collider);
                // Cant deal damage twice
                landed = true;
                // Ignore enemy collisions
                SetEnemyIgnoresOnLand();
            }
        }
        // Collision with GrapplePoint
        else if(collision.collider.gameObject.layer == LayerMask.NameToLayer("GrapplePoint"))
        {
            // Makes the player and melee weapon to collide until it is pulled again. Weapon can be used as a platform during grapple.
            Physics2D.IgnoreLayerCollision(3, 13, false);

            // Change the meleeweapon layer to Ground here! Player can stand adn jump on the weapon while attached to the grapple point.
            gameObject.layer = 6;

            // We are attached to a grappling point.
            attachedToGrapplePoint = true;
            landed = true;
            SetEnemyIgnoresOnLand();

            // Calculations for the angle where the weapon hit the block.
            // Gets the last contact point from the list and uses it to calculate the angle. Is a bit more accurate than the first contact point to get the desired result.
            int lastContact = collision.contactCount;
            Vector2 normal = collision.GetContact(lastContact - 1).normal;
            float collisionAngle = Vector2.SignedAngle(Vector2.right, normal);
            //Debug.Log(collisionAngle);

            myRB.gravityScale = 0f;

            // Makes the desired calculations for the weapon to attach to the point correctly. Does not work perfectly at the moment.
            if (collisionAngle <= 45 && collisionAngle >= -45)
            {
                //Debug.Log("Right");

                transform.position = new Vector2(collision.transform.position.x + (collision.transform.localScale.x / meleeWeaponGrapplingDistance), collision.transform.position.y);
                Quaternion q = Quaternion.AngleAxis(collisionAngle, Vector3.forward);

                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * pullForce);
                myRB.constraints = RigidbodyConstraints2D.FreezePosition;
                myRB.freezeRotation = true;

            }
            else if (collisionAngle < -45 && collisionAngle > -135)
            {
                //Debug.Log("Bottom");

                transform.position = new Vector2(collision.transform.position.x, collision.transform.position.y - (collision.transform.localScale.y / meleeWeaponGrapplingDistance));
                Quaternion q = Quaternion.AngleAxis(collisionAngle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * pullForce);
                myRB.constraints = RigidbodyConstraints2D.FreezePosition;
                myRB.freezeRotation = true;
            }
            else if (collisionAngle <= -135 || collisionAngle > 135)
            {
                //Debug.Log("Left");

                transform.position = new Vector2(collision.transform.position.x - (collision.transform.localScale.x / meleeWeaponGrapplingDistance), collision.transform.position.y);
                Quaternion q = Quaternion.AngleAxis(collisionAngle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * pullForce);
                myRB.constraints = RigidbodyConstraints2D.FreezePosition;
                myRB.freezeRotation = true;
            }
            else if (collisionAngle > 45 && collisionAngle <= 135)
            {
                //Debug.Log("Top");

                transform.position = new Vector2(collision.transform.position.x, collision.transform.position.y + (collision.transform.localScale.y / meleeWeaponGrapplingDistance));
                Quaternion q = Quaternion.AngleAxis(collisionAngle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * pullForce);
                myRB.constraints = RigidbodyConstraints2D.FreezePosition;
                myRB.freezeRotation = true;
            }
        }
    }


    // Called from PlayerMeleeCombat
    public void PullWeapon(GameObject objectThatPulls)
    {
        // Weapon cannot deal damage aka hit enemy or ground
        if (landed)
        {
            // Ignore layers that should't collide
            SetEnemyIgnoresOnPull();

            myRB.velocity = Vector2.zero; // Stop moving at the start of pulling physics bugs

            //Layer to PulledWeapon.
            gameObject.layer = 15;


            pullingObject = objectThatPulls;
            beingPulled = true;           
        }
    }

    private void SetEnemyIgnoresOnPull()
    {
        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon"), true);

        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("MeleeWeapon"), true);

        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("GrapplePoint"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GrapplePoint"), LayerMask.NameToLayer("MeleeWeapon"), true);

        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Boss"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Boss"), LayerMask.NameToLayer("MeleeWeapon"), true);

        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("BossWeakPoint"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("BossWeakPoint"), LayerMask.NameToLayer("MeleeWeapon"), true);
    }

    // Ignore enemylayer colliders when we land
    private void SetEnemyIgnoresOnLand()
    {
        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon"), true);

        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Boss"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Boss"), LayerMask.NameToLayer("MeleeWeapon"), true);

        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("BossWeakPoint"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("BossWeakPoint"), LayerMask.NameToLayer("MeleeWeapon"), true);
    }

    // Deal damage to given Collider2D
    public void DealDamage(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("BossWeakPoint"))
        {
            if (powerBoosted)
                col.gameObject.GetComponentInParent<Health>().TakeDamage(powerBoostedDamage * weakPointMultiplier);
            else if (beingPulled)
                col.gameObject.GetComponentInParent<Health>().TakeDamage(weaponPullDamage * weakPointMultiplier);
            else
                col.gameObject.GetComponentInParent<Health>().TakeDamage(weaponThrowDamage * weakPointMultiplier);
        }
        else
        {
            if (powerBoosted)
                col.gameObject.GetComponentInParent<Health>().TakeDamage(powerBoostedDamage);
            else if (beingPulled)
                col.gameObject.GetComponentInParent<Health>().TakeDamage(weaponPullDamage);
            else
                col.gameObject.GetComponentInParent<Health>().TakeDamage(weaponThrowDamage);
        }
    }

    public void Knockback(GameObject target, GameObject from, float knockbackForce)
    {
        float pushbackX = target.transform.position.x - from.transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Mathf.Abs(pushbackX / 4)).normalized;
        target.GetComponent<Rigidbody2D>().AddForce(knockbackDirection * knockbackForce);
    }

    // Called as event from player if interacted or when weapon is pulled and hits player
    public void Interact(GameObject objectWhoPicksUp)
    {
        Debug.Log("Pick up");
        // Set collision detection back if this was set to ignore
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("MeleeWeapon"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GrapplePoint"), LayerMask.NameToLayer("MeleeWeapon"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Boss"), LayerMask.NameToLayer("MeleeWeapon"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("BossWeakPoint"), LayerMask.NameToLayer("MeleeWeapon"), false);
        // If we are not attached to the grapple point, make the weapon pickable.
        if (!attachedToGrapplePoint || (attachedToGrapplePoint && objectWhoPicksUp.GetComponent<PlayerCombat>().getIsPlayerBeingPulled()))
        {
            // Inform Player to pickup
            objectWhoPicksUp.GetComponent<PlayerCombat>().PickUpWeapon();

            //Player was being pulled towards the weapon. Change the bool back to false and gravityscale to normal.
            objectWhoPicksUp.GetComponent<PlayerCombat>().setIsPlayerBeingPulled(false);
            objectWhoPicksUp.GetComponent<Rigidbody2D>().gravityScale = 5f;
            // Destroy instance form scene
            Destroy(gameObject);

        }
    }

    public void ActivatePowerBoost(Vector2 direction)
    {
        beingPulled = false; // No longer pulled
        powerBoosted = true; // Now power boosted
        gameObject.layer = 13; // Layer back to MeleeWeapon.
        mySpriteRenderer.color = Color.blue; // Visualize power boost
        GetComponent<TrailRenderer>().enabled = true;

        myRB.constraints = RigidbodyConstraints2D.FreezePositionY;
        myRB.velocity = direction.normalized * powerBoostForce;
    }

    public void StopPowerBoost()
    {
        powerBoosted = false;
        myRB.constraints = RigidbodyConstraints2D.None;
        mySpriteRenderer.color = Color.red;
        GetComponent<TrailRenderer>().enabled = false;
    }

    public bool isPowerBoosted()
    {
        return powerBoosted;
    }

    public Vector2 getDirection()
    {
        return myRB.velocity;
    }

    public bool getBeingPulled()
    {
        return beingPulled;
    }

    public bool getLanded()
    {
        return landed;
    }

    public void setMaxDistance(float f)
    {
        maxDistance = f;
    }
    public float getMaxDistance()
    {
        return maxDistance;
    }

    public bool getAttachedToGrapplePoint()
    {
        return attachedToGrapplePoint;
    }

    // ---- SAVING / LOADING ----
    public void setThrowDmg(float dmg)
    {
        weaponThrowDamage = dmg;
    }
    public float getThrowDmg()
    {
        return weaponThrowDamage;
    }

    public void setPullDmg(float dmg)
    {
        weaponPullDamage = dmg;
    }
    public float getPullDmg()
    {
        return weaponPullDamage;
    }

    public void setPowerBoostDmg(float dmg)
    {
        powerBoostedDamage = dmg;
    }
    public float getPowerBoostDmg()
    {
        return powerBoostedDamage;
    }

    // ---- UPGRADES ----
    // Called from PlayerCombat since it is only link to this prefab object
    public void UpgradeThrowDamage(float dmg)
    {
        weaponThrowDamage += dmg;
    }

    public void UpgradePullDamage(float dmg)
    {
        weaponPullDamage += dmg;
    }

    public void UpgradePowerBoostedDamage(float dmg)
    {
        powerBoostedDamage += dmg;
    }
}