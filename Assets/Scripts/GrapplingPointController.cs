using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GrapplingPointController : MonoBehaviour
{
    public bool canUseMultitoolForGrappling = false;
    public float meleeWeaponGrapplingDistance = 0.5f;
    public Rigidbody2D meleeWeapon;

    // Start is called before the first frame update
    void Start()
    {
        //meleeWeapon = GameObject.Find("MeleeWeapon");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //meleeWeapon = GameObject.Find("MeleeWeapon");
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if(collision.gameObject.layer == LayerMask.NameToLayer("MeleeWeapon"))
    //    {
    //        Vector2 normal = collision.GetContact(0).normal;
    //        float collisionAngle = Vector2.SignedAngle(Vector2.right, -normal);
    //        //Debug.Log(collisionAngle);
    //        if (collisionAngle <= 45 && collisionAngle >= -45)
    //        {
    //            Debug.Log("Right");
    //            meleeWeapon.transform.DORotate(new Vector3(0, 0, 0), 0);
    //            meleeWeapon.velocity = new Vector2(0,0);
    //            meleeWeapon.transform.position = new Vector2(transform.position.x + meleeWeaponGrapplingDistance, transform.position.y);
    //        }
    //        else if (collisionAngle < -45 && collisionAngle > -135)
    //        {
    //            Debug.Log("Bottom");
    //            meleeWeapon.transform.DORotate(new Vector3(0, 0, -90), 0);
    //            meleeWeapon.velocity = new Vector2(0, 0);
    //            meleeWeapon.transform.position = new Vector2(transform.position.x, transform.position.y - meleeWeaponGrapplingDistance);
    //        }
    //        else if (collisionAngle <= -135 || collisionAngle > 135)
    //        {
    //            Debug.Log("Left");
    //            meleeWeapon.transform.DORotate(new Vector3(0, 0, 180), 0);
    //            meleeWeapon.velocity = new Vector2(0, 0);
    //            meleeWeapon.transform.position = new Vector2(transform.position.x - meleeWeaponGrapplingDistance, transform.position.y);
    //        }
    //        else if (collisionAngle > 45 && collisionAngle <= 135)
    //        {
    //            Debug.Log("Top"); 
    //            meleeWeapon.transform.DORotate(new Vector3(0, 0, 90), 0);
    //            meleeWeapon.velocity = new Vector2(0, 0);
    //            meleeWeapon.transform.position = new Vector2(transform.position.x, transform.position.y + meleeWeaponGrapplingDistance);
    //        }
    //    }

    //}
}
