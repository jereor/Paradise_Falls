using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingVineController : MonoBehaviour
{
    private GameObject target;
    private Health targetHealth;
    private GameObject plantBoss;

    float counter = 0;

    Quaternion q;
    Quaternion endRotation;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player");
        targetHealth = target.GetComponent<Health>();
        plantBoss = GameObject.Find("PlantBoss");

        Vector3 vectorToTarget = target.transform.position - transform.parent.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        q = Quaternion.AngleAxis(angle, Vector3.forward);
        endRotation = Quaternion.AngleAxis(angle+45, Vector3.forward);
    }

    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        //transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, endRotation, Time.deltaTime * 100);
        //transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, endRotation, Time.deltaTime);
        transform.RotateAround(plantBoss.transform.position, Vector3.forward, Time.deltaTime * 720);

        if (counter > 0.5)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.name == "Player" && !plantBoss.GetComponent<BigPlantController>().GetKockbackOnCooldown())
        {
            plantBoss.GetComponent<BigPlantController>().PlayerPushback();
            targetHealth.TakeDamage(1);
        }
    }
}
