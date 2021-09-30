using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class FlyingEnemyAI : MonoBehaviour
{
    private Health _targetHealth;

    [Header("Transforms")]
    public Transform target;
    public Transform enemyGFX;
    public Rigidbody2D playerRB;
    public GameObject bullet;
    public Collider2D areaCollider;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;

    [Header("Player Check")]
    [SerializeField] LayerMask playerLayer;

    [Header("Mobility")]
    public float speed = 5f;
    public float chargeSpeed = 10f;
    public float roamSpeed = 5f;
    public float nextWaypointDistance = 3f;
    public float pathUpdateInterval = 1f;

    [Header("State and Parameters")]
    public string state = "roam";
    public float roamingRangeX = 10f;
    public float roamingRangeY = 10f;
    public float aggroDistance = 5f;
    public float shootingDistance = 10f;
    public float wallCheckDistance = 2f;
    public float knockbackForce = 5f;

    private bool returningFromChase = false;
    private bool canShoot = true;
    private float shootCooldown = 1.5f;
    private bool isFacingRight = true;
    private float vectorPathLength = 1;

    private Collider2D _collider;
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(spawnPosition, new Vector2(roamingRangeX, roamingRangeY));
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
        if (path == null) {return;}

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
        // Current path length is saved for enemy behaviour purposes.
        vectorPathLength = path.GetTotalLength();

        //Calculates the next path point and the amount of force applied
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed;

        //Distance between the enemy and next waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        //Keeps the count of the waypoints
        if (distance < nextWaypointDistance) {currentWaypoint++;}

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
        EnemyStateChange(force, vectorPathLength);
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

    private IEnumerator ShootCoolDown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    //Trying to raycast and check if there's an obstacle below the enemy unit
    private void ObstacleCheck()
    {
        RaycastHit2D hit;
        Vector2 force;

        //Casts a ray below the flying enemy checking that it doesn't hit the ground
        hit = Physics2D.Raycast(transform.position, new Vector2(0, -1), wallCheckDistance, groundLayer);
        Debug.DrawRay(transform.position, -transform.up * wallCheckDistance, Color.red);
        force = new Vector2(0, 20);

        if (hit.collider != null)
        {
            rb.AddForce(force);
        }
    }

    private bool IsPlayerInRange()
    {
        return Physics2D.OverlapBox(spawnPosition, new Vector2(roamingRangeX,roamingRangeY), 0, playerLayer);
    }

    // ENEMY BEHAVIOUR STATES
    // --------------------------------------------------------------------------------------------------------------------------

    private void EnemyStateChange(Vector2 force, float pathLength)
    {
        //switch-case system between different enemy states.
        switch (state)
        {
            // ROAM STATE
            //-------------------------------------------------------------------------------------------------------
            //Roams in a specified area given to the enemy unit and stays inside of it.
            case "roam":

                // Checks if enemy unit has given up a chase and is returning to spawn point. If target comes too close to the enemy, it begins to chase again.
                if(returningFromChase && (aggroDistance < pathLength || !IsPlayerInRange()))
                {
                    Vector2 directionToSpawnPoint = (spawnPosition - (Vector2)transform.position).normalized;
                    rb.AddForce(directionToSpawnPoint * speed);
                    if(_collider.bounds.Contains(spawnPosition))
                    {
                        returningFromChase = false;
                        rb.velocity = new Vector2(0,0);
                    }
                    break;
                }
                //If the enemy unit tries to go outside of the given area parameters in X-axis, it turns around.
                if (transform.position.x >= (spawnPosition.x + roamingRangeX/2))
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                    isFacingRight = false;
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                }
                else if (transform.position.x <= (spawnPosition.x - roamingRangeX/2))
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    isFacingRight = true;
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                }
                //If target is close enough the enemy unit, charges it towards the player.
                //else if (aggroDistance >= pathLength && CheckIfTargetInsideXAxisNegative() && CheckIfTargetInsideXAxisPositive() &&
                //    CheckIfTargetInsideYAxisNegative() && CheckIfTargetInsideYAxisPositive())
                //{
                //    state = "charge";
                //    speed = chargeSpeed;
                //    break;
                //}
                
                else if (aggroDistance >= pathLength && IsPlayerInRange())
                {
                    Debug.Log("We here bois");
                    state = "charge";
                    speed = chargeSpeed;
                    break;
                }

                // WallCheck! If enemy unit is about to hit a wall in roaming state, it turns around and continues to the opposite direction.
                if (transform.position.x <= (spawnPosition.x + roamingRangeX) && transform.position.x >= (spawnPosition.x - roamingRangeX))
                {
                    RaycastHit2D hit;
                    if(isFacingRight)
                    {
                        hit = Physics2D.Raycast(transform.position, Vector2.right, 3, groundLayer);
                        Debug.DrawRay(transform.position, Vector2.right, Color.red);
                        if(hit.collider != null)
                        {
                            transform.localScale = new Vector3(-1, 1f, 1f);
                        }
                    }
                    else
                    {
                        hit = Physics2D.Raycast(transform.position, Vector2.left, 3, groundLayer);
                        Debug.DrawRay(transform.position, Vector2.left, Color.red);
                        if (hit.collider != null)
                        {
                            transform.localScale = new Vector3(1, 1f, 1f);
                        }
                    }

                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                }
                break;


            // CHARGE STATE
            //------------------------------------------------------------------------------------------------------------------
            //Here enemy charges the player. Checks if player is inside enemy unit's roaming range.
            case "charge":
                // Target is out of aggro range, return to roaming state.
                if (pathLength > aggroDistance || !IsPlayerInRange())
                {
                    returningFromChase = true;
                    state = "roam";
                    speed = roamSpeed;
                    break;
                }
                else if (aggroDistance >= pathLength && pathLength > shootingDistance)
                {
                    rb.AddForce(force);
                }
                //If target is close enough the enemy unit, it changes the state to "shoot"
                else if (pathLength < shootingDistance)
                {
                    state = "shoot";
                }
                break;


            // SHOOT STATE
            //-------------------------------------------------------------------------------------------------------------------
            //Does damage to target if close enough. Otherwise goes to roam or charge state.
            case "shoot":
                // Bullets do the damage, this only checks if the bullet can hit the target from current angle.
                Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.blue);
                if (canShoot)
                {
                    RaycastHit2D hitGround;
                    RaycastHit2D hitPlayer;
                    hitGround = Physics2D.Raycast(transform.position, (target.transform.position - transform.position), (target.transform.position - transform.position).magnitude, groundLayer);
                    hitPlayer = Physics2D.Raycast(transform.position, (target.transform.position - transform.position), (target.transform.position - transform.position).magnitude, playerLayer);
                    if (hitPlayer && !hitGround)
                    {
                        // Debug.Log("PEW!");
                        Instantiate(bullet, transform.position, Quaternion.identity);                       
                    }                 

                    // Turns the enemy unit torwards the target when shooting.
                    if (target.transform.position.x - transform.position.x >= 0)
                    {
                        transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                    else
                    {
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                    }
                    StartCoroutine(ShootCoolDown());
                }
                // If target goes out of enemy's bounds, return to "roam" state
                if (!IsPlayerInRange())
                {
                    returningFromChase = true;
                    state = "roam";
                    speed = roamSpeed;
                    break;
                }
                else if (pathLength > shootingDistance && IsPlayerInRange())
                {
                    state = "charge";
                    speed = chargeSpeed;
                    break;
                }
                break;
        }
    }

    void PlayerPushback()
    {
        float pushbackX = target.transform.position.x - transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Math.Abs(pushbackX / 4)).normalized;
        playerRB.AddForce(knockbackDirection * knockbackForce);
    }

    // Axis checks if target is outside or inside the given roaming parameters
    private bool CheckIfTargetInsideYAxisPositive()
    {
        return target.transform.position.y < (spawnPosition.y + roamingRangeY);
    }
    private bool CheckIfTargetInsideYAxisNegative()
    {
        return target.transform.position.y >= (spawnPosition.y - roamingRangeY);
    }

    private bool CheckIfTargetInsideXAxisPositive()
    {
        return target.transform.position.x < (spawnPosition.x + roamingRangeX);
    }

    private bool CheckIfTargetInsideXAxisNegative()
    {
        return (target.transform.position.x >= (spawnPosition.x - roamingRangeX));
    }

    private bool CheckIfTargetOutsideXAxisPositive()
    {
        return target.transform.position.x > (spawnPosition.x + roamingRangeX);
    }

    private bool CheckIfTargetOutsideXAxisNegative()
    {
        return target.transform.position.x <= (spawnPosition.x - roamingRangeX);
    }

    private bool CheckIfTargetOutsideYAxisPositive()
    {
        return target.transform.position.y > (spawnPosition.y + roamingRangeY);
    }

    private bool CheckIfTargetOutsideYAxisNegative()
    {
        return target.transform.position.y <= (spawnPosition.y - roamingRangeY);
    }

}
