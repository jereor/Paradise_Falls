using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;


// TODO: Collider to trigger
// light attack state
// player can damage boss in every state

public class BigPlantController : MonoBehaviour
{
    [SerializeField] private bool skipToSecondPhase = false;
    [SerializeField] private bool skipSmallEnemies = false;
    private GameObject target;
    private Rigidbody2D targetRB;
    private GameObject spikyVine; // For visual purposes only.
    public Health health;
    private Rigidbody2D rb;
    private CircleCollider2D bossCollider;

    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private GameObject hiddenBossObjects;

    [SerializeField] private Transform bossTeleportPoint;
    [SerializeField] private Transform seedShootPosition;

    [SerializeField] private List<Vector2> seedSpawnPositions;

    private Health targetHealth;

    private SpikyDeathWallController deathWallController; // Phase 2 rising death wall.
    private PhaseTwoObjectActivator phaseTwoObjectActivator;

    [SerializeField] private GameObject workerDronePrefab; // Prefabs for spawnable enemies.
    private GameObject workerDroneOneInstance;
    private GameObject workerDroneTwoInstance;

    [SerializeField] private GameObject flyingDronePrefab;
    private GameObject flyingDroneOneInstance;
    private GameObject flyingDroneTwoInstance;

    [SerializeField] private GameObject attackVine; // Prefab that is instantiated in Angri state.
    [SerializeField] private GameObject grappleVine; // Prefab that is instantiated in Charge state.
    [SerializeField] private GameObject seedVine; // Prefab that is instantiated in Charge state.
    [SerializeField] private GameObject swingVine; // Prefab that is instantiated in LightAttack function.
    [SerializeField] private GameObject specialVine; // Prefab that is instantiated in LightAttack function.
    public List<GameObject> attackVineInstances; // All spawned vines are stored in a list.
    public List<GameObject> grappleVineInstances; // All spawned vines are stored in a list.
    private GameObject swingVineInstance;

    [SerializeField] private GameObject plantSeed;
    [SerializeField] private GameObject slowSeed;

    [Header("Bullet Hell variables")]
    [SerializeField] private float seedGap;
    [SerializeField] private float seedFrequency;
    [SerializeField] private float seedSpeed;
    [SerializeField] private float seedAmount;

    Quaternion leftVineRotation;
    Quaternion rightVineRotation;


    [Header("Current State")]
    public PlantState state = PlantState.Idle;

    private Vector2 spikyVineStartPosition; // Vectors for the SpikyVine start and end positions.
    private Vector2 spikyVineEndPosition;

    private Vector2 velocityPlayer; // Player velocity for knockback.

    [SerializeField] private float stunTime; // time the boss is stunned.
    [SerializeField] private float knockbackForce; // Knockback force for player pushback.
    [SerializeField] private float transportingToMiddleTime; // Time to prepare for seedshoot.
    [SerializeField] private float artilleryWobbleTime; // how long the Tweenings and yield returns will last in Artiller state
    public float vineSpeed = 1; // Default is 1. Scales accordingly when boss takes damage.
    public float speedMultiplier = 1; // Default is 1. Scales accordingly when boss takes damage.
    private float healthAtPhaseThreeTransition; // Used to calculate the phase three speed multiplier.
    private int seedCount = 1; // Used in a function to scale the seed amount with the boss health. Less health = more seeds.
    [SerializeField] private int chargeProbability = 1;
    private int timeToArtillery = 0;
    [SerializeField] private float knockbackCooldown = 0.5f;
    [SerializeField] private float lightAttackCircleRadius = 2;
    [SerializeField] private float lightAttackCooldown = 2;

    private float tempHealth = 0;

    [SerializeField] private int vineSpawnCounter = 0;


    private bool isCovered = false;
    private bool isRoaring = false;
    private bool isEnemiesSpawned = false;
    private bool knockbackOnCooldown = false;
    private bool spawnWorkerDrones = true;
    private bool isAttackVineSpawned = false;
    private bool isPhaseTwoTransitioning = false;
    private bool isPhaseTwoInitiated = false;
    private bool isPhaseThreeTransitioning = false;
    private bool isCharging = false;
    private bool hasCharged = false;
    private bool isSetUppingForSeedShoot = false;
    private bool isSeedShooting = false;
    private bool isArtillerying = false;
    private bool hasArtilleryed = false;
    private bool isDead = false;
    private bool lightAttackOnCooldown = false;
    private bool isPhaseThree = false;

