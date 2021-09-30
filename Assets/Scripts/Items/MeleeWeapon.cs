using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Interactable
{
    [Header("Variables from This script")]
    [SerializeField] private float weaponThrowDamage; // Damage dealt if hits enemy
    [SerializeField] private float rotAngle; // Rotation angle to spin when thowing
    [SerializeField] private float ricochetForce; // Force of hit ricochet on enemies and gorund elements
    [SerializeField] private float pullSpeed;
    [SerializeField] private float maxDistance; // Max distance to travel with gravityscale 0 and deal damage

    [SerializeField] private bool playerIsClose; // ATM used for debugging
    //[SerializeField] private bool isFlying = true;
    private PlayerCombat combatScript;
    private float defaultGravityScale;
    private Vector3 startPoint;
    private bool canDealDamage;
    private bool beingPulled;
    private GameObject puller;

    private Rigidbody2D myRB;
    private void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
        defaultGravityScale = myRB.gravityScale;
        myRB.gravityScale = 0f;
        // Set our throw start point
        startPoint = transform.position;

        // Weapon can deal damage since it is just thrown
        canDealDamage = true;

        // Hide text
        HideFloatingText();

        // We need to AddThisLister here because this is not Instantiated on Scene load
        itemEvent.AddListener(Interact);
    }
    private void Update()
    {
        if (beingPulled)
        {
            transform.position = Vector2.MoveTowards(transform.position, puller.transform.position, pullSpeed * Time.deltaTime);

            Vector3 vectorToTarget = puller.transform.position - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * pullSpeed);
        }
        if (canDealDamage)
        {
            Debug.Log("Flying");
            transform.Rotate(new Vector3(0f,0f, - rotAngle * Time.deltaTime));
        }
        if ((transform.position - startPoint).magnitude >= maxDistance && !beingPulled)
        {
            Debug.Log("Max d");
            myRB.gravityScale = defaultGravityScale;
            canDealDamage = false;
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
            // Mark for this item that player is close (easier to track interactions when debugging)
            playerIsClose = true;

            // If we pulled and hit player -> pick up
            if (beingPulled)
            {
                Interact();
            }

            ShowFloatingText();
        }
        // Ground
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // While weapon is beingPulled we hit ground
            if (beingPulled)
            {
                Debug.Log(collision.contacts[0]);
                Vector2 tmp = new Vector2(transform.position.x, transform.position.y) - collision.contacts[0].point;
                Debug.Log(tmp);
                myRB.AddForce(tmp * ricochetForce, ForceMode2D.Impulse);
            }
            // We hit ground we lose momentum -> no damage
            else
            {
                myRB.gravityScale = defaultGravityScale;
                canDealDamage = false;
            }
        }
        // Enemy
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {          
            // Hits enemy when flying and can deal damage
            if (canDealDamage)
            {
                StopRBForce();
                collision.gameObject.GetComponent<Health>().TakeDamage(weaponThrowDamage);

                Debug.Log("Bonk " + collision.gameObject.name);
                myRB.gravityScale = defaultGravityScale;
                
                myRB.AddForce((transform.position - collision.gameObject.transform.position).normalized + new Vector3(0,1,0) * ricochetForce, ForceMode2D.Impulse);
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
            playerIsClose = false;

            HideFloatingText();
        }
    }

    // Called from PlayerMeleeCombat
    public void PullWeapon(GameObject objectThatPulls)
    {
        if (!canDealDamage)
        {
            myRB.velocity = Vector2.zero;
            myRB.gravityScale = 0f;

            puller = objectThatPulls;
            beingPulled = true;
        }
    }

    private void StopRBForce()
    {
        myRB.velocity = Vector2.zero;
        myRB.angularVelocity = 0f;
    }

    // Called as event from player if interacted or when weapon is pulled and hits player
    public override void Interact()
    {
        Debug.Log("Pick up");
        combatScript.PickUpWeapon();
        Destroy(gameObject);
    }
}
