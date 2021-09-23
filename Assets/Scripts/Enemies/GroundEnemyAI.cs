using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;



public class GroundEnemyAI : MonoBehaviour
{
    public Transform target;
    public Transform enemyGFX;
    private Vector2 spawnPosition;

    public float speed = 200f;
    public float nextWaypointDistance = 3f;
    public float pathUpdateInterval = 1f;
    public float stepSpeed = 1f;
    public string state = "roam";
    public float roamingRange = 2f;
    public float aggroDistance = 5f;
    public float punchingDistance = 3f;

    private bool canMove = true;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;

        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }
    void UpdatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(spawnPosition);
        //Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        //Vector2 forceX = new Vector2(direction.x * speed, 0);
        //if (canMove)
        //{
        //    rb.AddForce(forceX);
        //    StartCoroutine(MoveCoolDown());
        //}

        //Returns if player is too far from the enemy
        //if (maxDistanceToTarget <= path.GetTotalLength() || path == null)
        //{
        //    return;
        //}

        if(path == null)
        {
            return;
        }

        //StartCoroutine(UpdatePath(pathUpdateInterval));

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 forceX = new Vector2(direction.x * speed, 0);

        //rb.AddForce(forceX);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if(distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (rb.velocity.x >= 0.01f)
        {
            transform.localScale = new Vector2(-1f, 1f);
        }
        else if (rb.velocity.x <= -0.01f)
        {
            transform.localScale = new Vector2(1f, 1f);
        }

        //if (canMove)
        //{
        //    rb.AddForce(forceX);
        //    StartCoroutine(MoveCoolDown());
        //}

        switch (state)
        {
            case "roam":
                Debug.Log("Roaming.");
                if(transform.position.x >= (spawnPosition.x + roamingRange) && canMove)
                {
                    rb.AddForce(new Vector2(-1 * speed, 0));
                    StartCoroutine(MoveCoolDown());
                    break;
                }
                else if(transform.position.x <= (spawnPosition.x - roamingRange) && canMove)
                {
                    rb.AddForce(new Vector2(1 * speed, 0));
                    StartCoroutine(MoveCoolDown());
                    break;
                }
                if(aggroDistance >= path.GetTotalLength())
                {
                    state = "charge";
                }
                break;

            case "charge":
                Debug.Log("Charging!");
                if(target.transform.position.x < -roamingRange || target.transform.position.x > roamingRange)
                {
                    state = "roam";
                }
                if (aggroDistance >= path.GetTotalLength() && path.GetTotalLength() > punchingDistance && canMove)
                {

                    rb.AddForce(forceX);
                    StartCoroutine(MoveCoolDown());

                }
                else if (path.GetTotalLength() < punchingDistance)
                {
                    state = "punch";
                }



                break;

            case "punch":
                //Do damage to player here
                Debug.Log("PUNCH!");
                if (path.GetTotalLength() > punchingDistance)
                {
                    state = "charge";
                    Debug.Log("Charge again!");
                }

                break;
        }
    }

    private IEnumerator UpdatePath(float waitTime)
    {
        if(seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
        yield return new WaitForSeconds(waitTime);
    }

    private IEnumerator MoveCoolDown()
    {
        canMove = false;
        yield return new WaitForSeconds(stepSpeed);
        canMove = true;
    }
}
