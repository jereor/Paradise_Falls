using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS SCRIPT ACTIVATES THE PLATFORMS AND THE VINES ATTACKING THE PLAYER DURING THE ESCAPE SEQUENCE.
public class PhaseTwoObjectActivator : MonoBehaviour
{
    // Bools to determine if the platforms or vines should be spawned.
    private bool startSpawningVines = false;
    private bool spawnAllEscapeObjects = false;

    private GameObject target;
    Quaternion leftSideRotation;
    Quaternion rightSideRotation;
    [SerializeField] GameObject attackVine;

    [SerializeField] private GameObject vineActivator;
    [SerializeField] private GameObject vineDisactivator;

    [SerializeField] private List<Transform> platforms; // List of all the spawnable platforms.
    [SerializeField] private GameObject additionalAppearingGround;

    [SerializeField] private float vineSpawnSpeed; // How frequently the vines will spawn.

    void Start()
    {
        target = GameObject.Find("Player");
        platforms = new List<Transform>();

        // Finds all the platforms that are this game objects' children.
        for(int i = 0; i < gameObject.transform.childCount; i++)
        {
            platforms.Add(gameObject.transform.GetChild(i));
        }

        // Sets the rotation for both sides' vines.
        rightSideRotation.eulerAngles = new Vector3(0,0, 90);
        leftSideRotation.eulerAngles = new Vector3(0, 0, -90);
    }

    // Update is called once per frame
    void Update()
    {
        if(startSpawningVines)
        {
            StartCoroutine(SpawnVines());
            startSpawningVines = false;
        }
    }

    // Continues infinitely until the coroutine is stopped. This will happen when player reaches certain point in the escape sequence.
    private IEnumerator SpawnVines()
    {
        for (int i = 0; i < Mathf.Infinity; i++)
        {
            // Spawns vines left and right of the player in given intervals.
            Instantiate(attackVine, new Vector2(transform.position.x + 12, (Random.Range(target.transform.position.y - 5, target.transform.position.y + 5))), rightSideRotation);
            yield return new WaitForSeconds(vineSpawnSpeed);
            Instantiate(attackVine, new Vector2(transform.position.x - 12, (Random.Range(target.transform.position.y - 5, target.transform.position.y + 5))), leftSideRotation);
            yield return new WaitForSeconds(vineSpawnSpeed);
        }
    }

    public void StopSpawningVines()
    {
        //StopCoroutine(SpawnVines());
        StopAllCoroutines();
    }

    public void SpawnAllEscapeObjects()
    {
        SpawnPlatforms();
        additionalAppearingGround.SetActive(true);
        vineActivator.SetActive(true);
        vineDisactivator.SetActive(true);
    }

    private void SpawnPlatforms()
    {
        foreach(Transform platform in platforms)
        {
            platform.gameObject.SetActive(true);
        }
    }

    public void SetStartSpawningVines(bool b)
    {
        startSpawningVines = b;
    }

    public void SetSpawnPlatforms(bool b)
    {
        spawnAllEscapeObjects = b;
    }
}
