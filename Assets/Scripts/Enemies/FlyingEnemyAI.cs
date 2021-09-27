using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FlyingEnemyAI : MonoBehaviour
{
    private Health _targetHealth;

    public Transform target;
    public Transform enemyGFX;
    private Vector2 spawnPosition;
    private Collider2D _collider;

    public Rigidbody2D playerRB;

    [SerializeField] LayerMask groundLayer;

    public float speed = 200f;
    public float nextWaypointDistance = 3f;
    public float pathUpdateInterval = 1f;

    public string state = "roam";
    public float roamingRange = 2f;
    public float aggroDistance = 5f;
    public float punchingDistance = 3f;
    public float wallCheckDistance = 2f;
    public float knockbackForce = 5f;

    private bool isFacingRight = true;
    //private bool canMove = true;
    //private bool canJump = true;
    private bool returningFromChase = false;
    private bool canPunch = true;
    private float punchCooldown = 1.5f;
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
        _targetHealth = GameObject.Find("Player").GetComponent<Health>();
        _collider = GetComponent<Collider2D>();
        spawnPosition = transform.position;

        Physics2D.IgnoreLayerCollision(3, 7);

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
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //If the target was not found, returns to the start of the update
        if (path == null)
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

        //Calculates the next path point and the amount of force applied
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed;

        //Distance between the enemy and next waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        //Keeps the count of the waypoints
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        //Used for turning the enemy sprite into the direction it is currently going towards to
        if (rb.velocity.x >= 0.1f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            isFacingRight = true;
        }
        else if (rb.velocity.x <= -0.1f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            isFacingRight = false;
        }

        ObstacleCheck();

        EnemyStateChange(force);
    }

    //Does not serve any purpose at the moment.
    private IEnumerator UpdatePath(float waitTime)
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
        yield return new WaitForSeconds(waitTime);
    }

    ////Cooldowns for walk, run, jump and punch.
    //private IEnumerator WalkCoolDown()
    //{
    //    canMove = false;
    //    yield return new WaitForSeconds(walkStepInterval);
    //    canMove = true;
    //}

    //private IEnumerator RunCoolDown()
    //{
    //    canMove = false;
    //    yield return new WaitForSeconds(runStepInterval);
    //    canMove = true;
    //}

    //private IEnumerator JumpCoolDown()
    //{
    //    canJump = false;
    //    yield return new WaitForSeconds(walkStepInterval);
    //    canJump = true;
    //}

    private IEnumerator PunchCoolDown()
    {
        canPunch = false;
        yield return new WaitForSeconds(punchCooldown);
        canPunch = true;
    }

    //private IEnumerator JumpForceForward(float jumpDirection)
    //{
    //    yield return new WaitForSeconds(0.4f);

    //    //Debug.Log("enemyJUMP");
    //    rb.AddForce(new Vector2(100 * jumpDirection, 0));
    //}

    //Trying to raycast and check if there's an obstacle below the enemy unit
    private void ObstacleCheck()
    {
        RaycastHit2D hit;
        Vector2 force;

        //Casts a ray below the flying enemy checking that it doesn't hit the ground
        hit = Physics2D.Raycast(transform.position, new Vector2(0, -1), wallCheckDistance, groundLayer);
        Debug.DrawRay(transform.position, -transform.up * wallCheckDistance, Color.red);
        force = new Vector2(0, 50);

        if (hit.collider != null)
        {
            rb.AddForce(force);


        }
    }

    // Returns true if ground check detects ground
    //private bool IsGrounded()
    //{
    //    return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    //}

    private void EnemyStateChange(Vector2 force)
    {
        //switch-case system between different enemy states.
        switch (state)
        {
            //Roams in a specified area given to the enemy unit and stays inside of it.
            case "roam":
                //Debug.Log("Roaming.");
                speed = 5;

                if(returningFromChase)
                {
                    Vector2 directionToSpawnPoint = (spawnPosition - (Vector2)transform.position).normalized;
                    rb.AddForce(directionToSpawnPoint * speed);
                    if(_collider.bounds.Contains(spawnPosition))
                    {
                        returningFromChase = false;
                    }
                    break;
                }
                //If the enemy unit tries to go outside of the given area parameters, it turns around.
                if (transform.position.x >= (spawnPosition.x + roamingRange))
                {
                    //rb.AddForce(forceX);
                    //StartCoroutine(WalkCoolDown());
                    //Debug.Log("Left");
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                    isFacingRight = false;
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                    //StartCoroutine(WalkCoolDown());
                    
                }
                else if (transform.position.x <= (spawnPosition.x - roamingRange))
                {
                    //rb.AddForce(forceX);
                    //StartCoroutine(WalkCoolDown());
                    //Debug.Log("Right");
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    isFacingRight = true;
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                    //StartCoroutine(WalkCoolDown());
                    
                }
                //If target is close enough the enemy unit, charges it towards the player.
                else if (aggroDistance >= path.GetTotalLength() && (target.transform.position.x >= (spawnPosition.x - roamingRange) && target.transform.position.x < (spawnPosition.x + roamingRange)))
                {
                    state = "charge";
                    break;
                }
                if (transform.position.x <= (spawnPosition.x + roamingRange) && transform.position.x >= (spawnPosition.x - roamingRange))
                {
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                    //StartCoroutine(WalkCoolDown());
                }
                break;

            //Here enemy charges the player. Checks if player is inside enemy unit's roaming range.
            case "charge":

                speed = 10;
                //Debug.Log("Charging!");
                if (path.GetTotalLength() > aggroDistance || (target.transform.position.x <= (spawnPosition.x - roamingRange) || target.transform.position.x > (spawnPosition.x + roamingRange)))
                {
                    returningFromChase = true;
                    state = "roam";
                    break;
                }
                else if (aggroDistance >= path.GetTotalLength() && path.GetTotalLength() > punchingDistance)
                {

                    rb.AddForce(force);
                    //StartCoroutine(RunCoolDown());
                    

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
                if (canPunch)
                {
                    Debug.Log("Player hit");
                    //_targetHealth.TakeDamage(1);
                    //PlayerPushback();
                    StartCoroutine(PunchCoolDown());
                }

                if (target.transform.position.x <= (spawnPosition.x - roamingRange) || target.transform.position.x > (spawnPosition.x + roamingRange))
                {
                    state = "roam";
                    break;
                }
                else if (path.GetTotalLength() > punchingDistance)
                {
                    state = "charge";
                    //Debug.Log("Charge again!");
                    break;
                }

                break;
        }
    }

    void PlayerPushback()
    {
        float pushbackX = target.transform.position.x - transform.position.x;
        Vector2 knockbackDirection = new Vector2((pushbackX), pushbackX / 2).normalized;
        playerRB.AddForce(knockbackDirection * knockbackForce);
    }

}
