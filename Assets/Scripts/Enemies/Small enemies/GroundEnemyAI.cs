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

    public bool bossMode;

    private Health health;

    [Header("Transforms")]
    public GameObject target;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private Transform groundDetection;
    [SerializeField] private GameObject energyItem;
    [SerializeField] private GameObject healthItem;
    private SpriteRenderer spriteRndr;
    private Animator animator;

    [Header("Ground And Player Check")]
    [SerializeField] private Transform groundCheck; // GameObject attached to player that checks if touching ground
    [SerializeField] private float checkRadius; // Radius for ground checks
    [SerializeField] LayerMask groundLayer; // Chosen layer that is recognized as ground in ground checks
    [SerializeField] LayerMask pipeLayer;

    [SerializeField] LayerMask playerLayer;

    [Header ("Enemy State")]
    public EnemyState enemyState = EnemyState.Idle;
    private EnemyState stateChangeIdentfier; // Used to determine right animations and so it doesn't loop the function for no reason in update.

    [Header("Mobility")]
    [SerializeField] private float speed = 10000f;
    [SerializeField] private float roamingSpeed = 10000f;
    [SerializeField] private float chargeSpeed = 15000f;
    [SerializeField] private float jumpChargeSpeed = 700f;
    [SerializeField] private float jumpHeight = 500f;
    [SerializeField] private float walkStepInterval = 1f;
    [SerializeField] private float runStepInterval = 0.5f;
    [SerializeField] private float jumpChargeInterval = 1f;
    [SerializeField] private float punchChargeTime = 1f;
    [SerializeField] private float punchCooldown = 1.5f;
    [SerializeField] private float attackPower = 2f;
    [SerializeField] private float staggerTime = 0.7f;
    [SerializeField] private float stunTime = 1.5f;

    [Header("State and Parameters")]
    [SerializeField] private Vector2 roamingRange = new Vector2(10, 10);
    [SerializeField] private Vector2 roamingOffset;
    [SerializeField] private Vector2 aggroDistance = new Vector2(5f, 5f);
    [SerializeField] private Vector2 aggroOffset;
    [SerializeField] private Vector2 hitDistance = new Vector2(3f, 3f);
    [SerializeField] private Vector2 hitOffset;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private int jumpProbability = 10; // Range between 1 - 100. Lower number means lower chance to jump during chase. Creates variation to the enemy movement.
    [SerializeField] private float forcedAggroTime = 3f;

    [Header("Health and Energy Spawn values")]
    [SerializeField] private float healthProbability; // Value between 1-100. Higher the better chance.
    [SerializeField] private float energyProbability;
    [SerializeField] private float amountWhenResourceIsSpawnable; // MaxHealth value between 0-1. When your health sinks below a certain amount health becomes spawnable.

    [Header("Pathfinding info")]
    [SerializeField] private float nextWaypointDistance = 1f;
    [SerializeField] private float pathUpdateInterval = 1f;
    private bool isFacingRight = true;

    private float hurtCounter = 0f;  
    private bool isForcedToAggro = false;

    [Header("Check Distances for Behaviours")]
    [SerializeField] private float jumpableWallCheckDistance = 1.5f;
    [SerializeField] private float higherWallCheckDistance = 1.5f;
    [SerializeField] private float obstacleJumpforce = 140;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private Vector2 wallCheckDirection;
  
    private bool stunned = false;
    private bool canMove = true;
    private bool canJump = true;
    private bool canPunch = true;
    
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

    private bool playMeleeSound = false;

    // Start is called before the first frame update
    void Start()
    {
        // Set speed and state to charge that if bossMode is true enemy starts at charge state with charge speed

        target = GameObject.Find("Player");
        playerRB = target.GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        _targetHealth = target.GetComponent<Health>();
        _targetEnergy = target.GetComponent<Energy>();
        health = GetComponent<Health>();
        spriteRndr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        healthCount = health.GetHealth();
        spawnPosition = transform.position;
        gizmoPositionChange = false;
        //animator.SetFloat("AttackTime", punchCooldown + punchChargeTime);

        if (bossMode)
        {
            enemyState = EnemyState.BossModeCharge;
            animator.SetBool("Run", true);
            speed = chargeSpeed;
        }
        else
        {
            animator.SetBool("Walk", true);
        }

        //Updates the path repeatedly with a chosen time interval
        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }
    void UpdatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, target.transform.position, OnPathComplete);
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
        if (path == null || target == null) { return; }

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

        // Every action enemy takes falls under this if-statement. If target is absolutely out of reach (currently hard coded as 60), start the action.
        if (isTargetInBehaviourRange)
        {
            if (isForcedToAggro)
            {
                hurtCounter += Time.deltaTime;
            }

            // Forced aggro timer. If it reaches the given parameter and target isn't in aggro range, return to normal roam state. (This is done in the state function.)
            if (hurtCounter >= forcedAggroTime)
            {
                isForcedToAggro = false;
                hurtCounter = 0;
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
            Vector2 forceX = new Vector2(direction.x, 0).normalized * speed * Time.deltaTime;

            //Distance between the enemy and next waypoint
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            //Keeps the count of the waypoints
            if (distance < nextWaypointDistance) { currentWaypoint++; }

            obstacleBetweenTarget = Physics2D.Raycast(transform.position, (target.transform.position - transform.position).normalized, (target.transform.position - transform.position).magnitude, LayerMask.GetMask("Ground"));
            Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.blue);

            // Has enemy unit taken damage after last update without noticing it itself.
            if (health.GetHealth() < healthCount)
            {
                // If there's damage taken and enemy isn't aggroed, force the aggro on enemy towards the target.
                // This means that player has done a surprise attack. Stun the enemy briefly by given parameter.
                healthCount = health.GetHealth();
                hurtCounter = 0;
                StopAllCoroutines();
                if (enemyState == EnemyState.Roam && !obstacleBetweenTarget)
                {
                    //Debug.Log("It hurts...");
                    healthCount = health.GetHealth();
                    enemyState = EnemyState.Stunned;

                }
                else
                {
                    StartCoroutine(Staggered(staggerTime));
                }
            }

            // Checks only for ground ahead, jumpable obstacles and walls.
            if(!bossMode || enemyState != EnemyState.Charge || enemyState != EnemyState.Punch)
                ObstacleCheck();


            switch (enemyState)
            {
                case EnemyState.Idle:
                    HandleIdleState();
                    break;

                case EnemyState.Roam:
                    HandleRoamState();
                    break;

                case EnemyState.Charge:
                    HandleChargeState(forceX);
                    break;

                case EnemyState.Punch:
                    HandlePunchState();
                    break;

                case EnemyState.Stunned:
                    HandleStunnedState();
                    break;

                case EnemyState.Staggered:
                    //HandleStaggeredState();
                    break;

                case EnemyState.BossModeCharge:
                    HandleBossModeCharge(forceX);
                    break;

                case EnemyState.BossModePunch:
                    HandleBossModePunch();
                    break;

                default:
                    break;
            }

            if (enemyState != stateChangeIdentfier)
            {
                HandleAnimations();
                stateChangeIdentfier = enemyState;
            }
        }
    }

    private void HandleIdleState()
    {

    }

    private void HandleRoamState()
    {
        if (stunned) enemyState = EnemyState.Stunned;
        // If the enemy unit tries to go outside of the given area parameters, it turns around.
        if (transform.position.x >= (spawnPosition.x + roamingRange.x / 2 + roamingOffset.x) && canMove && IsGrounded())
        {
            //Debug.Log("Left");
            Flip();
            Move();
            StartCoroutine(WalkCoolDown());
            return;
        }
        else if (transform.position.x <= (spawnPosition.x - roamingRange.x / 2 + roamingOffset.x) && canMove && IsGrounded())
        {
            //Debug.Log("Right");
            Flip();
            Move();
            StartCoroutine(WalkCoolDown());
            return;
        }
        // If target is close enough the enemy unit, charges it towards the player.
        if ((IsPlayerInAggroRange() || isForcedToAggro) && IsPlayerInRange() && !obstacleBetweenTarget)
        {
            speed = chargeSpeed;
            pathUpdateInterval = 0.5f;
            enemyState = EnemyState.Charge;
            return;
        }
        // If the enemy unit is inside the given roaming range and target is nowhere near, it roams around.
        if (transform.position.x <= (spawnPosition.x + roamingRange.x) && transform.position.x >= (spawnPosition.x - roamingRange.x) && canMove && IsGrounded())
        {
            Move();
            StartCoroutine(WalkCoolDown());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ShieldGrindPipe"))
        {
            Debug.Log("YEs");
            //Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), groundCollider);
            //Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), pipeCollider);
        }
    }

    private void HandleChargeState(Vector2 force)
    {
        if (stunned) enemyState = EnemyState.Stunned;
        if (!obstacleBetweenTarget) hurtCounter = 0;
        // Outside the range, return to roam state.
        if ((!IsPlayerInAggroRange() && !isForcedToAggro) || !IsPlayerInRange())
        {
            speed = roamingSpeed;
            pathUpdateInterval = 1;
            enemyState = EnemyState.Roam;
            return;
        }
        // Inside the range, runs towards the target or jump randomly towards the target
        if (IsGrounded() && canMove && ((IsPlayerInAggroRange() && !IsPlayerInPunchingRange()) || (!IsPlayerInAggroRange() && isForcedToAggro && !IsPlayerInPunchingRange())))
        {
            int rand = UnityEngine.Random.Range(1, 101);
            if (rand <= jumpProbability && canJump)
            {
                StartCoroutine(JumpCharge(force));
                return;
            }
            else
            {
                FlipLocalScaleWithForce(force);
                Move();

                StartCoroutine(RunCoolDown());
                return;
            }
        }
        //If target is close enough the enemy unit, it changes the state to "punch"
        if (IsPlayerInPunchingRange())
        {
            enemyState = EnemyState.Punch;
        }
    }

    private void HandlePunchState()
    {
        if (stunned) enemyState = EnemyState.Stunned;

        // If target goes out of enemy's bounds, return to "roam" state
        if(canPunch)
        {
            if (!IsPlayerInRange())
            {
                speed = roamingSpeed;
                pathUpdateInterval = 1;
                enemyState = EnemyState.Roam;
                return;
            }
            else if (!IsPlayerInPunchingRange())
            {
                isForcedToAggro = true;
                speed = chargeSpeed;
                pathUpdateInterval = 0.5f;
                enemyState = EnemyState.Charge;
                return;
            }

            if (IsPlayerInPunchingRange())
            {
                StartCoroutine(Attack());
            }
        }



    }

    private void HandleStunnedState()
    {
        if (!stunned)
        {
            StartCoroutine(Stunned(stunTime));
        }

    }

    //private void HandleStaggeredState()
    //{
    //    if(!stunned)
    //    {
    //        StartCoroutine(Stunned(1f));
    //    }
    //}

    private void HandleBossModeCharge(Vector2 force)
    {
        if (stunned) enemyState = EnemyState.Stunned;
        // Inside the range, runs towards the target or jump randomly towards the target
        if (IsGrounded() && ((canMove && !IsPlayerInPunchingRange()) || (!IsPlayerInAggroRange() && canMove && !IsPlayerInPunchingRange())))
        {
            int rand = UnityEngine.Random.Range(1, 101);
            if (rand <= jumpProbability && IsGrounded())
            {
                StartCoroutine(JumpCharge(force));
            }
            else
            {
                FlipLocalScaleWithForce(force);
                Move();
                StartCoroutine(RunCoolDown());
            }
        }
        //If target is close enough the enemy unit, it changes the state to "punch"
        if (IsPlayerInPunchingRange())
        {
            enemyState = EnemyState.BossModePunch;
        }
    }

    private void HandleBossModePunch()
    {
        if (stunned) enemyState = EnemyState.Stunned;
        if (canPunch && IsPlayerInPunchingRange())
        {
            StartCoroutine(Attack());
        }
        else if (!IsPlayerInPunchingRange())
        {
            speed = chargeSpeed;
            enemyState = EnemyState.BossModeCharge;
        }
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
        // Check if enemy will fall through pipe
        if(Physics2D.OverlapCircle(groundCheck.position, checkRadius, pipeLayer) != null)
        {
            // Get colliders that are on pipe prefab
            List<Collider2D> colliders = new List<Collider2D>();
            colliders.Add(Physics2D.OverlapCircle(groundCheck.position, checkRadius, pipeLayer));
            colliders.Add(Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer));
            // Ignore collisions between these colliders
            foreach (var collider in colliders)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collider);
            }
            return false;
        }

        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    // Checks if target is in a box shaped area given by parameters.
    //private bool IsUnitInRoamingRange()
    //{
    //    return Physics2D.OverlapBox(new Vector2(spawnPosition.x + roamingOffset.x, spawnPosition.y + roamingOffset.y), roamingRange, 0,, boxCollider);
    //}

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

    // Small knockback to the target when too close to the enemy unit. Knockback knocks slightly upwards so the friction doesn't stop the target right away.
    void PlayerPushback()
    {
        float pushbackX = target.transform.position.x - transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Math.Abs(pushbackX / 4)).normalized;
        playerRB.AddForce(knockbackDirection * knockbackForce * Time.deltaTime);
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
        if (_targetHealth.GetHealth() <= _targetHealth.GetMaxHealth() * amountWhenResourceIsSpawnable && rand <= healthProbability)
        {
            // Debug.Log(rand);
            Instantiate(healthItem, transform.position, Quaternion.identity);
        }
        else if (_targetEnergy.GetEnergy() <= _targetEnergy.getMaxEnergy() * amountWhenResourceIsSpawnable && rand <= energyProbability)
        {
            // Debug.Log(rand);
            Instantiate(energyItem, transform.position, Quaternion.identity);
        }

    }

    private void Move()
    {
        if(IsGrounded())
            rb.AddForce(new Vector2(transform.localScale.x * speed * Time.deltaTime, 0));

    }


    private void Flip()
    {
        // Character flip
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // If enemy is hit by the flying melee weapon, enemy is forced to aggro.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "MeleeWeapon")
        {
            isForcedToAggro = true;
            hurtCounter = 0;
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

    private IEnumerator JumpCharge(Vector2 force)
    {
        canJump = false;
        canMove = false;
        yield return new WaitForSeconds(jumpChargeInterval);
        Vector2 jumpForce = new Vector2(transform.localScale.x * 1.5f, 1);
        FlipLocalScaleWithForce(force);
        rb.AddForce(jumpForce * jumpChargeSpeed, ForceMode2D.Impulse);
        if(IsGrounded())
        {
            yield return new WaitForSeconds(jumpChargeInterval);
            canMove = true;
            canJump = true;
        }

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

    IEnumerator Stunned(float stunT)
    {
        stunned = true;
        spriteRndr.color = Color.blue;
        float timer = stunT;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        stunned = false;
        canMove = true;
        canJump = true;
        canPunch = true;
        isForcedToAggro = true;
        hurtCounter = 0;
        speed = chargeSpeed;
        pathUpdateInterval = 0.5f;
        spriteRndr.color = Color.white;
        enemyState = EnemyState.Charge;
    }

    // Briefly flashes player sprite red when enemy hits them.
    //IEnumerator PlayerHit()
    //{
    //    GameObject.Find("Player").GetComponent<SpriteRenderer>().color = Color.red;
    //    yield return new WaitForSeconds(0.1f);
    //    GameObject.Find("Player").GetComponent<SpriteRenderer>().color = Color.white;
    //}

    private IEnumerator Attack()
    {
        canPunch = false;
        StartCoroutine(MeleeSoundWait(punchChargeTime / 2f));
        yield return new WaitForSeconds(punchChargeTime);
        if (IsPlayerInPunchingRange())
        {
            //Do damage to player here
            //Debug.Log("Player hit");

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
                    enemyState = EnemyState.Stunned; // Get stunned
                }
                else
                {
                    //_targetHealth.TakeDamage(attackPower);

                    //PlayerPushback();
                    //StartCoroutine(PlayerHit());
                }
            }
            else
            {
                //_targetHealth.TakeDamage(attackPower);

                //PlayerPushback();
                //StartCoroutine(PlayerHit());
            }
        }
        else
        {
            //Debug.Log("Player dodged the attack?!");
        }
        yield return new WaitForSeconds(punchCooldown);
        canPunch = true;
    }

    public IEnumerator MeleeSoundWait(float duration)
    {
        yield return new WaitForSeconds(duration);
        playMeleeSound = true;
    }

    public void DealDamage()
    {
        if(IsPlayerInPunchingRange())
        {
            _targetHealth.TakeDamage(attackPower);
        }
    }

    // This function adds force on X-axis, so the enemy unit doesn't get stuck to small obstacles when moving
    private IEnumerator JumpForceForward(float jumpDirection)
    {
        yield return new WaitForSeconds(0.4f);
        rb.AddForce(new Vector2(obstacleJumpforce * jumpDirection * Time.deltaTime, 0));
    }

    private IEnumerator Staggered(float time)
    {
        stunned = true;
        //spriteRndr.color = Color.magenta;
        yield return new WaitForSeconds(time);
        speed = chargeSpeed;
        pathUpdateInterval = 0.5f;
        if (bossMode)
            enemyState = EnemyState.BossModeCharge;
        else
            enemyState = EnemyState.Charge;

        isForcedToAggro = true;
        hurtCounter = 0;
        canMove = true;
        canJump = true;
        canPunch = true;
        //spriteRndr.color = Color.white;
        stunned = false;
    }

    private void HandleAnimations()
    {
        if(enemyState != EnemyState.Staggered)
        {
            animator.SetBool("Stagger", false);
        }
        if(enemyState == EnemyState.Roam)
        {
            animator.SetBool("Walk", true);
            animator.SetBool("Run", false);
            animator.SetBool("Attack", false);
            animator.SetBool("Stagger", false);
        }

        if(enemyState == EnemyState.Charge)
        {
            animator.SetBool("Run", true);
            animator.SetBool("Walk", false);
            animator.SetBool("Attack", false);
            animator.SetBool("Stagger", false);
        }

        if (enemyState == EnemyState.Punch)
        {
            animator.Play("Attack");
            animator.SetBool("Attack", true);
            animator.SetBool("Run", false);
            animator.SetBool("Walk", false);
        }

        if (enemyState == EnemyState.Stunned)
        {
            animator.SetBool("Stagger", true);
            animator.SetBool("Run", false);
            animator.SetBool("Walk", false);
            animator.SetBool("Attack", false);
        }

        if (enemyState == EnemyState.Staggered)
        {
            animator.SetBool("Stagger", true);
            animator.SetBool("Run", false);
            animator.SetBool("Walk", false);
            animator.SetBool("Attack", false);
        }

        if (enemyState == EnemyState.BossModeCharge)
        {
            animator.SetBool("Run", true);
            animator.SetBool("Walk", false);
        }

        if (enemyState == EnemyState.BossModePunch)
        {
            animator.Play("Attack");
            animator.SetBool("Run", false);
            animator.SetBool("Walk", false);
        }

        if (enemyState == EnemyState.Idle)
        {
            animator.SetBool("Idle", true);
            animator.SetBool("Run", false);
            animator.SetBool("Walk", false);
        }
    }


    public enum EnemyState
    {
        Idle,
        Roam,
        Charge,
        Punch,
        Stunned,
        Staggered,
        BossModeCharge,
        BossModePunch
    }

    public bool getPlaySoundMelee()
    {
        if (playMeleeSound)
        {
            playMeleeSound = false;
            return true;
        }
        else
            return false;
    }
}

