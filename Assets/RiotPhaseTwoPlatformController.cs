using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotPhaseTwoPlatformController : MonoBehaviour
{
    [SerializeField] private List<Transform> firstSet;
    [SerializeField] private List<Transform> secondSet;
    private Transform firstChild;
    private Transform secondChild;
    // Start is called before the first frame update
    void Start()
    {
        firstChild = gameObject.transform.GetChild(0).transform;
        secondChild = gameObject.transform.GetChild(1).transform;

        foreach(Transform platform in firstChild.GetComponentInChildren<Transform>())
        {
            firstSet.Add(platform);
        }

        foreach (Transform platform in secondChild.GetComponentInChildren<Transform>())
        {
            secondSet.Add(platform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateFirstSet()
    {
        foreach(Transform platform in firstSet)
        {
            platform.GetComponent<DisappearingPlatform>().Work();
        }
    }

    public void ActivateSecondSet()
    {
        foreach (Transform platform in firstSet)
        {
            platform.GetComponent<DisappearingPlatform>().Work();
        }
        foreach (Transform platform in secondSet)
        {
            platform.GetComponent<DisappearingPlatform>().Work();
        }
    }

    public void DeactivateSecondSet()
    {
        foreach (Transform platform in secondSet)
        {
            platform.GetComponent<DisappearingPlatform>().Work();
        }
    }
}

// Activate platforms -> Button press -> Laser Shoots -> Platforms disappear -> Next set appears -> Repeat
