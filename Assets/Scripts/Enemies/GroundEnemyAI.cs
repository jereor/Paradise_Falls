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
    public float walkStepInterval = 1f;
    public float runStepInterval = 0.5f;
    public string state = "roam";
    public float roamingRange = 2f;
    public float aggroDistance = 5f;
    public float punchingDistance = 3f;

    private bool canMove = true;
    private bool canJump = true;
    private int layerMask = 1 << 6;

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

        //Updates the path repeatedly with a chosen time interval
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

        //If the target was not found, returns to the start of the update
        if(path == null)
        {
            return;
        }

        //Checks if the enemy is in the end of the path
        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        //Calculates the next path point and the amount of force applied on X-axis
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 forceX = new Vector2(direction.x * speed, 0);

        //Distance between the enemy and next waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        //Keeps the count of the waypoints
        if(distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        //Used for turning the enemy sprite into the direction it is currently going towards to
        if (forceX.x >= 0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (forceX.x <= -0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        //if (canMove)
        //{
        //    rb.AddForce(forceX);
        //    StartCoroutine(MoveCoolDown());
        //}


        //CURRENTLY NOT WORKING. Trying to raycast and check if there's an obstacle in front of the enemy unit and performing a jump over it.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, 1, layerMask);
        Debug.DrawRay(transform.position, transform.right, Color.red);
        if (hit.collider != null && canJump)
        {
            rb.AddForce(Vector3.up * 500);
            StartCoroutine(JumpCoolDown());
        }

        //switch-case system between different enemy states.
        switch (state)
        {
            //Roams in a specified area given to the enemy unit and stays inside of it.
            case "roam":
                Debug.Log("Roaming.");
                //If the enemy unit tries to go outside of the given area parameters, it turns around.
                if(transform.position.x >= (spawnPosition.x + roamingRange) && canMove)
                {
                    rb.AddForce(new Vector2(-1 * speed, 0));
                    StartCoroutine(WalkCoolDown());
                    break;
                }
                else if(transform.position.x <= (spawnPosition.x - roamingRange) && canMove)
                {
                    rb.AddForce(new Vector2(1 * speed, 0));
                    StartCoroutine(WalkCoolDown());
                    break;
                }
                //If target is close enough the enemy unit, charges it towards the player.
                else if(aggroDistance >= path.GetTotalLength())
                {
                    state = "charge";
                    break;
                }
                break;

            //Here enemy charges the player. Checks if player is inside enemy unit's roaming range.
            case "charge":
                Debug.Log("Charging!");
                if(target.transform.position.x <= (spawnPosition.x - roamingRange) || target.transform.position.x > (spawnPosition.x + roamingRange))
                {
                    state = "roam";
                    break;
                }
                else if (aggroDistance >= path.GetTotalLength() && path.GetTotalLength() > punchingDistance && canMove)
                {

                    rb.AddForce(forceX);
                    StartCoroutine(RunCoolDown());
                    break;

                }
                //If target is close enough the enemy unit, it changes the state to "punch"
                else if (path.GetTotalLength() < punchingDistance)
                {
                    state = "punch";
                }
                break;

            //Does damage to target if close enough. Otherwise goes to roam or charge state.
            case "punch":
                //Do damage to player here
                Debug.Log("PUNCH!");
                if (target.transform.position.x <= (spawnPosition.x - roamingRange) || target.transform.position.x > (spawnPosition.x + roamingRange))
                {
                    state = "roam";
                    break;
                }
                else if (path.GetTotalLength() > punchingDistance)
                {
                    state = "charge";
                    Debug.Log("Charge again!");
                    break;
                }

                break;
        }
    }

    //Does not serve any purpose at the moment.
    private IEnumerator UpdatePath(float waitTime)
    {
        if(seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
        yield return new WaitForSeconds(waitTime);
    }

    //Cooldowns for walk, run and jump.
    private IEnumerator WalkCoolDown()
    {
        canMove = false;
        yield return new WaitForSeconds(walkStepInterval);
        canMove = true;
    }

    private IEnumerator RunCoolDown()
    {
        canMove = false;
        yield return new WaitForSeconds(runStepInterval);
        canMove = true;
    }

    private IEnumerator JumpCoolDown()
    {
        canJump = false;
        yield return new WaitForSeconds(walkStepInterval);
        canJump = true;
    }

}