    public UnityEvent firstPhaseMusic;
    public UnityEvent climbPhaseMusic;
    public UnityEvent lastPhaseMusic;
    public UnityEvent victoryMusic;


    void Start()
    {
        seedSpawnPositions = new List<Vector2>();
        attackVineInstances = new List<GameObject>();
        grappleVineInstances = new List<GameObject>();
        target = GameObject.Find("Player");
        targetRB = target.GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<CircleCollider2D>();
        spikyVine = GameObject.Find("SpikyVine");
        spikyVineStartPosition = spikyVine.transform.position;
        spikyVineEndPosition = new Vector2(spikyVineStartPosition.x, transform.position.y);
        deathWallController = GameObject.Find("SpikyDeathWall").GetComponent<SpikyDeathWallController>();
        phaseTwoObjectActivator = GameObject.Find("PhaseTwoObjectActivator").GetComponent<PhaseTwoObjectActivator>();
        health = GetComponent<Health>();
        targetHealth = target.GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        //if (skipToSecondPhase)
        //{
        //    health.TakeDamage(health.GetMaxHealth() / 2 - 1);
        //}
        rightVineRotation.eulerAngles = new Vector3(0, 0, 90);
        leftVineRotation.eulerAngles = new Vector3(0, 0, -90);
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            HandlePlayerIsDeadState();
            return;
        }

        if(state == PlantState.Stunned && health.GetHealth() <= tempHealth - 8f && health.GetHealth() > health.GetMaxHealth() * 0.5f)
        {
            tempHealth = 0;
            StopCoroutine(Stun());
            ChangeToDefaultLayer();
            state = PlantState.Protected;
        }


        if(health.GetHealth() <= 0 && !isDead)
        {
            StopAllCoroutines();
            DOTween.PauseAll();
            if(grappleVineInstances != null)
            {
                for (int i = 0; i < grappleVineInstances.Count; i++)
                {
                    Destroy(grappleVineInstances[i]);
                }
            }


            //Destroy(GameObject.Find("SlowSeed"));
            var obj = GameObject.FindGameObjectsWithTag("EnemyProjectile");
            if(obj != null)
            {
                foreach (GameObject o in obj)
                {
                    Destroy(o.gameObject);
                }
            }

            state = PlantState.Die;
        }
        // If bosses health goes down more that 50%, change phase.
        if ((health.GetHealth() <= health.GetMaxHealth() * 0.5f && !isPhaseTwoInitiated) || (skipToSecondPhase && !isPhaseTwoInitiated))
        {
            //health.TakeDamage(health.GetMaxHealth() - 1);
            state = PlantState.PhaseTwo;
            ChangeToDefaultLayer();
            isPhaseTwoInitiated = true;
        }

        if (timeToArtillery >= 5)
        {
            state = PlantState.Artillery;
            timeToArtillery = 0;
        }

        if(isPhaseThree && IsTargetInHitRange() && state != PlantState.Artillery && state != PlantState.Die && !lightAttackOnCooldown)
        {
            Debug.Log("Attacking!");
            StartCoroutine(LightAttack());
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

            case PlantState.Die:
                HandleDie();
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

        if (!isCovered) // Checks if the boss is covered (visual only).
            StartCoroutine(Cover());

        if (!isEnemiesSpawned) // If enemies are not spawned, spawn some.
            SpawnEnemies();

        // If the small enemy instances are NULL, change state to Angri.
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

        // Script saves the spawned vines into a list. AttackVine script removes them one at a time when the behaviour is completed, leaving the list empty in the end.
        // Empty list means that boss can spawn the next set of vines. This prevents that no more vines spawn until all are destroyed by the corresponding instance.
        if (attackVineInstances.Count == 0)
        {
            StartCoroutine(SpawnVine());
        }


    }

