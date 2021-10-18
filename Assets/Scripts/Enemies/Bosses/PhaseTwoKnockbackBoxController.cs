using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseTwoKnockbackBoxController : MonoBehaviour
{
    [SerializeField] private Vector2 velocityPlayer;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float chargeReadyTime;

    private bool knockbackOnCooldown = false;

    private void Start()
    {
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
    }

    void PlayerPushback()
    {
        //float pushbackX = (target.position.x - transform.position.x > 0 ? 1 : -1);

        //Vector2 knockbackDirection = new Vector2(pushbackX, 0);
        //playerRB.AddForce(knockbackDirection * knockbackForce * Time.deltaTime);
        velocityPlayer = new Vector2(playerRB.position.x - transform.position.x > 0 ? knockbackForce * 1 : knockbackForce * -1, 0);
        playerRB.MovePosition(playerRB.position + velocityPlayer * Time.deltaTime);
        StartCoroutine(KnockbackCooldown());
    }

    // Cooldown for the player knockback.
    private IEnumerator KnockbackCooldown()
    {
        knockbackOnCooldown = true;
        yield return new WaitForSeconds(chargeReadyTime);
        knockbackOnCooldown = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Pushes player back when collider is hit and knockback is not on cooldown.
        if (collision.collider.tag == "Player" && !knockbackOnCooldown)
        {
            PlayerPushback();
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        // If player stays in contact with the boss, knockback.
        if (collision.collider.tag == "Player" && !knockbackOnCooldown)
        {
            PlayerPushback();
        }
    }
}
