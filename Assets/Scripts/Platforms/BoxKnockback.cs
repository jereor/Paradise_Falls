using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxKnockback : MonoBehaviour
{
    private bool dealtDmg = false;
    private bool falling = false;

    [Header("Player knockback control")]
    [SerializeField] private Vector2 pushDir;
    [SerializeField] private Vector2 velocityPlayer;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float dmgToPlayer;

    private BoxSFX boxSFX;

    private void Start()
    {
        boxSFX = gameObject.GetComponentInParent<BoxSFX>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && !dealtDmg && falling)
        {
            PlayerPushback(collision.GetComponent<Rigidbody2D>());
            // Deal dmg to player if we set some dmg
            collision.gameObject.GetComponent<Health>().TakeDamage(dmgToPlayer);
            dealtDmg = true;
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            boxSFX.PlayHitGroundSound();
            falling = false;
        }
    }

    void PlayerPushback(Rigidbody2D playerRB)
    {
        // We havent set pushDir so push is to the left if Players pivot point is left side of box pivot point, vice versa
        if (pushDir.x == 0f)
        {
            velocityPlayer = new Vector2(playerRB.position.x - transform.position.x > 0 ? knockbackForce * 1 : knockbackForce * -1, 0);
            playerRB.MovePosition(playerRB.position + velocityPlayer * Time.deltaTime);
        }
        else
        {
            // Normal knockbackForce is enough to move player to desired pushDir
            if (Mathf.Sign(playerRB.transform.position.x - transform.position.x) == Mathf.Sign(pushDir.x))
                velocityPlayer = pushDir * knockbackForce;
            // We are left side of the box and pushDir is to the right we need more force 2x value works
            else
                velocityPlayer = pushDir * knockbackForce * 2f;
            playerRB.MovePosition(playerRB.position + velocityPlayer * Time.deltaTime);
        }
    }


    public void setFalling(bool b)
    {
        falling = true;
    }
}