    private void HandleStunnedState()
    {
        // Stunned for certain amount of time
        // Returns to protected when stun ends
        gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;

        // Uncovers the boss (visual only) and starts the coroutine for stun state.
        if(isCovered)
        {
            tempHealth = health.GetHealth();
            Uncover();
            StartCoroutine(Stun());
        }
    }

    private void HandlePhaseTwoState()
    {
        // Platforms spawn
        // When player hits the trigger, transition to escape sequence
        // Just big wall rising, player escaping
        // Trigger in the other end, closes the big climb room and third phase begins
        if(!isPhaseTwoTransitioning)
        {
            StartCoroutine(PhaseTwoTransition()); // Everything regarding the transition is dealt in the coroutine.
            climbPhaseMusic.Invoke();
        }

        if(deathWallController.GetPlayerSurvived() && !isPhaseThreeTransitioning)
        {
            //bossCollider.isTrigger = true;
            isPhaseThree = true;
            healthAtPhaseThreeTransition = health.GetHealth();
            phaseTwoObjectActivator.SpawnBoostPlants();
            ChangeToBossLayer();
            StartCoroutine(PhaseThreeTransition());
            lastPhaseMusic.Invoke();
        }


    }

    private void HandleChargeState()
    {
        // Visual something to indicate that charge is coming
        // Locks to position after a while and jumps in straight line there
        // Attached to the point it was jumped
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        if (hasCharged && grappleVineInstances.Count == 0)
        {
            int i = Random.Range(1, 11);
            if (i <= chargeProbability)
                state = PlantState.Charge;
            else
                state = PlantState.SeedShoot;
            hasCharged = false;
            isCharging = false;
            timeToArtillery++;
            return;
        }

        if(!isCharging && !hasCharged)
        {
            ChangeSpeedMultiplier();
            Charge();
        }
    }

    private void HandleSeedShootState()
    {
        // Goes to CENTER of room
        // Shoot MANY seed to everywhere
        gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        if(!isSetUppingForSeedShoot && !isSeedShooting)
        {
            SeedAmountValueChanger();
            ChangeSpeedMultiplier();
            StartCoroutine(ShootTheSeeds());
        }

        if(!isSetUppingForSeedShoot && isSeedShooting && grappleVineInstances.Count == 0)
        {
            isSeedShooting = false;
            state = PlantState.Charge;
            timeToArtillery++;
            return;
        }
    }

    private void HandleArtilleryState()
    {
        // Time when player is supposed to hit the plant
        // Shoots different seed into air which fall slowly towards the ground
        gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;

        if (!isArtillerying && !hasArtilleryed)
        {
            SeedAmountValueChanger();
            ChangeSpeedMultiplier();           
            StartCoroutine(Artillery());
        }

        if(hasArtilleryed)
        {
            hasArtilleryed = false;
            state = PlantState.Charge;
            return;
        }
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
        Debug.Log("Player died.");
        StopAllCoroutines();
        DOTween.PauseAll();
        if(grappleVineInstances.Count > 0)
        {
            for (int i = 0; i < grappleVineInstances.Count; i++)
            {
                Destroy(grappleVineInstances[i]);
            }
        }
    }

    private void HandleDie()
    {
        if(!isDead)
        {
            isDead = true;
            bossCollider.enabled = false;
            StartCoroutine(BossIsDead());
        }
    }

    private bool IsTargetInHitRange()
    {
        return Physics2D.OverlapCircle(transform.position, lightAttackCircleRadius, playerLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lightAttackCircleRadius);

    }

    private IEnumerator Cover()
    {
        isCovered = true;
        spikyVine.transform.DOMove(spikyVineEndPosition, 1);
        yield return new WaitForSeconds(1);
        gameObject.GetComponent<CircleCollider2D>().radius = 2f;
        
    }

    private void Uncover()
    {
        gameObject.GetComponent<CircleCollider2D>().radius = 1.2f;
        spikyVine.transform.DOMove(spikyVineStartPosition, 1);
        isCovered = false;
    }

