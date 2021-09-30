using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Interactable
{
    [Header("Variables from This script")]
    [SerializeField] private float weaponThrowDamage; // Damage dealt if hits enemy
    [SerializeField] private float rotAngle; // Rotation angle to spin when thowing
    [SerializeField] private float ricochetForce; // Force of hit ricochet on enemies and gorund elements
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
        startPoint = transform.position;

        canDealDamage = true;

        HideFloatingText();

        itemEvent.AddListener(Interact);
    }
    private void Update()
    {
        if (beingPulled)
        {
            transform.position = Vector2.MoveTowards(transform.position, puller.transform.position, 10 * Time.deltaTime);
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(puller.transform.position), Time.deltaTime * 10);
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
        if (collision.gameObject.layer == 3)
        {
            // Gives persmission to save and gives GameObject that this script is attached so PlayerInteraction know of whos Interact() to Invoke
            collision.gameObject.GetComponent<PlayerInteractions>().AllowInteract(true);
            collision.gameObject.GetComponent<PlayerInteractions>().GiveGameObject(gameObject);
            combatScript = collision.gameObject.GetComponent<PlayerCombat>();
            // Mark for this item that player is close (easier to track interactions when debugging)
            playerIsClose = true;

            if (beingPulled)
            {
                Interact();
            }

            ShowFloatingText();
        }
        // Ground
        else if (collision.gameObject.layer == 6)
        {
            if (beingPulled)
            {
                Vector2 tmp = new Vector2(transform.position.x, transform.position.y) - collision.GetContact(0).point;
                myRB.AddForce(tmp.normalized * ricochetForce, ForceMode2D.Impulse);
            }
            else
            {
                canDealDamage = false;
            }
        }
        // Enemy
        else if (collision.gameObject.layer == 7)
        {          
            // Hits enemy when flying and can deal damage
            if (canDealDamage)
            {
                collision.gameObject.GetComponent<Health>().TakeDamage(weaponThrowDamage);

                Debug.Log("Bonk " + collision.gameObject.name);
                myRB.gravityScale = defaultGravityScale;
                
                myRB.AddForce((transform.position - collision.gameObject.transform.position).normalized + new Vector3(0,2,0) * ricochetForce, ForceMode2D.Impulse);
            }
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

    // Called as event from player if interacted or when weapon is pulled and hits player
    public override void Interact()
    {
        Debug.Log("Pick up");
        combatScript.PickUpWeapon();
        Destroy(gameObject);
    }
}
