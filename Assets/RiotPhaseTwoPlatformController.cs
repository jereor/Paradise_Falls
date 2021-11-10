using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotPhaseTwoPlatformController : MonoBehaviour
{
    [SerializeField] private List<Transform> firstBoxSet;
    [SerializeField] private List<Transform> secondBoxSet;
    [SerializeField] private List<Transform> firstEnemySet;
    [SerializeField] private List<Transform> secondEnemySet;
    private Transform firstChild;
    private Transform secondChild;
    private Transform thirdChild;
    private Transform fourthChild;
    // Start is called before the first frame update
    void Start()
    {
        // Get all child gameobjects for activation.
        firstChild = gameObject.transform.GetChild(0).transform;
        secondChild = gameObject.transform.GetChild(1).transform;
        thirdChild = gameObject.transform.GetChild(2).transform;
        fourthChild = gameObject.transform.GetChild(3).transform;

        // Find the list of gameobjects that needs to be spawned/activated.
        foreach(Transform platform in firstChild.GetComponentInChildren<Transform>())
        {
            firstBoxSet.Add(platform);
        }

        foreach (Transform platform in secondChild.GetComponentInChildren<Transform>())
        {
            secondBoxSet.Add(platform);
        }

        foreach (Transform enemy in thirdChild.GetComponentInChildren<Transform>())
        {
            firstEnemySet.Add(enemy);
        }

        foreach (Transform enemy in fourthChild.GetComponentInChildren<Transform>())
        {
            secondEnemySet.Add(enemy);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateFirstSet() // Activate first set of boxes and enemies.
    {
        foreach(Transform platform in firstBoxSet)
        {
            platform.GetComponent<DisappearingPlatform>().Work();
        }

        foreach(Transform enemy in firstEnemySet)
        {
            enemy.gameObject.SetActive(true);
        }
    }

    public void ActivateSecondSet() // Activate second set of boxes and enemies.
    {
        foreach (Transform platform in firstBoxSet)
        {
            platform.GetComponent<DisappearingPlatform>().Work();
        }
        foreach (Transform platform in secondBoxSet)
        {
            platform.GetComponent<DisappearingPlatform>().Work();
        }
        foreach (Transform enemy in secondEnemySet)
        {
            enemy.gameObject.SetActive(true);
        }
    }

    public void BeginThirdPhase() // Deactivate all boxes and set the phase 2 complete when the last button is pressed.
    {
        foreach (Transform platform in secondBoxSet)
        {
            platform.GetComponent<DisappearingPlatform>().Work();
        }
        GameObject.Find("RiotControlDrone").GetComponent<RiotControlDrone>().setPhaseTwoComplete(true);
    }
}

// Activate platforms -> Button press -> Laser Shoots -> Platforms disappear -> Next set appears -> Repeat
