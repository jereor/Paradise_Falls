using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // This script will handle player collisions

    private void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "MovingPlatform")
        {
            // Tries to move the player with moving platform, but does not work properly (not at all).
            //transform.SetParent(collision.gameObject.transform);
            //rb.velocity = collision.gameObject.GetComponent<Rigidbody2D>().velocity;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "MovingPlatform")
        {
            //transform.SetParent(collision.gameObject.transform);
            //rb.velocity = collision.gameObject.GetComponent<Rigidbody2D>().velocity;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "MovingPlatform")
        {
            transform.SetParent(GameObject.Find("[Gameplay]").transform);
        }
    }
}
