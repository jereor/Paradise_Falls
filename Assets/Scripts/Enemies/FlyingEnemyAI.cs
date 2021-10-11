using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class FlyingEnemyAI : MonoBehaviour
{
    private Health _targetHealth;
    private Energy _targetEnergy;
    private Shield targetShield;

    [Header("Transforms")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform enemyGFX;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject energyItem;
    [SerializeField] private GameObject healthItem;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;

    [Header("Player Check")]
    [SerializeField] LayerMask playerLayer;

    [Header("Mobility")]
    [SerializeField] private float speed = 250f;
    [SerializeField] private float chargeSpeed = 400f;
    [SerializeField] private float roamSpeed = 250f;
    [SerializeField] private float attackPower = 1f;
    [SerializeField] private float explosionPower = 2f;


    [Header("State and Parameters")]
    [SerializeField] private string state = "roam";
    [SerializeField] private Vector2 roamingRange = new Vector2(10, 10);
    [SerializeField] private float aggroDistance = 5f;
    [SerializeField] private float shootingDistance = 10f;
    [SerializeField] private float wallCheckDistance = 2f;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Health and Energy Spawn values")]
    [SerializeField] private float healthProbability; // Value between 1-100. Higher the better chance.
    [SerializeField] private float energyProbability;
    [SerializeField] private float amountWhenHealthIsSpawnable; // MaxHealth value between 0-1. When your health sinks below a certain amount health becomes spawnable.

    [Header("Pathfinding info")]
    [SerializeField] private float nextWaypointDistance = 1f;
    [SerializeField] private float pathUpdateInterval = 1f;

    private bool returningFromChase = false;
    private bool canShoot = true;
    private float shootCooldown = 1.5f;
    private bool isFacingRight = true;
    //private float vectorPathLength = 1;
    private bool isTargetInBehaviourRange = false;
    private Vector2 lastSeenTargetPosition;

    private Collider2D _collider;
    private Vector2 spawnPosition;
    private Path path;
    private bool gizmoPositionChange = true;
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
        targetShield = GameObject.Find("Player").GetComponent<Shield>();
        _collider = GetComponent<Collider2D>();
        spawnPosition = transform.position;
        gizmoPositionChange = false;
        Physics2D.IgnoreLayerCollision(3, 7);
        //Updates the path repeatedly with a chosen time interval
        InvokeRepeating("UpdatePathToPlayer", 0f, pathUpdateInterval);
        
    }
    void UpdatePathToPlayer()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    void UpdatePathReturn()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, spawnPosition, OnPathComplete);
    }

    void UpdatePathToLastSeenTargetLocation()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, lastSeenTargetPosition, OnPathComplete);
    }

    //Draws gizmos for enemy's "territory".
    private void OnDrawGizmosSelected()
    {
        if(gizmoPositionChange)
        {
            Gizmos.DrawWireCube(transform.position, roamingRange);
        }
        else
        {
            Gizmos.DrawWireCube(spawnPosition, roamingRange);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingDistance);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {

            path = p;
            currentWaypoint = 0;
        }
        else 
            Debug.Log("Error");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //If the target was not found, returns to the start of the update
        if (path == null) { return;}

        // If the target is too far from the enemy unit, it respawns in to the spawn point and stays there until target is close enough again.
        // Enemy stops all actions for the time being.
        if ((target.transform.position - transform.position).magnitude > 60 && !IsPlayerInRange())
        {
            transform.position = spawnPosition;
            isTargetInBehaviourRange = false;
        }
        else
        {
            isTargetInBehaviourRange = true;
        }

        if(isTargetInBehaviourRange)
        {
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
            Vector2 force = direction * speed * Time.deltaTime;

            //Distance between the enemy and next waypoint
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            //Keeps the count of the waypoints
            if (distance < nextWaypointDistance) { currentWaypoint++; }

            //Used for turning the enemy sprite into the direction it is currently going towards to

            ObstacleCheck();
            EnemyStateChange(force);
        }
    }

    //Does not serve any purpose at the moment.
    private IEnumerator UpdatePath(float waitTime)
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
            Debug.Log("Path updated.");
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

    // Checks if target is in a box shaped area given by parameters.
    private bool IsPlayerInRange()
    {
        return Physics2D.OverlapBox(spawnPosition, roamingRange, 0, playerLayer);
    }

    private bool IsPlayerInAggroRange()
    {
        return Physics2D.OverlapCircle(transform.position, aggroDistance, playerLayer);
    }

    private bool IsPlayerInShootingRange()
    {
        return Physics2D.OverlapCircle(transform.position, shootingDistance, playerLayer);
    }

    private bool IsHittingGround()
    {
        return Physics2D.CircleCast(transform.position, 4, transform.position, 4, groundLayer);
    }

    // ENEMY BEHAVIOUR STATES
    // --------------------------------------------------------------------------------------------------------------------------

    private void EnemyStateChange(Vector2 force)
    {
        //switch-case system between different enemy states.
        switch (state)
        {
            // ROAM STATE
            //-------------------------------------------------------------------------------------------------------
            //Roams in a specified area given to the enemy unit and stays inside of it.
            case "roam":
                // Checks if enemy unit has given up a chase and is returning to spawn point. If target comes too close to the enemy, it begins to chase again.
                if(returningFromChase && (!IsPlayerInAggroRange() || !IsPlayerInRange()))
                {
                    FlipLocalScaleWithForce(force);
                    rb.AddForce(force);
                    if (_collider.bounds.Contains(spawnPosition))
                    {
                        returningFromChase = false;
                        rb.velocity = new Vector2(0,0);
                        InvokeRepeating("UpdatePathToPlayer", 0f, pathUpdateInterval);
                    }
                    break;
                }
                //If the enemy unit tries to go outside of the given area parameters in X-axis, it turns around.
                if (transform.position.x >= (spawnPosition.x + roamingRange.x/2))
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                    isFacingRight = false;
                    rb.AddForce(new Vector2(transform.localScale.x * speed * Time.deltaTime, 0));
                }
                else if (transform.position.x <= (spawnPosition.x - roamingRange.x/2))
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    isFacingRight = true;
                    rb.AddForce(new Vector2(transform.localScale.x * speed * Time.deltaTime, 0));
                }

                //If target is close enough the enemy unit, charges it towards the player.              
                else if (IsPlayerInAggroRange() && IsPlayerInRange())
                {
                    CancelInvoke();
                    InvokeRepeating("UpdatePathToPlayer", 0f, pathUpdateInterval);
                    state = "charge";
                    speed = chargeSpeed;
                    break;
                }

                // WallCheck! If enemy unit is about to hit a wall in roaming state, it turns around and continues to the opposite direction.
                if (transform.position.x <= (spawnPosition.x + roamingRange.x) && transform.position.x >= (spawnPosition.x - roamingRange.x))
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

                    rb.AddForce(new Vector2(transform.localScale.x * speed * Time.deltaTime, 0));
                }
                break;


            // CHARGE STATE
            //------------------------------------------------------------------------------------------------------------------
            //Here enemy charges the player. Checks if player is inside enemy unit's roaming range.
            case "charge":
                // Target is out of aggro range, return to roaming state.
                //if (!IsPlayerInAggroRange() || !IsPlayerInRange())
                //{
                //    returningFromChase = true;
                //    CancelInvoke();
                //    InvokeRepeating("UpdatePathReturn", 0f, pathUpdateInterval);
                //    state = "roam";
                //    speed = roamSpeed;
                //    break;
                //}

                FlipLocalScaleWithForce(force);
                if (IsPlayerInAggroRange() && !IsPlayerInShootingRange())
                {
                    rb.AddForce(force);
                }

                //If target is close enough the enemy unit, it changes the state to "shoot"
                else if (IsPlayerInShootingRange())
                {
                    if (targetShield.Blocking)
                    {
                        state = "ram";
                    }
                    else
                    {
                        state = "shoot";
                    }
                    
                }

                if(!IsPlayerInAggroRange())
                {
                    lastSeenTargetPosition = target.transform.position;
                    CancelInvoke();
                    InvokeRepeating("UpdatePathToLastSeenTargetLocation", 0f, pathUpdateInterval);
                    state = "alert";
                }
                break;


            // SHOOT STATE
            //-------------------------------------------------------------------------------------------------------------------
            //Does damage to target if close enough. Otherwise goes to roam or charge state.
            case "shoot":
                // Bullets do the damage, this only checks if the bullet can hit the target from current angle.
                Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.blue);
                if(targetShield.Blocking)
                {
                    state = "ram";
                }
                if (canShoot)
                {
                    // Draws two rays in the direction of the target. First checks if there's ground in between the enemy unit and the target, second checks if it hit the target.
                    RaycastHit2D hitGround;
                    RaycastHit2D hitPlayer;
                    hitGround = Physics2D.Raycast(transform.position, (target.transform.position - transform.position), (target.transform.position - transform.position).magnitude, groundLayer);
                    hitPlayer = Physics2D.Raycast(transform.position, (target.transform.position - transform.position), (target.transform.position - transform.position).magnitude, playerLayer);
                    if (hitPlayer && !hitGround)
                    {
                        // Instantiate a bullet prefab from enemy unit location.
                        GameObject bulletObject = Instantiate(bullet, transform.position, Quaternion.identity);
                        bulletObject.GetComponent<BulletBehaviour>().shooter = this.gameObject;
                        StartCoroutine(ShootCoolDown());
                    }
                    // If the target is in shooting range but there's a ground in between, enemy unit tries to find a path past it.
                    else if(hitPlayer && hitGround)
                    {
                        //Debug.Log("Moving closer.");
                        rb.AddForce(force);
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
                }
                // If target goes out of enemy's bounds, return to "roam" state
                if (!IsPlayerInRange())
                {
                    returningFromChase = true;
                    CancelInvoke();
                    InvokeRepeating("UpdatePathReturn", 0f, pathUpdateInterval);
                    state = "roam";
                    speed = roamSpeed;
                    break;
                }
                else if (!IsPlayerInShootingRange() && IsPlayerInAggroRange())
                {
                    state = "charge";
                    speed = chargeSpeed;
                    break;
                }
                break;

            // RAM STATE
            //------------------------------------------------------------------------------------------------------------------------
            // Enemy unit rams into player if they're holding shield up, since shooting would only kill the enemy certainly.
            case "ram":
                // Make player and enemy colliders hit each other
                if(Physics2D.GetIgnoreLayerCollision(3, 7) == true)
                {
                    Physics2D.IgnoreLayerCollision(3, 7, false);
                }

                FlipLocalScaleWithForce(force);
                // Check if player is still holding their shield up.
                if (!targetShield.Blocking)
                {
                    state = "shoot";
                    canShoot = true;
                    Physics2D.IgnoreLayerCollision(3, 7);
                    gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
                    break;
                }
                // Ramming into player!
                else
                {
                    gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                    rb.AddForce(force);
                }
                break;

            // ALERT STATE
            //-------------------------------------------------------------------------------------------------------------------------
            // If player escapes the range, enemy unit enters in a alert state where it will follow player to it's last seen position.
            case "alert":
                // Player is in shooting range. Shoot!
                if (IsPlayerInShootingRange() && IsPlayerInRange())
                {
                    CancelInvoke();
                    InvokeRepeating("UpdatePathToPlayer", 0f, pathUpdateInterval);
                    state = "shoot";
                    break;
                }
                // Player is in Aggro range. Charge towards the player.
                else if (IsPlayerInAggroRange() && IsPlayerInRange())
                {
                    CancelInvoke();
                    InvokeRepeating("UpdatePathToPlayer", 0f, pathUpdateInterval);
                    state = "charge";
                    break;
                }
                // In every other case find the last player location and go there. When the position is reached and player is nowhere to be seen, return to spawn position.
                else
                {
                    rb.AddForce(force);
                    if (_collider.bounds.Contains(lastSeenTargetPosition))
                    {
                        returningFromChase = true;
                        rb.velocity = new Vector2(0, 0);
                        CancelInvoke();
                        InvokeRepeating("UpdatePathReturn", 0f, pathUpdateInterval);
                        state = "roam";
                        break;
                    }
                }
                break;
        }
    }

    // No purpose yet in flying enemy script.
    void PlayerPushback()
    {
        float pushbackX = target.transform.position.x - transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Math.Abs(pushbackX / 4)).normalized;
        playerRB.AddForce(knockbackDirection * knockbackForce);
    }

    public void SpawnHealthOrEnergy()
    {
        int rand = UnityEngine.Random.Range(1, 101);
        if (_targetHealth.GetHealth() <= _targetHealth.MaxHealth * amountWhenHealthIsSpawnable && rand <= healthProbability)
        {
            // Debug.Log(rand);
            Instantiate(healthItem, transform.position, Quaternion.identity);
        }
        else if (rand <= energyProbability)
        {
            // Debug.Log(rand);
            Instantiate(energyItem, transform.position, Quaternion.identity);
        }

    }

    private void FlipLocalScaleWithForce(Vector2 force)
    {
        if (force.x >= 0.1f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            isFacingRight = true;
        }
        else if (force.x <= -0.1f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            isFacingRight = false;
        }
    }

    // When enemy is ramming desperately into player. If hit, EXPLODE and deal damage to player!
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "Player" && canShoot)
        {
            _targetHealth.TakeDamage(explosionPower);
            Destroy(gameObject);
            PlayerPushback();
        }
        
    }
}
