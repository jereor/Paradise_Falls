using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class GroundEnemyAI : MonoBehaviour
{
    private Health _targetHealth;

    [Header("Transforms")]
    public Transform target;
    public Transform enemyGFX;
    public Rigidbody2D playerRB;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // GameObject attached to player that checks if touching ground
    [SerializeField] private float checkRadius; // Radius for ground checks
    [SerializeField] LayerMask groundLayer; // Chosen layer that is recognized as ground in ground checks

    [Header("Mobility")]
    public float speed = 200f;
    public float jumpHeight = 500f;    
    public float pathUpdateInterval = 1f;
    public float walkStepInterval = 1f;
    public float runStepInterval = 0.5f;

    [Header("State and Parameters")]
    public string state = "roam";
    public float roamingRange = 2f;
    public float aggroDistance = 5f;
    public float punchingDistance = 3f;
    public float knockbackForce = 5f;

    private float wallCheckDistance = 1.5f;
    private float higherWallCheckDistance = 1.5f;
    private float groundCheckDistance = 2f;
    private bool isFacingRight = true;
    private bool canMove = true;
    private bool canJump = true;
    private bool canPunch = true;
    private float punchCooldown = 1.5f;

    public float nextWaypointDistance = 3f;

    private Vector2 spawnPosition;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;

    private Seeker seeker;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        _targetHealth = GetComponent<Health>();
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
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //If the target was not found, returns to the start of the update
        if(path == null) {return;}

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
        Vector2 forceX = new Vector2(direction.x, 0).normalized * speed;

        //Distance between the enemy and next waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        //Keeps the count of the waypoints
        if(distance < nextWaypointDistance) {currentWaypoint++;}

        //Used for turning the enemy sprite into the direction it is currently going towards to
        if (rb.velocity.x >= 1f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            isFacingRight = true;
        }
        else if (rb.velocity.x <= -1f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            isFacingRight = false;
        }

        ObstacleCheck();

        EnemyStateChange(forceX);
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

    //Cooldowns for walk, run, jump and punch.
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

    private IEnumerator PunchCoolDown()
    {
        canPunch = false;
        yield return new WaitForSeconds(punchCooldown);
        canPunch = true;
    }

    // This function adds force on X-axis, so the enemy unit doesn't get stuck to small obstacles when moving
    private IEnumerator JumpForceForward(float jumpDirection)
    {
        yield return new WaitForSeconds(0.4f);

        //Debug.Log("enemyJUMP");
        rb.AddForce(new Vector2(100 * jumpDirection, 0));
    }

    // Trying to raycast and check if there's an obstacle in front of the enemy. Function also checks if the obstacle is too high to jump over and turns around if impossible to get over.
    // Third ray checks if there's a pit coming ahead so the enemy unit doesn't fall off from the edge.
    private void ObstacleCheck()
    {           
        RaycastHit2D hitHorizontal;
        RaycastHit2D hitAngularUp;
        RaycastHit2D hitAngularDown;
        float jumpDirection;
        Vector2 force;
        Vector2 jumpPosition = transform.position;

        // Casts rays in the direction enemy unit is facing.
        if (isFacingRight)
        {
            hitHorizontal = Physics2D.Raycast(transform.position, transform.right, wallCheckDistance, groundLayer);
            hitAngularUp = Physics2D.Raycast(transform.position, new Vector2(1, 1), higherWallCheckDistance, groundLayer);
            hitAngularDown = Physics2D.Raycast(transform.position, new Vector2(1, -1.5f), groundCheckDistance, groundLayer);
            Debug.DrawRay(transform.position, transform.right * wallCheckDistance, Color.red);
            Debug.DrawRay(transform.position, new Vector2(1,1) * higherWallCheckDistance, Color.red);
            Debug.DrawRay(transform.position, new Vector2(1,-1.5f) * groundCheckDistance, Color.red);
            jumpDirection = 1;
        }
        else
        {
            hitHorizontal = Physics2D.Raycast(transform.position, -transform.right, wallCheckDistance, groundLayer);
            hitAngularUp = Physics2D.Raycast(transform.position, new Vector2(-1, 1), higherWallCheckDistance, groundLayer);
            hitAngularDown = Physics2D.Raycast(transform.position, new Vector2(-1, -1.5f), groundCheckDistance, groundLayer);
            Debug.DrawRay(transform.position, -transform.right * wallCheckDistance, Color.red);
            Debug.DrawRay(transform.position, new Vector2(-1, 1) * higherWallCheckDistance, Color.red);
            Debug.DrawRay(transform.position, new Vector2(-1, -1.5f) * groundCheckDistance, Color.red);
            jumpDirection = -1;
        }


        // If there's no ground ahead, turns around and starts going back.
        if ((hitHorizontal.collider != null && hitAngularUp.collider != null) || hitAngularDown.collider == null)
        {
            if(hitAngularDown.collider == null)
            {
                Debug.Log("No ground ahead!");
            }
            transform.localScale = new Vector3(-jumpDirection, 1f, 1f);
        }
        // If the ray collides with ground layer, unit is grounded and jump is not on cooldown, performs a jump.
        else if (hitHorizontal.collider != null && hitAngularUp.collider == null && IsGrounded() && canJump)
        {
            force = new Vector2(jumpDirection, jumpHeight);
            rb.AddForce(force, ForceMode2D.Impulse);  
            StartCoroutine(JumpForceForward(jumpDirection));
            StartCoroutine(JumpCoolDown());
           
        }
    }

    // Returns true if ground check detects ground
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void EnemyStateChange(Vector2 forceX)
    {
        // switch-case system between different enemy states.
        switch (state)
        {
            // Roams in a specified area given to the enemy unit and stays inside of it.
            case "roam":
                //Debug.Log("Roaming.");
                // If the enemy unit tries to go outside of the given area parameters, it turns around.
                if (transform.position.x >= (spawnPosition.x + roamingRange) && canMove && IsGrounded())
                {
                    //rb.AddForce(forceX);
                    //StartCoroutine(WalkCoolDown());
                    //Debug.Log("Left");
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                    isFacingRight = false;
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                    StartCoroutine(WalkCoolDown());
                    break;
                }
                else if (transform.position.x <= (spawnPosition.x - roamingRange) && canMove && IsGrounded())
                {
                    //rb.AddForce(forceX);
                    //StartCoroutine(WalkCoolDown());
                    //Debug.Log("Right");
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    isFacingRight = true;
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                    StartCoroutine(WalkCoolDown());
                    break;
                }
                // If target is close enough the enemy unit, charges it towards the player.
                else if (aggroDistance >= path.GetTotalLength() && (target.transform.position.x >= (spawnPosition.x - roamingRange) && target.transform.position.x < (spawnPosition.x + roamingRange)))
                {
                    state = "charge";
                    break;
                }
                // If the enemy unit is inside the given roaming range and target is nowhere near, it roams around.
                if(transform.position.x <= (spawnPosition.x + roamingRange) && transform.position.x >= (spawnPosition.x - roamingRange) && canMove && IsGrounded())
                {
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                    StartCoroutine(WalkCoolDown());
                }
                break;

            //Here enemy charges the target. Checks if target is inside enemy unit's roaming range.
            case "charge":
                //Debug.Log("Charging!");
                // Outside the range, return to roaming state.
                if (path.GetTotalLength() > aggroDistance || (target.transform.position.x <= (spawnPosition.x - roamingRange) || target.transform.position.x > (spawnPosition.x + roamingRange)))
                {
                    state = "roam";
                    break;
                }
                // Inside the range, runs towards the target.
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
                if (canPunch)
                {
                    //Do damage to player here
                    Debug.Log("Player hit");

                    // Turns the enemy unit torwards the target when punching.
                    if(target.transform.position.x - transform.position.x >= 0)
                    {
                        transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                    else
                    {
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                    }
                    //_targetHealth.TakeDamage(1);
                    PlayerPushback();
                    StartCoroutine(PunchCoolDown());
                }

                //If target goes out of enemy's bounds, return to "roam" state
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

    // Small knockback to the target when too close to the enemy unit. Knockback knocks slightly upwards so the friction doesn't stop the target right away.
    void PlayerPushback()
    {
        float pushbackX = target.transform.position.x - transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Math.Abs(pushbackX / 4)).normalized;
        playerRB.AddForce(knockbackDirection * knockbackForce);
    }

}

