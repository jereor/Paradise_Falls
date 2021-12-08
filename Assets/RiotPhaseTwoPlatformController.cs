using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class RiotPhaseTwoPlatformController : MonoBehaviour
{
    [SerializeField] private List<Transform> firstBoxes;
    [SerializeField] private List<Transform> secondBoxes;
    [SerializeField] private List<Transform> thirdBoxes;

    [SerializeField] private List<Transform> buttons;
    private Transform boxSetOne;
    private Transform boxSetTwo;
    private Transform boxSetThree;

    private Transform buttonSet;

    [SerializeField] Transform spawnerLeft;
    [SerializeField] Transform spawnerRight;
    [SerializeField] private GameObject workerDrone;
    [SerializeField] private GameObject flyingDrone;
    public List<GameObject> droneInstances;

    [SerializeField] private GameObject entranceDoor;
    [SerializeField] private GameObject secondMiddleDoor;
    [SerializeField] private GameObject exitDoor;


    // Start is called before the first frame update
    void Start()
    {
        droneInstances = new List<GameObject>();
        // Get all child gameobjects for activation.
        boxSetOne = gameObject.transform.GetChild(0).transform;
        boxSetTwo = gameObject.transform.GetChild(1).transform;
        boxSetThree = gameObject.transform.GetChild(2).transform;

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


        foreach(Transform button in buttonSet)
        {
            buttons.Add(button);
        }

    }

    public void ActivateFirstSet() // Activate first set of boxes and enemies.
    {
        foreach(Transform platform in firstBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
            var guo = new GraphUpdateObject(platform.GetComponent<BoxCollider2D>().bounds);
            // Set some settings
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo, 1);
        }


        droneInstances.Add(Instantiate(workerDrone, spawnerRight.position, Quaternion.identity));
        droneInstances[0].GetComponent<GroundEnemyAI>().bossMode = true;
        droneInstances.Add(Instantiate(flyingDrone, spawnerLeft.position, Quaternion.identity));
        droneInstances[1].GetComponent<FlyingEnemyAI>().bossMode = true;

        buttons[0].transform.gameObject.SetActive(true);
    }

    public void ActivateSecondSet() // Activate second set of boxes and enemies.
    {
        buttons[0].transform.gameObject.SetActive(false);
        foreach (Transform platform in firstBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
            var guo = new GraphUpdateObject(platform.GetComponent<BoxCollider2D>().bounds);
            // Set some settings
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo, 1);
        }
        foreach (Transform platform in secondBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
            var guo = new GraphUpdateObject(platform.GetComponent<BoxCollider2D>().bounds);
            // Set some settings
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo, 1);
        }

        droneInstances.Add(Instantiate(workerDrone, spawnerRight.position, Quaternion.identity));
        droneInstances[2].GetComponent<GroundEnemyAI>().bossMode = true;
        droneInstances.Add(Instantiate(workerDrone, spawnerLeft.position, Quaternion.identity));
        droneInstances[3].GetComponent<GroundEnemyAI>().bossMode = true;
        buttons[1].transform.gameObject.SetActive(true);
    }

    public void ActivateThirdSet() // Activate third set of boxes and enemies.
    {
        buttons[1].transform.gameObject.SetActive(false);
        foreach (Transform platform in secondBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
            var guo = new GraphUpdateObject(platform.GetComponent<BoxCollider2D>().bounds);
            // Set some settings
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo, 1);
        }
        foreach (Transform platform in thirdBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
            var guo = new GraphUpdateObject(platform.GetComponent<BoxCollider2D>().bounds);
            // Set some settings
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo, 1);
        }
        droneInstances.Add(Instantiate(workerDrone, spawnerRight.position, Quaternion.identity));
        droneInstances[4].GetComponent<GroundEnemyAI>().bossMode = true;
        droneInstances.Add(Instantiate(workerDrone, spawnerLeft.position, Quaternion.identity));
        droneInstances[5].GetComponent<GroundEnemyAI>().bossMode = true;
        buttons[2].transform.gameObject.SetActive(true);
    }

    public void BeginThirdPhase() // Deactivate all boxes and set the phase 2 complete when the last button is pressed.
    {
        foreach (Transform platform in thirdBoxes)
        {
            platform.GetComponent<DisappearingPlatform>().RiotRoomWork();
            var guo = new GraphUpdateObject(platform.GetComponent<BoxCollider2D>().bounds);
            // Set some settings
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo, 1);
        }
        buttons[2].transform.gameObject.SetActive(false);
        GameObject.Find("RiotControlDrone").GetComponent<RiotControlDrone>().setPhaseTwoComplete(true);
    }

    public void OpenDoors()
    {
        entranceDoor.GetComponent<DoorController>().Work();
        secondMiddleDoor.GetComponent<DoorController>().Work();
        exitDoor.GetComponent<DoorController>().Work();
    }

}

// Activate platforms -> Button press -> Laser Shoots -> Platforms disappear -> Next set appears -> Repeat
