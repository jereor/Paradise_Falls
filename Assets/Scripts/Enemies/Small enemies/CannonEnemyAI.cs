using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class CannonEnemyAI : MonoBehaviour
{
    private Health _targetHealth;

    public bool bossMode;

    [Header("Transforms")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform enemyGFX;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private Transform shootTransform;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject energyItem;
    [SerializeField] private GameObject healthItem;

    [Header("Layer Checks")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask playerLayer;

    [Header("State and Parameters")]
    [SerializeField] private string state = "wait";
    [SerializeField] private float aggroDistance = 5f;
    [SerializeField] private float shootingDistance = 10f;
    [SerializeField] private float wallCheckDistance = 2f; // How far of a wall the enemy truns around.
    [SerializeField] private float knockbackForce = 5f;

    [Header("Health and Energy Spawn values")]
    [SerializeField] private float healthProbability; // Value between 1-100. Higher the better chance.
    [SerializeField] private float energyProbability;
    [SerializeField] private float amountWhenHealthIsSpawnable; // MaxHealth value between 0-1. When your health sinks below a certain amount health becomes spawnable.

    [Header("Pathfinding info")]
    [SerializeField] private float nextWaypointDistance = 1f;
    [SerializeField] private float pathUpdateInterval = 1f;

    private bool canShoot = true;
    private float shootCooldown = 1.5f;
    private bool isFacingRight = true;
    //private float vectorPathLength = 1;
    private bool isTargetInBehaviourRange = false;

    private Vector2 spawnPosition;
    private Path path;
    private int currentWaypoint = 0;

    private Seeker seeker;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        // Set speed and state to charge that if bossMode is true enemy starts at charge state with charge speed
        if (bossMode)
        {
            state = "shoot";
        }

        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        _targetHealth = GameObject.Find("Player").GetComponent<Health>();
        spawnPosition = transform.position;
        Physics2D.IgnoreLayerCollision(3, 7);
        //Updates the path repeatedly with a chosen time interval
        InvokeRepeating("UpdatePathToPlayer", 0f, pathUpdateInterval);

    }
    void UpdatePathToPlayer()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    //Draws gizmos for enemy's "territory".
    private void OnDrawGizmosSelected()
    {
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
        if (path == null) { return; }

        // If the target is too far from the enemy unit, it respawns in to the spawn point and stays there until target is close enough again.
        // Enemy stops all actions for the time being.
        if ((target.transform.position - transform.position).magnitude > 60)
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

            //Calculates the next path point and the amount of force applied
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 force = direction * Time.deltaTime;

            //Distance between the enemy and next waypoint
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            //Keeps the count of the waypoints
            if (distance < nextWaypointDistance) { currentWaypoint++; }

            if (bossMode)
                EnemyBossStateChange(force);
            else
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
            // WAIT STATE
            //-------------------------------------------------------------------------------------------------------
            case "wait":
                //If target is close enough the enemy unit, charges it towards the player.              
                if (IsPlayerInAggroRange())
                {
                    CancelInvoke();
                    InvokeRepeating("UpdatePathToPlayer", 0f, pathUpdateInterval);
                    state = "shoot";
                    gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;
                    break;
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
                    // Draws two rays in the direction of the target. First checks if there's ground in between the enemy unit and the target, second checks if it hit the target.
                    RaycastHit2D hitGround;
                    RaycastHit2D hitPlayer;
                    hitGround = Physics2D.Raycast(transform.position, (target.transform.position - transform.position), (target.transform.position - transform.position).magnitude, groundLayer);
                    hitPlayer = Physics2D.Raycast(transform.position, (target.transform.position - transform.position), (target.transform.position - transform.position).magnitude, playerLayer);
                    if (hitPlayer && !hitGround)
                    {
                        // Instantiate a bullet prefab from enemy unit location.
                        GameObject bulletObject = Instantiate(bullet, shootTransform.position, Quaternion.identity);
                        bulletObject.GetComponent<BulletBehaviour>().shooter = this.gameObject;
                        StartCoroutine(ShootCoolDown());
                    }
                    // If the target is in shooting range but there's a ground in between, enemy unit tries to find a path past it.
                    else if (hitPlayer && hitGround)
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
                // If target goes out of enemy's bounds, return to "roam" state, otherwise start chasing again.
                if (!IsPlayerInAggroRange())
                {
                    CancelInvoke();
                    InvokeRepeating("UpdatePathReturn", 0f, pathUpdateInterval);
                    state = "wait";
                    gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
                    break;
                }
                break;
        }
    }

    // ENEMY BEHAVIOUR STATES
    // --------------------------------------------------------------------------------------------------------------------------
    private void EnemyBossStateChange(Vector2 force)
    {
        //switch-case system between different enemy states.
        switch (state)
        {

            // SHOOT STATE
            //-------------------------------------------------------------------------------------------------------------------
            //Does damage to target if close enough. Otherwise goes to roam or charge state.
            case "shoot":
                // Bullets do the damage, this only checks if the bullet can hit the target from current angle.
                gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
                Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.blue);
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
                        GameObject bulletObject = Instantiate(bullet, shootTransform.position, Quaternion.identity);
                        bulletObject.GetComponent<BulletBehaviour>().shooter = this.gameObject;
                        StartCoroutine(ShootCoolDown());
                    }
                    // If the target is in shooting range but there's a ground in between, enemy unit tries to find a path past it.
                    else if (hitPlayer && hitGround)
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
                break;
        }
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

    // Flips the localscale of the enemy unit by given force.
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
}