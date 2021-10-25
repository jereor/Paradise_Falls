using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseBoxController : MonoBehaviour
{
    [SerializeField] private GameObject box;
    private GameObject boxInstance;
    private float counter = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        counter += Time.deltaTime;
        if(boxInstance == null && counter > 5)
        {
            boxInstance = Instantiate(box, transform.position, Quaternion.identity);
            counter = 0;
        }
    }
}
