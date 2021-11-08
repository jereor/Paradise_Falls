using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class CannonEnemyAI : MonoBehaviour
{
    private Health _targetHealth;

    public bool bossMode;

    public bool staticMode;

    [Header("Transforms")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform cannonTransform;
    [SerializeField] private Transform cannon1;
    [SerializeField] private Transform cannon2;
    [SerializeField] private Transform shootTransform;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject energyItem;
    [SerializeField] private GameObject healthItem;

    [Header("Layer Checks")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask playerLayer;

    [Header("State and Parameters")]
    [SerializeField] public CannonStates currentState = CannonStates.Idle;
    [SerializeField] private float aggroDistance = 5f;
    [SerializeField] private float shootingDistance = 10f;
    [SerializeField] private float bossModeDistance = 20f;
    [SerializeField] private float detectionAreaOffsetY = 0.5f;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Health and Energy Spawn values")]
    [SerializeField] private float healthProbability; // Value between 1-100. Higher the better chance.
    [SerializeField] private float energyProbability;
    [SerializeField] private float amountWhenHealthIsSpawnable; // MaxHealth value between 0-1. When your health sinks below a certain amount health becomes spawnable.

    [Header("Pathfinding info")]
    [SerializeField] private float pathUpdateInterval = 1f;

    private bool canShoot = true;
    private float shootCooldown = 2f;
    [SerializeField] private float rotSpeed = 100f;
    private bool isFacingRight = true;
    //private float vectorPathLength = 1;
    private bool isTargetInBehaviourRange = false;

    private Vector2 spawnPosition;
    private Path path;

    private Seeker seeker;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        // Set speed and state to charge that if bossMode is true enemy starts at charge state with charge speed
        if (bossMode && !staticMode)
        {
            currentState = CannonStates.Aim;
            shootingDistance = bossModeDistance;
        }
        else if (staticMode)
        {
            currentState = CannonStates.Shoot;
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
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(shootTransform.position + new Vector3(0, detectionAreaOffsetY ,0), shootTransform.right * shootingDistance);
        Gizmos.DrawRay(shootTransform.position + new Vector3(0, -detectionAreaOffsetY, 0), shootTransform.right * shootingDistance);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
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
            if (bossMode)
                EnemyBossStateChange();
            else
                EnemyStateChange();
        }
    }

    private IEnumerator ShootCoolDown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    private bool IsPlayerInAggroRange()
    {
        if (bossMode)
            return true;

        return Physics2D.OverlapCircle(transform.position, aggroDistance, playerLayer);
    }

    private bool IsPlayerInShootingRange()
    {
        RaycastHit2D hitUpper = Physics2D.Raycast(shootTransform.position + new Vector3(0, detectionAreaOffsetY, 0), shootTransform.right, shootingDistance, playerLayer);
        RaycastHit2D hitLower = Physics2D.Raycast(shootTransform.position + new Vector3(0, -detectionAreaOffsetY, 0), shootTransform.right, shootingDistance, playerLayer);
        // If either of these hit player return true
        if (hitUpper || hitLower)
            return true;

        return false;
    }

    private bool IsHittingGround()
    {
        return Physics2D.CircleCast(transform.position, 4, transform.position, 4, groundLayer);
    }

    // ENEMY BEHAVIOUR STATES
    // --------------------------------------------------------------------------------------------------------------------------
    private void EnemyStateChange()
    {
        switch (currentState)
        {
            case CannonStates.Idle:
                HandleIdleState();
                break;
            case CannonStates.Aim:
                HandleAimState();
                break;
            case CannonStates.Shoot:
                HandleShootState();
                break;
        }
    }

    // ENEMY BEHAVIOUR STATES
    // --------------------------------------------------------------------------------------------------------------------------
    private void EnemyBossStateChange()
    {
        switch (currentState)
        {
            case CannonStates.Aim:
                HandleAimState();
                break;
            case CannonStates.Shoot:
                HandleShootState();
                break;
        }
    }

    public void SpawnHealthOrEnergy()
    {
        int rand = UnityEngine.Random.Range(1, 101);
        if (_targetHealth.GetHealth() <= _targetHealth.MaxHealth * amountWhenHealthIsSpawnable && rand <= healthProbability)
        {
            Instantiate(healthItem, transform.position, Quaternion.identity);
        }
        else if (rand <= energyProbability)
        {
            Instantiate(energyItem, transform.position, Quaternion.identity);
        }
    }

    private void HandleIdleState()
    {
        //If target is close enough the enemy unit, charges it towards the player.              
        if (IsPlayerInAggroRange())
        {
            CancelInvoke();
            InvokeRepeating("UpdatePathToPlayer", 0f, pathUpdateInterval);
            currentState = CannonStates.Aim;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.grey;
        }
    }

    private void HandleAimState()
    {
        Vector2 vectorToPlayer = target.transform.position - cannonTransform.position;

        // Angle to player
        float angle = Mathf.Atan2(vectorToPlayer.y, vectorToPlayer.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.Euler(new Vector3(0, 0, angle));

        // One of the check rays hit player go to shoot state
        if (IsPlayerInShootingRange())
        {
            currentState = CannonStates.Shoot;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
        // Turns the enemy unit torwards the target when aiming.
        cannon1.rotation = Quaternion.RotateTowards(cannon1.rotation, q, rotSpeed * Time.fixedDeltaTime);
        cannon2.rotation = Quaternion.RotateTowards(cannon2.rotation, q, rotSpeed * Time.fixedDeltaTime);

        // If target goes out of enemy's bounds, return to "idle" state
        if (!IsPlayerInAggroRange() && !staticMode && !bossMode)
        {
            currentState = CannonStates.Idle;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
        }
    }

    private void HandleShootState()
    {
        // Turns the enemy unit torwards the target when shooting.
        if (!IsPlayerInShootingRange() && !staticMode)
        {
            currentState = CannonStates.Aim;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.grey;
            return;
        }

        if(canShoot && staticMode) 
        {
            // Instantiate a bullet prefab from enemy unit location.
            GameObject bulletObject = Instantiate(bullet, shootTransform.position, Quaternion.identity);
            bulletObject.GetComponent<BulletBehaviour>().shooter = shootTransform.gameObject;
            bulletObject.GetComponent<BulletBehaviour>().staticShot = true;
            StartCoroutine(ShootCoolDown());
        }
        else if (canShoot && !staticMode)
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
        }
        // If target goes out of enemy's bounds, return to "idle" state
        if (!IsPlayerInAggroRange() && !staticMode && !bossMode)
        {
            currentState = CannonStates.Idle;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
        }
    }

    public enum CannonStates
    {
        Idle,
        Aim,
        Shoot
    }
}