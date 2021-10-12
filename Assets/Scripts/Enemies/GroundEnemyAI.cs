using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

#pragma warning disable 0414

public class GroundEnemyAI : MonoBehaviour
{
    private Health _targetHealth;
    private Energy _targetEnergy;

    private Health health;

    [Header("Transforms")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform enemyGFX;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private Transform groundDetection;
    [SerializeField] private GameObject energyItem;
    [SerializeField] private GameObject healthItem;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck; // GameObject attached to player that checks if touching ground
    [SerializeField] private float checkRadius; // Radius for ground checks
    [SerializeField] LayerMask groundLayer; // Chosen layer that is recognized as ground in ground checks

    [SerializeField] LayerMask playerLayer;

    [Header("Mobility")]
    [SerializeField] private float speed = 10000f;
    [SerializeField] private float roamingSpeed = 10000f;
    [SerializeField] private float chargeSpeed = 15000f;
    [SerializeField] private float jumpChargeSpeed = 700f;
    [SerializeField] private float jumpHeight = 500f;
    [SerializeField] private float walkStepInterval = 1f;
    [SerializeField] private float runStepInterval = 0.5f;
    [SerializeField] private float jumpChargeInterval = 1f;
    [SerializeField] private float attackPower = 2f;

    [Header("State and Parameters")]
    [SerializeField] private string state = "roam";
    [SerializeField] private Vector2 roamingRange = new Vector2(10, 10);
    [SerializeField] private Vector2 roamingOffset;
    [SerializeField] private Vector2 aggroDistance = new Vector2(5f, 5f);
    [SerializeField] private Vector2 aggroOffset;
    [SerializeField] private float aggroDistanceLength = 5f;
    [SerializeField] private Vector2 hitDistance = new Vector2(3f, 3f);
    [SerializeField] private Vector2 hitOffset;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private int jumpProbability = 10; // Range between 1 - 100. Lower number means lower chance to jump during chase. Creates variation to the enemy movement.

    [Header("Health and Energy Spawn values")]
    [SerializeField] private float healthProbability; // Value between 1-100. Higher the better chance.
    [SerializeField] private float energyProbability;
    [SerializeField] private float amountWhenHealthIsSpawnable; // MaxHealth value between 0-1. When your health sinks below a certain amount health becomes spawnable.

    [Header("Pathfinding info")]
    [SerializeField] private float nextWaypointDistance = 1f;
    [SerializeField] private float pathUpdateInterval = 1f;
    [SerializeField] private bool isFacingRight = true;

    private float hurtCounter = 0f;
    private bool isHurt = false;

    [Header("Check Distances for Behaviours")]
    [SerializeField] private float jumpableWallCheckDistance = 1.5f;
    [SerializeField] private float higherWallCheckDistance = 1.5f;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private Vector2 wallCheckDirection;
  
    private bool stunned = false;
    private bool canMove = true;
    private bool canJump = true;
    private bool canPunch = true;

    private float punchCooldown = 1.5f;
    private float healthCount;

    private Vector2 spawnPosition;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private bool gizmoPositionChange = true;
    private bool isTargetInBehaviourRange = false;

    private RaycastHit2D obstacleBetweenTarget;

    private Seeker seeker;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        _targetHealth = target.GetComponent<Health>();
        health = GetComponent<Health>();
        healthCount = health.CurrentHealth;
        spawnPosition = transform.position;
        gizmoPositionChange = false;

        //Updates the path repeatedly with a chosen time interval
        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }
    void UpdatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    //Draws gizmos for enemy's "territory".
    private void OnDrawGizmosSelected()
    {
        if (gizmoPositionChange)
        {
            Gizmos.DrawWireCube(new Vector2(transform.position.x + roamingOffset.x, transform.position.y + roamingOffset.y), roamingRange);
        }
        else
        {
            Gizmos.DrawWireCube(new Vector2(spawnPosition.x + roamingOffset.x, spawnPosition.y + roamingOffset.y), roamingRange);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x * aggroOffset.x), transform.position.y + aggroOffset.y), aggroDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x * hitOffset.x), transform.position.y + hitOffset.y), hitDistance);

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
        if (path == null) { return; }

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

        if (isTargetInBehaviourRange)
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

            //Calculates the next path point and the amount of force applied on X-axis
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 forceX = new Vector2(direction.x, 0).normalized * speed * Time.deltaTime;

            //Distance between the enemy and next waypoint
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            //Keeps the count of the waypoints
            if (distance < nextWaypointDistance) { currentWaypoint++; }

            // Has enemy unit taken damage after last update.
            if(health.CurrentHealth < healthCount && state == "roam" && !isHurt)
            {
                Debug.Log("It hurts...");
                healthCount = health.CurrentHealth;
                StartCoroutine(Stunned(2));
                state = "charge";
                isHurt = true;
            }

            obstacleBetweenTarget = Physics2D.Raycast(transform.position, (target.transform.position - transform.position).normalized, (target.transform.position - transform.position).magnitude, LayerMask.GetMask("Ground"));
            Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.blue);

            // If enemy is hurt, it chases the target.
            if(isHurt)
            {
                hurtCounter += Time.deltaTime;
            }
            if(hurtCounter >= 5)
            {
                isHurt = false;
                hurtCounter = 0;
            }

            ObstacleCheck();

            EnemyStateChange(forceX, obstacleBetweenTarget);
        }


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

    private IEnumerator JumpChargeCoolDown()
    {
        canMove = false;
        yield return new WaitForSeconds(jumpChargeInterval);
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
        rb.AddForce(new Vector2(100 * jumpDirection, 0));
    }

    // Trying to raycast and check if there's an obstacle in front of the enemy. Function also checks if the obstacle is too high to jump over and turns around if impossible to get over.
    // Third ray checks if there's a pit coming ahead so the enemy unit doesn't fall off from the edge.
    private void ObstacleCheck()
    {
        RaycastHit2D hitHorizontal;
        RaycastHit2D hitAngularUp;
        RaycastHit2D hitDown;
        float jumpDirection;
        Vector2 force;
        Vector2 jumpPosition = transform.position;

        // Casts rays in the direction enemy unit is facing.
        if (isFacingRight)
        {
            hitHorizontal = Physics2D.Raycast(transform.position, transform.right, jumpableWallCheckDistance, groundLayer);
            hitAngularUp = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 1), wallCheckDirection.normalized, higherWallCheckDistance, groundLayer);
            hitDown = Physics2D.Raycast(groundDetection.transform.position, Vector2.down, groundCheckDistance, groundLayer);
            Debug.DrawRay(transform.position, transform.right * jumpableWallCheckDistance, Color.red);
            Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + 1), wallCheckDirection.normalized * higherWallCheckDistance, Color.red);
            Debug.DrawRay(groundDetection.transform.position, Vector2.down * groundCheckDistance, Color.red);
            jumpDirection = 1;
        }
        else
        {
            hitHorizontal = Physics2D.Raycast(transform.position, -transform.right, jumpableWallCheckDistance, groundLayer);
            hitAngularUp = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 1), new Vector2(-wallCheckDirection.x, wallCheckDirection.y).normalized, higherWallCheckDistance, groundLayer);
            hitDown = Physics2D.Raycast(groundDetection.transform.position, Vector2.down, groundCheckDistance, groundLayer);
            Debug.DrawRay(transform.position, -transform.right * jumpableWallCheckDistance, Color.red);
            Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + 1), new Vector2(-wallCheckDirection.x, wallCheckDirection.y).normalized * higherWallCheckDistance, Color.red);
            Debug.DrawRay(groundDetection.transform.position, Vector2.down * groundCheckDistance, Color.red);
            jumpDirection = -1;
        }


        // If there's no ground ahead, turns around and starts going back.
        if ((hitHorizontal.collider != null && hitAngularUp.collider != null) || hitDown.collider == null && IsGrounded())
        {
            if (hitDown.collider == null)
            {
                Debug.Log("No ground ahead!");
            }
            if(isFacingRight)
            {
                isFacingRight = false;
            }
            else
            {
                isFacingRight = true;
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

    // Checks if target is in a box shaped area given by parameters.
    private bool IsPlayerInRange()
    {
        return Physics2D.OverlapBox(new Vector2(spawnPosition.x + roamingOffset.x, spawnPosition.y + roamingOffset.y), roamingRange, 0, playerLayer);
    }

    private bool IsPlayerInPunchingRange()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * hitOffset.x), transform.position.y + hitOffset.y), hitDistance, 0, playerLayer);
    }

    private bool IsPlayerInAggroRange()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * aggroOffset.x), transform.position.y + aggroOffset.y), aggroDistance, 0, playerLayer);
    }



    // ENEMY BEHAVIOUR STATES
    // ---------------------------------------------------------------------------------------------------------------
    private void EnemyStateChange(Vector2 forceX, RaycastHit2D obstacleBetweenTarget)
    {
        // switch-case system between different enemy states.
        switch (state)
        {
            // ROAM STATE
            //-------------------------------------------------------------------------------------------------------
            // Roams in a specified area given to the enemy unit and stays inside of it.
            case "roam":
                if (stunned) break;
                // If the enemy unit tries to go outside of the given area parameters, it turns around.
                if (transform.position.x >= (spawnPosition.x + roamingRange.x / 2 + roamingOffset.x) && canMove && IsGrounded())
                {
                    //Debug.Log("Left");
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                    isFacingRight = false;
                    rb.AddForce(new Vector2(transform.localScale.x * speed * Time.deltaTime, 0));
                    StartCoroutine(WalkCoolDown());
                    break;
                }
                else if (transform.position.x <= (spawnPosition.x - roamingRange.x / 2 + roamingOffset.x) && canMove && IsGrounded())
                {
                    //Debug.Log("Right");
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    isFacingRight = true;
                    rb.AddForce(new Vector2(transform.localScale.x * speed * Time.deltaTime, 0));
                    StartCoroutine(WalkCoolDown());
                    break;
                }
                // If target is close enough the enemy unit, charges it towards the player.
                if ((IsPlayerInAggroRange() || isHurt) && IsPlayerInRange() && !obstacleBetweenTarget)
                {
                    speed = chargeSpeed;
                    state = "charge";
                    break;
                }
                // If the enemy unit is inside the given roaming range and target is nowhere near, it roams around.
                if (transform.position.x <= (spawnPosition.x + roamingRange.x) && transform.position.x >= (spawnPosition.x - roamingRange.x) && canMove && IsGrounded())
                {
                    rb.AddForce(new Vector2(transform.localScale.x * speed * Time.deltaTime, 0));
                    StartCoroutine(WalkCoolDown());
                }
                break;

            // CHARGE STATE
            //------------------------------------------------------------------------------------------------------------------
            //Here enemy charges the target. Checks if target is inside enemy unit's roaming range.
            case "charge":                
                if (stunned) break;
                gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                // Outside the range, return to roam state.
                if ((!IsPlayerInAggroRange() && !isHurt) || !IsPlayerInRange())
                {
                    gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.black;
                    speed = roamingSpeed;
                    state = "roam";
                    break;
                }
                // Inside the range, runs towards the target or jump randomly towards the target
                if ((IsPlayerInAggroRange() && canMove && !IsPlayerInPunchingRange()) || (!IsPlayerInAggroRange() && canMove && isHurt && !IsPlayerInPunchingRange()))
                {
                    int rand = UnityEngine.Random.Range(1, 101);
                    FlipLocalScaleWithForce(forceX);
                    if(rand <= jumpProbability)
                    {
                        Vector2 force = new Vector2(transform.localScale.x * jumpHeight * 1.5f, jumpHeight).normalized;
                        FlipLocalScaleWithForce(force);                        
                        rb.AddForce(force * jumpChargeSpeed * Time.deltaTime, ForceMode2D.Impulse);                       
                        StartCoroutine(JumpChargeCoolDown());
                    }
                    else
                    {
                        FlipLocalScaleWithForce(forceX);
                        rb.AddForce(forceX);                       
                        StartCoroutine(RunCoolDown());
                    }
                    break;
                }
                //If target is close enough the enemy unit, it changes the state to "punch"
                if (IsPlayerInPunchingRange())
                {
                    state = "punch";
                }
                break;

            // PUNCH STATE
            //------------------------------------------------------------------------------------------------------------------
            //Does damage to target if close enough. Otherwise goes to roam or charge state.
            case "punch":
                if (stunned) break;
                if (canPunch)
                {
                    //Do damage to player here
                    Debug.Log("Player hit");

                    // Turns the enemy unit torwards the target when punching.
                    if (target.transform.position.x - transform.position.x >= 0)
                    {
                        isFacingRight = true;
                        transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                    else
                    {
                        isFacingRight = false;
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                    }

                    if (target.TryGetComponent(out Shield shield))
                    {
                        if (shield.Parrying)
                        {
                            target.GetComponent<Shield>().HitWhileParried(); // Tell player parry was successful
                            StartCoroutine(Stunned(1.5f)); // Get stunned
                        }
                        else
                        {
                            _targetHealth.TakeDamage(attackPower);

                            //PlayerPushback();
                            StartCoroutine(PlayerHit());
                            StartCoroutine(PunchCoolDown());
                        }
                    }
                    else
                    {
                        _targetHealth.TakeDamage(attackPower);

                        //PlayerPushback();
                        StartCoroutine(PlayerHit());
                        StartCoroutine(PunchCoolDown());
                    }
                }

                // If target goes out of enemy's bounds, return to "roam" state
                if (!IsPlayerInRange())
                {
                    gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.black;
                    speed = roamingSpeed;
                    state = "roam";
                    break;
                }
                else if (!IsPlayerInPunchingRange() && IsPlayerInAggroRange())
                {
                    speed = chargeSpeed;
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

    IEnumerator Stunned(float stunTime)
    {
        Debug.Log("Stunned...");
        stunned = true;
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.blue;

        float timer = stunTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Stun ended");
        stunned = false;
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.black;
    }

    // Briefly flashes player sprite red when enemy hits them.
    IEnumerator PlayerHit()
    {
        GameObject.Find("Player").GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.1f);
        GameObject.Find("Player").GetComponent<SpriteRenderer>().color = Color.white;
    }

    // Flip the local scale of the enemy by force value.
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

    // Small knockback that can be activated by player and environment. Knockback knocks slightly upwards so the friction doesn't stop it right away.
    public void KnockBackEnemy(GameObject from, float force)
    {
        float pushbackX = transform.position.x - from.transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Mathf.Abs(pushbackX / 4)).normalized;
        rb.AddForce(knockbackDirection * force);
    }
    
    // Function for energy or health drop when enemy dies or in any other conditions. Probabilites are assigned with the probability values and health amount. Both drops cannot spawn at the same time.
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

    // If enemy is hit by the flying melee weapon, it gets hurt and sees the player right away.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.tag == "MeleeWeapon")
        {
            isHurt = true;
        }
    }
}

