using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BigPlantController : MonoBehaviour
{
    private GameObject target;
    private Rigidbody2D targetRB;
    private GameObject spikyVine;
    private SmallEnemySpawnController enemyController;

    public Health health;

    [SerializeField] private GameObject workerDroneOnePrefab;
    [SerializeField] private GameObject workerDroneTwoPrefab;
    private GameObject workerDroneOneInstance;
    private GameObject workerDroneTwoInstance;

    [SerializeField] private GameObject flyingDroneOnePrefab;
    [SerializeField] private GameObject flyingDroneTwoPrefab;
    private GameObject flyingDroneOneInstance;
    private GameObject flyingDroneTwoInstance;

    public GameObject escapePlatform;


    [SerializeField] private GameObject attackVine;
    public List<GameObject> attackVineInstances;

    [Header("Current State")]
    public PlantState state = PlantState.Idle;

    private Vector2 spikyVineStartPosition;
    private Vector2 spikyVineEndPosition;
    private Vector2 velocityPlayer;

    [SerializeField] private float attackVineStretchDuration;
    [SerializeField] private float attackVineRotateDuration;
    [SerializeField] private float attackVineMoveDuration;
    [SerializeField] private float attackVineWaitTime;
    [SerializeField] private float attackVineStretchAmount;
    [SerializeField] private float stunTime;
    [SerializeField] private float knockbackForce;
    public float vineSpeed;


    private bool isCovered = false;
    private bool isRoaring = false;
    private bool isEnemiesSpawned = false;
    private bool isAttackVineActivated = false;
    private bool isRotatingTowardsTarget = false;
    private bool knockbackOnCooldown = false;
    private bool spawnWorkerDrones = true;
    private bool phaseTwoActionsInitiated = false;

    void Start()
    {
        attackVineInstances = new List<GameObject>();
        target = GameObject.Find("Player");
        targetRB = target.GetComponent<Rigidbody2D>();
        spikyVine = transform.GetChild(0).gameObject;
        spikyVineStartPosition = spikyVine.transform.position;
        spikyVineEndPosition = new Vector2(spikyVineStartPosition.x, transform.position.y);

        health = GetComponent<Health>();

        //attackVine = transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // If bosses health goes down more that 50%, change phase. The way of handling the state variation changes most likely in the future.
        if (health.GetHealth() <= health.GetMaxHealth() * 0.5f)
        {
            state = PlantState.PhaseTwo;
            ChangeToDefaultLayer();
        }


        switch (state)
        {
            case PlantState.Idle:
                HandleIdleState();
                break;

            case PlantState.Protected:
                HandleProtectedState();
                break;

            case PlantState.Angri:
                HandleAngriState();
                break;

            case PlantState.Stunned:
                HandleStunnedState();
                break;

            case PlantState.PhaseTwo:
                HandlePhaseTwoState();
                break;

            case PlantState.Charge:
                HandleChargeState();
                break;

            case PlantState.SeedShoot:
                HandleSeedShootState();
                break;

            case PlantState.Artillery:
                HandleArtilleryState();
                break;

            case PlantState.EvilLaugh:
                HandleEvilLaughState();
                break;

            case PlantState.LightAttack:
                HandleLightAttackState();
                break;

            case PlantState.PlayerIsDead:
                HandlePlayerIsDeadState();
                break;
        }
    }

    private void HandleIdleState()
    {
        // Roar to player the go protected state
        if (!isRoaring)
            StartCoroutine(Roar());

    }

    private void HandleProtectedState()
    {
        // Covers itself -> Enemies spawns
        // Waits for enemies to be killed
        gameObject.GetComponent<SpriteRenderer>().color = Color.blue;

        if (!isCovered)
            Cover();

        if (!isEnemiesSpawned)
            SpawnEnemies();

        if (workerDroneOneInstance == null && workerDroneTwoInstance == null && flyingDroneOneInstance == null && flyingDroneTwoInstance == null)
        {
            isEnemiesSpawned = false;
            state = PlantState.Angri;
        }

    }

    private void HandleAngriState()
    {
        // Enemies are dead
        // Vines spawn -> Comes forth&follows player movement -> starts shaking -> charge
        // If hit boss, stunned
        // If hit player, player take damage, spawn new vine

        gameObject.GetComponent<SpriteRenderer>().color = Color.red;

        if (attackVineInstances.Count == 0)
        {
            StartCoroutine(SpawnVine());
            //attackVine.transform.rotation = new Quaternion(0, 0, -180,0);
            isRotatingTowardsTarget = true;
        }

        //if (isRotatingTowardsTarget)
        //    RotateVineTowardsTheTarget();


    }

    private void HandleStunnedState()
    {
        // Stunned for certain amount of time
        // Returns to protected when stun ends
        gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;

        if(isCovered)
        {
            Uncover();
            StartCoroutine(Stun());
        }
    }

    private void HandlePhaseTwoState()
    {
        // Platforms spawn
        // When player hits the trigger, transition to escape sequence
        // Just big wall rising, player escaping
        // Trigger in the other end, closes the big climb room and thrid phasse begins

        if(!phaseTwoActionsInitiated)
        {
            Cover();
            escapePlatform.SetActive(true);


        }

    }

    private void HandleChargeState()
    {
        // Visual something to indicate that charge is coming
        // Locks to position after a while and jumps in straight line there
        // Attached to the point it was jumped
    }

    private void HandleSeedShootState()
    {
        // Goes to CENTER of room
        // Shoot MANY seed to everywhere
    }

    private void HandleArtilleryState()
    {
        // Time when player is supposed to hit the plant
        // Shoots different seed into air which fall slowly towards the ground
    }

    private void HandleEvilLaughState()
    {
        // 4head
    }

    private void HandleLightAttackState()
    {
        // Player goes too close to the boss during any other state than artillery
        // Hits the player with vines
    }

    private void HandlePlayerIsDeadState()
    {
        // Dead player :)
    }

    private void Cover()
    {
        spikyVine.transform.DOMove(spikyVineEndPosition, 1);
        isCovered = true;
    }

    private void Uncover()
    {
        spikyVine.transform.DOMove(spikyVineStartPosition, 1);
        isCovered = false;
    }

    private void SpawnEnemies()
    {
        isEnemiesSpawned = true;
        //if(spawnWorkerDrones)
        //{
        //    workerDroneOneInstance = Instantiate(workerDroneOnePrefab, new Vector2(transform.position.x - 5, transform.position.y + 15), Quaternion.identity);
        //    workerDroneTwoInstance = Instantiate(workerDroneTwoPrefab, new Vector2(transform.position.x + 5, transform.position.y + 15), Quaternion.identity);
        //    workerDroneOneInstance.GetComponent<GroundEnemyAI>().target = target.transform;
        //    workerDroneTwoInstance.GetComponent<GroundEnemyAI>().target = target.transform;
        //    workerDroneOneInstance.GetComponent<GroundEnemyAI>().bossMode = true;
        //    workerDroneTwoInstance.GetComponent<GroundEnemyAI>().bossMode = true;
        //    spawnWorkerDrones = false;
        //}
        //else
        //{
        //    flyingDroneOneInstance = Instantiate(flyingDroneOnePrefab, new Vector2(transform.position.x - 5, transform.position.y + 15), Quaternion.identity);
        //    flyingDroneTwoInstance = Instantiate(flyingDroneTwoPrefab, new Vector2(transform.position.x + 5, transform.position.y + 15), Quaternion.identity);
        //    flyingDroneOneInstance.GetComponent<FlyingEnemyAI>().target = target.transform;
        //    flyingDroneTwoInstance.GetComponent<FlyingEnemyAI>().target = target.transform;
        //    flyingDroneOneInstance.GetComponent<FlyingEnemyAI>().bossMode = true;
        //    flyingDroneTwoInstance.GetComponent<FlyingEnemyAI>().bossMode = true;
        //    spawnWorkerDrones = true;
        //}

    }

    //private void RotateVineTowardsTheTarget()
    //{
    //    // Rotating object to point player
    //    Vector3 vectorToTarget = target.transform.position - attackVine.transform.position;
    //    float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
    //    Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    //    attackVine.transform.rotation = Quaternion.Slerp(attackVine.transform.rotation, q, Time.deltaTime);
    //}

    private void ChangeToBossLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("Boss");
    }

    private void ChangeToDefaultLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.gameObject.name == "AttackVine")
        //{
        //    Debug.Log("OUCH!");
        //    state = PlantState.Stunned;
        //}

        if (collision.collider.gameObject.name == "Player" && state != PlantState.Stunned)
        {
            PlayerPushback();
        }
    }

    // Cooldown for the player knockback.
    private IEnumerator KnockbackCooldown()
    {
        knockbackOnCooldown = true;
        yield return new WaitForSeconds(0.5f);
        knockbackOnCooldown = false;
    }

    // Pushbacks the player when hit with riot drone collider. Uses velocity for the knockback instead of force.
    public void PlayerPushback()
    {
        velocityPlayer = new Vector2(target.transform.position.x - transform.position.x > 0 ? knockbackForce * 1 : knockbackForce * -1, knockbackForce / 3);
        targetRB.MovePosition(targetRB.position + velocityPlayer * Time.deltaTime);
        StartCoroutine(KnockbackCooldown());
    }

    //private IEnumerator SpawnVine()
    //{
    //    vineSpeed = health.GetHealth() / health.GetMaxHealth();
    //    isAttackVineActivated = true;
    //    attackVine.transform.position = new Vector2(Random.Range(transform.position.x - 5, transform.position.x + 5), transform.position.y + 15);
    //    attackVine.transform.DOMoveY(attackVine.transform.position.y - 5, attackVineMoveDuration * vineSpeed);
    //    yield return new WaitForSeconds(attackVineRotateDuration * vineSpeed);
    //    attackVine.GetComponent<SpriteRenderer>().color = Color.red;
    //    isRotatingTowardsTarget = false;
    //    Vector2 attackDirection = target.transform.position;
    //    yield return new WaitForSeconds(attackVineWaitTime * vineSpeed);
    //    attackVine.transform.DOScaleY(attackVineStretchAmount, attackVineStretchDuration * vineSpeed);
    //    yield return new WaitForSeconds(attackVineStretchDuration * vineSpeed);
    //    attackVine.transform.DOScaleY(1, attackVineStretchDuration * vineSpeed);
    //    yield return new WaitForSeconds(attackVineStretchDuration * vineSpeed);
    //    attackVine.GetComponent<SpriteRenderer>().color = Color.white;
    //    attackVine.transform.DOMoveY(attackVine.transform.position.y + 5, attackVineMoveDuration * vineSpeed);

    //    yield return new WaitForSeconds(attackVineMoveDuration * vineSpeed);
    //    isAttackVineActivated = false;
    //}

    private IEnumerator SpawnVine()
    {
        int spawnAmount = ((int)health.GetMaxHealth() - (int)health.GetHealth()) / 3;
        isAttackVineActivated = true;
        vineSpeed = health.GetHealth() / health.GetMaxHealth();
        if(spawnAmount <= 1)
        {
            attackVineInstances.Add(Instantiate(attackVine, new Vector2(Random.Range(transform.position.x - 5, transform.position.x + 5), transform.position.y + 15), new Quaternion(0, 0, -180, 0)));
        }
        else
        {
            for (int i = 0; i < spawnAmount; i++)
            {
                attackVineInstances.Add(Instantiate(attackVine, new Vector2(Random.Range(transform.position.x - 5, transform.position.x + 5), transform.position.y + 15), new Quaternion(0, 0, -180, 0)));
                yield return new WaitForSeconds(vineSpeed);
            }
        }

        yield return new WaitForSeconds(2);
        isAttackVineActivated = false;
        
    }

    private IEnumerator Roar()
    {
        isRoaring = true;
        Debug.Log("roar!");
        yield return new WaitForSeconds(2);
        state = PlantState.Protected;
        isRoaring = false;
    }

    private IEnumerator Stun()
    {
        ChangeToBossLayer();
        yield return new WaitForSeconds(stunTime);
        ChangeToDefaultLayer();
        state = PlantState.Protected;
    }

    public enum PlantState
    {
        Idle,
        Protected,
        Angri,
        Stunned,
        PhaseTwo,
        Charge,
        SeedShoot,
        Artillery,
        EvilLaugh,
        LightAttack,
        PlayerIsDead

    }

    public float GetVineSpeed()
    {
        return vineSpeed;
    }
}
