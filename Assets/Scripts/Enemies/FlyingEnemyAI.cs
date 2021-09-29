using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FlyingEnemyAI : MonoBehaviour
{
    private Health _targetHealth;

    [Header("Transforms")]
    public Transform target;
    public Transform enemyGFX;
    public Rigidbody2D playerRB;
    public GameObject bullet;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;

    [Header("Player Check")]
    [SerializeField] LayerMask playerLayer;

    [Header("Mobility")]
    public float speed = 200f;
    public float nextWaypointDistance = 3f;
    public float pathUpdateInterval = 1f;

    [Header("State and Parameters")]
    public string state = "roam";
    public float roamingRange = 2f;
    public float aggroDistance = 5f;
    public float shootingDistance = 10f;
    public float wallCheckDistance = 2f;
    public float knockbackForce = 5f;

    private bool returningFromChase = false;
    private bool canShoot = true;
    private float shootCooldown = 1.5f;

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
        }
        else if (rb.velocity.x <= -0.1f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        //ObstacleCheck();
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
        force = new Vector2(0, 50);

        if (hit.collider != null)
        {
            rb.AddForce(force);
        }
    }

    private void EnemyStateChange(Vector2 force)
    {
        //switch-case system between different enemy states.
        switch (state)
        {
            //Roams in a specified area given to the enemy unit and stays inside of it.
            case "roam":
                //Debug.Log("Roaming.");
                

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
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));                 
                }
                else if (transform.position.x <= (spawnPosition.x - roamingRange))
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));                
                }
                //If target is close enough the enemy unit, charges it towards the player.
                else if (aggroDistance >= path.GetTotalLength() && (target.transform.position.x >= (spawnPosition.x - roamingRange) && target.transform.position.x < (spawnPosition.x + roamingRange)))
                {
                    state = "charge";
                    speed = 10;
                    break;
                }
                if (transform.position.x <= (spawnPosition.x + roamingRange) && transform.position.x >= (spawnPosition.x - roamingRange))
                {
                    rb.AddForce(new Vector2(transform.localScale.x * speed, 0));
                }
                break;

            //Here enemy charges the player. Checks if player is inside enemy unit's roaming range.
            case "charge":

                
                //Debug.Log("Charging!");
                if (path.GetTotalLength() > aggroDistance || (target.transform.position.x <= (spawnPosition.x - roamingRange) || target.transform.position.x > (spawnPosition.x + roamingRange)))
                {
                    returningFromChase = true;
                    state = "roam";
                    speed = 5;
                    break;
                }
                else if (aggroDistance >= path.GetTotalLength() && path.GetTotalLength() > shootingDistance)
                {
                    rb.AddForce(force);
                }
                //If target is close enough the enemy unit, it changes the state to "shoot"
                else if (path.GetTotalLength() < shootingDistance)
                {
                    state = "shoot";
                }
                break;

            //Does damage to target if close enough. Otherwise goes to roam or charge state.
            case "shoot":
                //Do damage to player here
                Debug.DrawRay(transform.position, target.transform.position - transform.position, Color.blue);
                if (canShoot)
                {
                    RaycastHit2D hit;
                    hit = Physics2D.Raycast(transform.position, (target.transform.position - transform.position), (target.transform.position - transform.position).magnitude, playerLayer);
                    
                    if(hit.collider != null)
                    {
                        Debug.Log("PEW!");
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
                    //_targetHealth.TakeDamage(1);
                    //PlayerPushback();
                    StartCoroutine(ShootCoolDown());
                }
                if (target.transform.position.x <= (spawnPosition.x - roamingRange) || target.transform.position.x > (spawnPosition.x + roamingRange))
                {
                    state = "roam";
                    speed = 5;
                    break;
                }
                else if (path.GetTotalLength() > shootingDistance)
                {
                    state = "charge";
                    speed = 10;
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
