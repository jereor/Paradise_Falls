using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Variables from This script")]
    [SerializeField] private float weaponThrowDamage; // Damage dealt if hits enemy
    [SerializeField] private float weaponPullDamage;
    [SerializeField] private float rotSpeed; // Rotation angle to spin when thowing
    [SerializeField] private float ricochetImpulseForce; // Force of hit ricochet on enemies and gorund elements
    [SerializeField] private float ricochetYImpulse; // Float parameter if we want to ricochet weapon slightly upward feels better and tell player that we hit and dealt damage to something
    [SerializeField] private float pullForce; // Force we are pulling
    [SerializeField] private float maxDistance; // Max distance to travel with gravityscale 0
    [SerializeField] private float meleeWeaponGrapplingDistance;

    [SerializeField] private bool worldPickUp;

    public float knockbackForce;

    // Other variables
    private Rigidbody2D myRB;
    private float defaultGravityScale;

    private Vector3 startPoint; // Used to calculate maximum distance to travel
    private bool landed; // If weapon can deal damage

    private GameObject pullingObject; // Object that is pulling given in PullWeapon()
    private bool beingPulled;
    private bool attachedToGrapplePoint = false;

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
            SetEnemyIngoresOnLand();
        }
        else
            // Weapon is not landed since it is just thrown
            landed = false;
    }
    private void FixedUpdate()
    {
        ItemPull();
        WeaponThrow();
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

    // Use this if you want to pull player character towards the grapple point when the multitool is attached to it.
    private void playerPull()
    {
        if (beingPulled)
        {
            // Make the player and weapon ignore collisions again, which were declined in collision check after the weapon hit the grapple point.
            Physics2D.IgnoreLayerCollision(3, 13);

            // During grapple all constraints were freezed so the player couldn't push the weapon around when attached to the point.
            // Here we unfreeze the position. Rotation is unfreezed automatically when being pulled again.
            myRB.constraints = ~RigidbodyConstraints2D.FreezePosition;

            // Rotating object to point player
            Vector3 vectorToTarget =  transform.position - pullingObject.transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * pullForce);

            // Moving object to player
            pullingObject.GetComponent<Rigidbody2D>().velocity = vectorToTarget.normalized * pullForce * Time.deltaTime;

            // Weapon is not attached to the grapple point anymore.
            attachedToGrapplePoint = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!landed)
            {
                myRB.gravityScale = defaultGravityScale;
                // Ricochet quickmaths
                Vector2 tmp = new Vector2(collision.contacts[0].point.x - collision.transform.position.x, collision.contacts[0].point.y - collision.transform.position.y);
                myRB.velocity = tmp.normalized + new Vector2(0, ricochetYImpulse) * ricochetImpulseForce;

                landed = true;
                SetEnemyIngoresOnLand();
            }
        }
        // Enemy
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {          
            // Hits enemy when can deal damage
            if (!landed)
            {
                // Ricochet quickmaths
                Vector2 tmp = new Vector2(collision.contacts[0].point.x - collision.transform.position.x , collision.contacts[0].point.y - collision.transform.position.y);
                myRB.velocity = tmp.normalized + new Vector2(0, ricochetYImpulse) * ricochetImpulseForce;

                // If this is somehow not default set it here to be sure
                if(myRB.gravityScale != defaultGravityScale)
                    myRB.gravityScale = defaultGravityScale;

                // Deal damage last if we kill enemy we lose collision parameter
                DealDamage(collision.collider);
                // Knockback
                Knockback(gameObject, collision.gameObject, knockbackForce);
                // Cant deal damage twice
                landed = true;
                // Ignore enemy collisions
                SetEnemyIngoresOnLand();
            }      
        }
        // Collision with GrapplePoint
        else if(collision.gameObject.layer == LayerMask.NameToLayer("GrapplePoint"))
        {
            // Makes the player and melee weapon to collide until it is pulled again. Weapon can be used as a platform during grapple.
            Physics2D.IgnoreLayerCollision(3, 13, false);

            // TODO: Change grapple point and meleeweapon layers to Ground here!
            //gameObject.layer = 6;
            //collision.gameObject.layer = 6;

            // We are attached to a grappling point.
            attachedToGrapplePoint = true;
            landed = true;
            SetEnemyIngoresOnLand();

            // Calculations for the angle where the weapon hit the block.
            Vector2 normal = collision.GetContact(0).normal;
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
            SetEnemyIngoresOnPull();

            myRB.velocity = Vector2.zero; // Stop moving at the start of pulling physics bugs

            pullingObject = objectThatPulls;
            beingPulled = true;           
        }
    }

    private void SetEnemyIngoresOnPull()
    {
        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon"), true);

        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("MeleeWeapon"), true);

        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("GrapplePoint"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GrapplePoint"), LayerMask.NameToLayer("MeleeWeapon"), true);
    }

    // Ignore enemylayer colliders when we land
    private void SetEnemyIngoresOnLand()
    {
        if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon")))
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("MeleeWeapon"), true);
    }

    // Deal damage to given Collider2D
    public void DealDamage(Collider2D col)
    {
        if (beingPulled)
        {
            //Debug.Log("Dealing Pull Damage");
            col.gameObject.GetComponent<Health>().TakeDamage(weaponPullDamage);
        }
        else
        {
            //Debug.Log("Dealing Throw Damage");
            col.gameObject.GetComponent<Health>().TakeDamage(weaponThrowDamage);
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

        // If we are not attached to the grapple point, make the weapon pickable.
        if(!attachedToGrapplePoint)
        {
            // Inform Player to pickup
            objectWhoPicksUp.GetComponent<PlayerCombat>().PickUpWeapon();
            // Destroy instance form scene
            Destroy(gameObject);

        }

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
}