using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxChainController : MonoBehaviour
{
    private StaticMovingBox staticMovingBox;
    private BackAndForthMovingBox backAndForthBox;
    // Start is called before the first frame update
    void Start()
    {
        // Because this script is used in both BackAndForthMovingBox and StaticMovingBox scripts, assign the correct script to be used.
        if (GetComponentInParent<StaticMovingBox>() != null)
            staticMovingBox = GetComponentInParent<StaticMovingBox>();
        else if (GetComponentInParent<BackAndForthMovingBox>() != null)
            backAndForthBox = GetComponentInParent<BackAndForthMovingBox>();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(staticMovingBox != null)
        {
            if (collision.gameObject.tag == "MeleeWeapon" && staticMovingBox.getIsBoxColliderEnabled() && staticMovingBox.getIsCuttableChain())
            {
                // Destroy the chain if collided with MeleeWeapon. Check if the collider is enabled and the chain is cuttable.
                transform.parent.gameObject.tag = "Box";
                transform.parent.gameObject.layer = LayerMask.GetMask("Breakable");
                Destroy(gameObject);

            }
        }
        else if (backAndForthBox != null)
        {
            if (collision.gameObject.tag == "MeleeWeapon" && backAndForthBox.getIsBoxColliderEnabled() && backAndForthBox.getIsCuttableChain())
            {
                transform.parent.gameObject.tag = "Box";
                transform.parent.gameObject.layer = LayerMask.GetMask("Breakable");
                Destroy(gameObject);

            }
        }

    }
}
