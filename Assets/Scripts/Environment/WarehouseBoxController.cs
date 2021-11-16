using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseBoxController : MonoBehaviour
{
    private RiotControlDrone drone;
    [SerializeField] private GameObject box;
    private GameObject boxInstance;
    private float counter = 10;
    // Start is called before the first frame update
    void Start()
    {
        drone = GameObject.Find("RiotControlDrone").GetComponent<RiotControlDrone>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(boxInstance == null)
        {
            counter += Time.deltaTime;
        }
        if(boxInstance == null && counter > 10) // Instance a new box to replace the previous one, if it was destroyed and enough time has passed.
        {
            boxInstance = Instantiate(box, transform.position, Quaternion.identity);
            counter = 0;
        }
        if(drone.state == RiotControlDrone.RiotState.PhaseTwoRun)
        {
            if (boxInstance != null)
            {
                Destroy(boxInstance);               
            }
            gameObject.SetActive(false);

        }
    }
}