    // Spawn the enemies corresponding the bool value. One set is worker drones, other flying drones.
    // Set the spawned enemies to boss mode and set the player to their target.
    private void SpawnEnemies()
    {
        isEnemiesSpawned = true;
        if(!skipSmallEnemies)
        {
            if (spawnWorkerDrones)
            {
                workerDroneOneInstance = Instantiate(workerDronePrefab, new Vector2(transform.position.x - 5, transform.position.y + 15), Quaternion.identity);
                workerDroneTwoInstance = Instantiate(workerDronePrefab, new Vector2(transform.position.x + 5, transform.position.y + 15), Quaternion.identity);
                //workerDroneOneInstance.GetComponent<GroundEnemyAI>().target = target.transform;
                //workerDroneTwoInstance.GetComponent<GroundEnemyAI>().target = target.transform;
                workerDroneOneInstance.GetComponent<GroundEnemyAI>().bossMode = true;
                workerDroneTwoInstance.GetComponent<GroundEnemyAI>().bossMode = true;
                spawnWorkerDrones = false;
            }
            else
            {
                flyingDroneOneInstance = Instantiate(flyingDronePrefab, new Vector2(transform.position.x - 5, transform.position.y + 15), Quaternion.identity);
                flyingDroneTwoInstance = Instantiate(flyingDronePrefab, new Vector2(transform.position.x + 5, transform.position.y + 15), Quaternion.identity);
                //flyingDroneOneInstance.GetComponent<FlyingEnemyAI>().target = target.tr.transform;
                //flyingDroneTwoInstance.GetComponent<FlyingEnemyAI>().target = target.transform;
                flyingDroneOneInstance.GetComponent<FlyingEnemyAI>().bossMode = true;
                flyingDroneTwoInstance.GetComponent<FlyingEnemyAI>().bossMode = true;
                spawnWorkerDrones = true;
            }
        }


    }

    // LAYER CHANGES
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

        if (collision.collider.gameObject.name == "Player" && !knockbackOnCooldown && state != PlantState.Stunned && state != PlantState.Artillery)
        {
            PlayerPushback();
            targetHealth.TakeDamage(1);
        }

        //if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //{
        //    rb.constraints = RigidbodyConstraints2D.FreezeAll;
        //}
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player" && !knockbackOnCooldown && state != PlantState.Stunned && state != PlantState.Artillery)
        {
            PlayerPushback();
            targetHealth.TakeDamage(1);
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.name == "Player" && !knockbackOnCooldown && state != PlantState.Stunned && state != PlantState.Artillery)
    //    {
    //        PlayerPushback();
    //        targetHealth.TakeDamage(1);
    //    }
    //}

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (collision.gameObject.name == "Player" && !knockbackOnCooldown && state != PlantState.Stunned && state != PlantState.Artillery)
    //    {
    //        PlayerPushback();
    //        targetHealth.TakeDamage(1);
    //    }
    //}

    // Cooldown for the player knockback.
    private IEnumerator KnockbackCooldown()
    {
        knockbackOnCooldown = true;
        yield return new WaitForSeconds(knockbackCooldown);
        knockbackOnCooldown = false;
    }

    private void ChangeSpeedMultiplier()
    {
        float variable = 1 - health.GetHealth() / healthAtPhaseThreeTransition;
        speedMultiplier = 1 - variable / 2;
    }

    private void SeedAmountValueChanger()
    {
        if (health.GetHealth() / healthAtPhaseThreeTransition < 1)
            seedCount = 1;
        if (health.GetHealth() / healthAtPhaseThreeTransition < 0.8)
            seedCount = 2;
        if (health.GetHealth() / healthAtPhaseThreeTransition < 0.6)
            seedCount = 3;
        if (health.GetHealth() / healthAtPhaseThreeTransition < 0.4)
            seedCount = 4;

    }

    // Pushbacks the player when hit with riot drone collider. Uses velocity for the knockback instead of force.
    public void PlayerPushback()
    {
        velocityPlayer = new Vector2(target.transform.position.x - transform.position.x > 0 ? knockbackForce * 1 : knockbackForce * -1, knockbackForce / 3);
        targetRB.MovePosition(targetRB.position + velocityPlayer * Time.deltaTime);
        StartCoroutine(KnockbackCooldown());
    }

