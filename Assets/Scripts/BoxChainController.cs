using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxChainController : MonoBehaviour
{
    private RiotBossBoxMover box;
    // Start is called before the first frame update
    void Start()
    {
        box = GetComponentInParent<RiotBossBoxMover>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "MeleeWeapon" && box.getIsBoxColliderEnabled())
        {

            Destroy(gameObject);

        }
    }
}
