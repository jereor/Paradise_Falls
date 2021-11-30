using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseTwoObjectActivator : MonoBehaviour
{
    private bool startSpawningVines = false;
    private bool spawnEscapePlatforms = false;

    [SerializeField] GameObject attackVine;

    [SerializeField] private List<Transform> platforms;

    [SerializeField] private float vineSpeed;
    // Start is called before the first frame update
    void Start()
    {
        platforms = new List<Transform>();
        for(int i = 0; i < gameObject.transform.childCount; i++)
        {
            platforms.Add(gameObject.transform.GetChild(i));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(spawnEscapePlatforms)
        {
            SpawnPlatforms();
            spawnEscapePlatforms = false;
        }

        if(startSpawningVines)
        {
            StartCoroutine(SpawnVines());
            startSpawningVines = false;
        }
    }

    private IEnumerator SpawnVines()
    {
        for (int i = 0; i < Mathf.Infinity; i++)
        {
            Instantiate(attackVine, new Vector2(transform.position.x + 10, (Random.Range(transform.position.y - 10, transform.position.y + 10))), new Quaternion(0, 0, -90, 0));
            Instantiate(attackVine, new Vector2(transform.position.x - 10, (Random.Range(transform.position.y - 10, transform.position.y + 10))), new Quaternion(0, 0, 90, 0));
            yield return new WaitForSeconds(vineSpeed);
        }
    }

    public void StopSpawningVines()
    {
        StopCoroutine(SpawnVines());
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
        spawnEscapePlatforms = b;
    }
}