    private IEnumerator LightAttack()
    {
        lightAttackOnCooldown = true;
        swingVineInstance = Instantiate(swingVine, transform.position, Quaternion.identity);
        swingVineInstance.transform.SetParent(transform);
        yield return new WaitForSeconds(lightAttackCooldown);
        lightAttackOnCooldown = false;
    }


    // Coroutine spawns vines with a simple calculation that uses the remaining and maximum boss health as a vine spawn amount.
    private IEnumerator SpawnVine()
    {
        int spawnAmount = ((int)health.GetMaxHealth() - (int)health.GetHealth()) / 4;
        vineSpeed = health.GetHealth() / health.GetMaxHealth();

        // The operation would normally result in 0 as it is rounded to integer. In this case spawn only one vine.
        if(vineSpawnCounter >= 5)
        {
            attackVineInstances.Add(Instantiate(specialVine, new Vector2(Random.Range(transform.position.x - 10, transform.position.x + 10), transform.position.y + 18), new Quaternion(0, 0, -180, 0)));
            isAttackVineSpawned = true;
        }
        else if (spawnAmount <= 1 && vineSpawnCounter < 5)
        {
            attackVineInstances.Add(Instantiate(attackVine, new Vector2(Random.Range(transform.position.x - 10, transform.position.x + 10), transform.position.y + 18), new Quaternion(0, 0, -180, 0)));
            vineSpawnCounter++;
        }
        else if(vineSpawnCounter < 5)
        {
            for (int i = 0; i < spawnAmount; i++)
            {
                attackVineInstances.Add(Instantiate(attackVine, new Vector2(Random.Range(transform.position.x - 10, transform.position.x + 10), transform.position.y + 18), new Quaternion(0, 0, -180, 0)));
                yield return new WaitForSeconds(vineSpeed);
            }
            vineSpawnCounter++;
        }
        if(isAttackVineSpawned)
        {
            vineSpawnCounter = 0;
            isAttackVineSpawned = false;
        }
            

        yield return new WaitForSeconds(2);
    }

    private IEnumerator PhaseTwoTransition()
    {
        isPhaseTwoTransitioning = true;
        hiddenBossObjects.SetActive(true);
        // Simple pulsing effect for boss local scale.
        for (int i = 0; i < 3; i++)
        {
            transform.DOScale(1.2f, 0.3f);
            yield return new WaitForSeconds(0.3f);
            transform.DOScale(1f, 0.3f);
            yield return new WaitForSeconds(0.3f);
        }
        // Camera shakiiiiing
        // -----HERE----
        PlayerCamera.Instance.CameraShake(1.5f, 4);
        // Moves the boss into the ground and grows the local scale bigger during that.
        transform.DOScale(1.5f, 2f);
        transform.DOMove(new Vector2(transform.position.x, transform.position.y - 5), 3);
        yield return new WaitForSeconds(2);

        // Activate platforms to let the player out of the pit.
        GameObject.Find("PhaseTwoObjectActivator").GetComponent<PhaseTwoObjectActivator>().SetSpawnPlatforms(true);

        // Death wall is rising. Better hurry up.
        deathWallController.enabled = true;
        // Activate all triggers and platforms for the escape sequence.
        phaseTwoObjectActivator.SpawnAllEscapeObjects();

    }

