using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotPhaseTwoPlatformController : MonoBehaviour
{
    [SerializeField] private List<Transform> firstBoxes;
    [SerializeField] private List<Transform> secondBoxes;
    [SerializeField] private List<Transform> thirdBoxes;
    [SerializeField] private List<Transform> firstEnemies;
    [SerializeField] private List<Transform> secondEnemies;
    [SerializeField] private List<Transform> thirdEnemies;
    [SerializeField] private List<Transform> buttons;
    private Transform boxSetOne;
    private Transform boxSetTwo;
    private Transform boxSetThree;
    private Transform enemySetOne;
    private Transform enemySetTwo;
    private Transform enemySetThree;
    private Transform buttonSet;
    // Start is called before the first frame update
    void Start()
    {
        // Get all child gameobjects for activation.
        boxSetOne = gameObject.transform.GetChild(0).transform;
        boxSetTwo = gameObject.transform.GetChild(1).transform;
        boxSetThree = gameObject.transform.GetChild(2).transform;
        enemySetOne = gameObject.transform.GetChild(3).transform;
        enemySetTwo = gameObject.transform.GetChild(4).transform;
        enemySetThree = gameObject.transform.GetChild(5).transform;
        buttonSet = gameObject.transform.GetChild(6).transform;

        // Find the list of gameobjects that needs to be spawned/activated.
        foreach(Transform platform in boxSetOne.GetComponentInChildren<Transform>())
        {
            firstBoxes.Add(platform);
        }

        foreach (Transform platform in boxSetTwo.GetComponentInChildren<Transform>())
        {
            secondBoxes.Add(platform);
        }

        foreach (Transform platform in boxSetThree.GetComponentInChildren<Transform>())
        {
            thirdBoxes.Add(platform);
        }

        foreach (Transform enemy in enemySetOne.GetComponentInChildren<Transform>())
        {
            firstEnemies.Add(enemy);
        }

        foreach (Transform enemy in enemySetTwo.GetComponentInChildren<Transform>())
        {
            secondEnemies.Add(enemy);
        }

        foreach (Transform enemy in enemySetThree.GetComponentInChildren<Transform>())
        {
            thirdEnemies.Add(enemy);
        }

        foreach(Transform button in buttonSet)
        {
            buttons.Add(button);
        }


        //foreach (Transform platform in firstBoxes)
        //{
        //    platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        //    platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        //}
        //foreach (Transform platform in secondBoxes)
        //{
        //    platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        //    platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        //}
        //foreach (Transform platform in thirdBoxes)
        //{
        //    platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        //    platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        //}
    }

    private void FixedUpdate()
    {
        if(GameObject.Find("RiotControlDrone") == null)
        {
            foreach(Transform enemy in firstEnemies)
            {
                if(enemy != null)
                    Destroy(enemy.gameObject);
            }
            foreach (Transform enemy in secondEnemies)
            {
                if (enemy != null)
                    Destroy(enemy.gameObject);
            }
            foreach (Transform enemy in thirdEnemies)
            {
                if (enemy != null)
                    Destroy(enemy.gameObject);
            }
        }
    }

    public void ActivateFirstSet() // Activate first set of boxes and enemies.
    {
        foreach(Transform platform in firstBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        }

        foreach(Transform enemy in firstEnemies)
        {
            enemy.gameObject.SetActive(true);
        }

        buttons[0].transform.gameObject.SetActive(true);
    }

    public void ActivateSecondSet() // Activate second set of boxes and enemies.
    {
        buttons[0].transform.gameObject.SetActive(false);
        foreach (Transform platform in firstBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        }
        foreach (Transform platform in secondBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        }
        foreach (Transform enemy in secondEnemies)
        {
            enemy.gameObject.SetActive(true);
        }
        buttons[1].transform.gameObject.SetActive(true);
    }

    public void ActivateThirdSet() // Activate third set of boxes and enemies.
    {
        buttons[1].transform.gameObject.SetActive(false);
        foreach (Transform platform in secondBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        }
        foreach (Transform platform in thirdBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        }
        foreach (Transform enemy in thirdEnemies)
        {
            enemy.gameObject.SetActive(true);
        }
        buttons[2].transform.gameObject.SetActive(true);
    }

    public void BeginThirdPhase() // Deactivate all boxes and set the phase 2 complete when the last button is pressed.
    {
        foreach (Transform platform in thirdBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
        }
        buttons[2].transform.gameObject.SetActive(false);
        GameObject.Find("RiotControlDrone").GetComponent<RiotControlDrone>().setPhaseTwoComplete(true);
    }
}

// Activate platforms -> Button press -> Laser Shoots -> Platforms disappear -> Next set appears -> Repeat
