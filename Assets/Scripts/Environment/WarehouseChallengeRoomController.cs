using UnityEngine;

public class WarehouseChallengeRoomController : MonoBehaviour
{
    public GameObject[] enemies;
    public ConveyorBelt[] beltsThatChangeDirection;
    public bool activated = false;

    public void StartChallenge()
    {
        if (activated)
            return;
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
                enemy.SetActive(true);
        }
        foreach (ConveyorBelt belt in beltsThatChangeDirection)
        {
            belt.ChangeDirection();
        }
    }

    // For saving and loading
    public bool GetActivated()
    {
        return activated;
    }

    public void SetActivated(bool b)
    {
        activated = b;
    }
}