    private IEnumerator PhaseThreeTransition()
    {
        isPhaseThreeTransitioning = true;
        target.GetComponent<Player>().HandleAllPlayerControlInputs(false);
        transform.position = bossTeleportPoint.position;
        PlayerCamera.Instance.CameraShake(1.5f, 5);
        transform.DOMoveY(transform.position.y + 5, 3);
        yield return new WaitForSeconds(5);
        Debug.Log("MURRRR!");
        PlayerCamera.Instance.CameraShake(1f, 3);
        // Simple pulsing effect for boss local scale.
        for (int i = 0; i < 3; i++)
        {
            transform.DOScale(1.8f, 0.3f);
            yield return new WaitForSeconds(0.3f);
            transform.DOScale(1.5f, 0.3f);
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(2);
        target.GetComponent<Player>().HandleAllPlayerControlInputs(true);
        state = PlantState.Charge;
        deathWallController.SetForThirdPhase();
    }

    private void Charge()
    {
        isCharging = true;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        for(int i = 0; i < 4; i++)
        {
            grappleVineInstances.Add(Instantiate(grappleVine, new Vector2(Random.Range(transform.position.x - 1, transform.position.x + 1), Random.Range(transform.position.y - 1, transform.position.y + 1)), Quaternion.identity));
            grappleVineInstances[i].transform.SetParent(transform);
        }
    }

    private IEnumerator Artillery()
    {
        isArtillerying = true;
        int a = Random.Range(0, 2);
        if(a == 0)
            transform.DOJump(seedShootPosition.GetChild(0).position, 0, 0, transportingToMiddleTime * 2 * speedMultiplier);
        else
            transform.DOJump(seedShootPosition.GetChild(1).position, 0, 0, transportingToMiddleTime * 2 * speedMultiplier);

        yield return new WaitForSeconds(transportingToMiddleTime * 2 * speedMultiplier);
        //int amount = seedCount;

        for (int  i = 0; i < 5; i++)
        {
            for(int e = 0; e < seedCount; e++)
                Instantiate(slowSeed, new Vector2(Random.Range(seedShootPosition.GetChild(2).position.x - 12, seedShootPosition.GetChild(2).position.x + 12), seedShootPosition.GetChild(2).position.y), Quaternion.identity);
            
            transform.DOScaleX(1.5f, artilleryWobbleTime * speedMultiplier);
            transform.DOScaleY(1.3f, artilleryWobbleTime * speedMultiplier);
            yield return new WaitForSeconds(artilleryWobbleTime * speedMultiplier);
            for (int u = 0; u < seedCount; u++)
                Instantiate(slowSeed, new Vector2(Random.Range(seedShootPosition.GetChild(2).position.x - 12, seedShootPosition.GetChild(2).position.x + 12), seedShootPosition.GetChild(2).position.y), Quaternion.identity);
            
            transform.DOScaleX(1.3f, artilleryWobbleTime * speedMultiplier);
            transform.DOScaleY(1.5f, artilleryWobbleTime * speedMultiplier);
            yield return new WaitForSeconds(artilleryWobbleTime * speedMultiplier);
        }
        for (int d = 0; d < seedCount; d++)
            Instantiate(slowSeed, new Vector2(Random.Range(seedShootPosition.GetChild(2).position.x - 12, seedShootPosition.GetChild(2).position.x + 12), seedShootPosition.GetChild(2).position.y), Quaternion.identity);
        
        transform.DOScaleX(1.5f, artilleryWobbleTime * speedMultiplier);
        yield return new WaitForSeconds(artilleryWobbleTime * speedMultiplier);


        isArtillerying = false;
        hasArtilleryed = true;
    }


    private IEnumerator ShootTheSeeds()
    {
        isSeedShooting = true;
        isSetUppingForSeedShoot = true;
        transform.DOJump(seedShootPosition.position, 0, 0, transportingToMiddleTime * speedMultiplier);
        yield return new WaitForSeconds(transportingToMiddleTime * speedMultiplier);
        for (int i = 0; i < 2; i++)
        {
            grappleVineInstances.Add(Instantiate(seedVine, new Vector2(Random.Range(transform.position.x - 1, transform.position.x + 1), Random.Range(transform.position.y - 1, transform.position.y + 1)), rightVineRotation));
            grappleVineInstances[i].transform.SetParent(transform);
        }
        for (int i = 2; i < 4; i++)
        {
            grappleVineInstances.Add(Instantiate(seedVine, new Vector2(Random.Range(transform.position.x - 1, transform.position.x + 1), Random.Range(transform.position.y - 1, transform.position.y + 1)), leftVineRotation));
            grappleVineInstances[i].transform.SetParent(transform);
        }

        yield return new WaitForSeconds(2);

        float amount = 1;
        //float positionModifierY = 1;
        //float positionModifierX = 0;
        for (int i = 0; i < 10; i++)
        {
            float positionModifierY = Random.Range(-1f, 1f);
            float positionModifierX = Random.Range(-1f, 1f);
            Instantiate(plantSeed, new Vector2(transform.position.x + Random.Range(0f, 1f), transform.position.y + Random.Range(0f, 1f)), Quaternion.identity);
            Instantiate(plantSeed, new Vector2(transform.position.x - Random.Range(0f, 1f), transform.position.y - Random.Range(0f, 1f)), Quaternion.identity);
            Instantiate(plantSeed, new Vector2(transform.position.x + Random.Range(0f, 1f), transform.position.y - Random.Range(0f, 1f)), Quaternion.identity);
            Instantiate(plantSeed, new Vector2(transform.position.x - Random.Range(0f, 1f), transform.position.y + Random.Range(0f, 1f)), Quaternion.identity);
            if(i == 9 && seedCount * seedAmount > amount)
            {
                amount++;
                i = -1;
                //positionModifierX = 0;
                //positionModifierY = 1;
            }
            else
            {
                //positionModifierX += seedGap;
                //positionModifierY -= seedGap;
            }

            yield return new WaitForSeconds(seedFrequency * speedMultiplier);
        }
        yield return new WaitForSeconds(2);
        isSetUppingForSeedShoot = false;
        yield return new WaitForSeconds(2);
    }

    private IEnumerator BossIsDead()
    {
        transform.DOJump(seedShootPosition.position, 0, 0, transportingToMiddleTime * 2);
        yield return new WaitForSeconds(transportingToMiddleTime * 2);
        PlayerCamera.Instance.CameraShake(0.7f, 5);
        for (int i = 0; i < 5; i++)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
            transform.DOScaleX(1.5f, (artilleryWobbleTime * speedMultiplier) / 2);
            transform.DOScaleY(1.3f, (artilleryWobbleTime * speedMultiplier) / 2);
            yield return new WaitForSeconds((artilleryWobbleTime * speedMultiplier) / 2);
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            transform.DOScaleX(1.3f, (artilleryWobbleTime * speedMultiplier) / 2);
            transform.DOScaleY(1.5f, (artilleryWobbleTime * speedMultiplier) / 2);
            yield return new WaitForSeconds((artilleryWobbleTime * speedMultiplier) / 2);
            gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            yield return new WaitForSeconds((artilleryWobbleTime * speedMultiplier) / 2);
        }
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        transform.DOScale(2.5f, 1);
        Color tmp = transform.GetComponent<SpriteRenderer>().color;
        float endValue = 0;
        float duration = 1;
        float elapsedTime = 0;
        float startValue = GetComponent<SpriteRenderer>().color.a;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r, GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b, newAlpha);
            yield return null;
        }
        yield return new WaitForSeconds(1);
        phaseTwoObjectActivator.OpenDoors();
        phaseTwoObjectActivator.DeactivateBoostPlants();

        victoryMusic.Invoke();

        Destroy(gameObject);
    }

    private IEnumerator Roar()
    {
        isRoaring = true;
        Debug.Log("roar!");
        yield return new WaitForSeconds(2);
        state = PlantState.Protected;
        isRoaring = false;
        firstPhaseMusic.Invoke();
    }

    // Boss layer is changed for a certain amount of time. Hit the boss hard.
    // Returns to protected state after.
    private IEnumerator Stun()
    {
        ChangeToBossLayer();
        yield return new WaitForSeconds(stunTime);
        ChangeToDefaultLayer();
        if(health.GetHealth() > health.GetMaxHealth() * 0.5f)
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
        Die,
    }

    // Vinespeed is needed in AttackVineController script.
    public float GetVineSpeed()
    {
        return vineSpeed;
    }

    public float GetSpeedMultiplier()
    {
        return speedMultiplier;
    }

    public void SetIsCharging(bool b)
    {
        isCharging = b;
    }

    public bool GetisCharging()
    {
        return isCharging;
    }

    public void SetHasCharged(bool b)
    {
        hasCharged = b;
    }

    public bool GetIsSettingUpForSeedShoot()
    {
        return isSetUppingForSeedShoot;
    }

    public bool GetKockbackOnCooldown()
    {
        return knockbackOnCooldown;
    }
}
