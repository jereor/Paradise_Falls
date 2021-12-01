using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BigPlantController : MonoBehaviour
{
    [SerializeField] private bool skipToSecondPhase = false;
    [SerializeField] private bool skipSmallEnemies = false;
    private GameObject target;
    private Rigidbody2D targetRB;
    private GameObject spikyVine; // For visual purposes only.
    public Health health;
    private Rigidbody2D rb;

    [SerializeField] private GameObject hiddenBossObjects;

    [SerializeField] private Transform bossTeleportPoint;

    private Health targetHealth;

    private SpikyDeathWallController deathWallController; // Phase 2 rising death wall.
    private PhaseTwoObjectActivator phaseTwoObjectActivator;

    [SerializeField] private GameObject workerDroneOnePrefab; // Prefabs for spawnable enemies.
    [SerializeField] private GameObject workerDroneTwoPrefab;
    private GameObject workerDroneOneInstance;
    private GameObject workerDroneTwoInstance;

    [SerializeField] private GameObject flyingDroneOnePrefab;
    [SerializeField] private GameObject flyingDroneTwoPrefab;
    private GameObject flyingDroneOneInstance;
    private GameObject flyingDroneTwoInstance;

    [SerializeField] private GameObject attackVine; // Prefab that is instantiated in Angri state.
    [SerializeField] private GameObject grappleVine; // Prefab that is instantiated in Charge state.
    public List<GameObject> attackVineInstances; // All spawned vines are stored in a list.
    public List<GameObject> grappleVineInstances; // All spawned vines are stored in a list.

    [Header("Current State")]
    public PlantState state = PlantState.Idle;

    private Vector2 spikyVineStartPosition; // Vectors for the SpikyVine start and end positions.
    private Vector2 spikyVineEndPosition;

    private Vector2 velocityPlayer; // Player velocity for knockback.

    [SerializeField] private float stunTime; // time the boss is stunned.
    [SerializeField] private float knockbackForce; // Knockback force for player pushback.
    public float vineSpeed = 1; // Default is 1. Scales accordingly when boss takes damage.


    private bool isCovered = false;
    private bool isRoaring = false;
    private bool isEnemiesSpawned = false;
    private bool knockbackOnCooldown = false;
    private bool spawnWorkerDrones = true;
    private bool isPhaseTwoTransitioning = false;
    private bool isPhaseTwoInitiated = false;
    private bool isPhaseThreeTransitioning = false;
    private bool isCharging = false;

    void Start()
    {
        attackVineInstances = new List<GameObject>();
        grappleVineInstances = new List<GameObject>();
        target = GameObject.Find("Player");
        targetRB = target.GetComponent<Rigidbody2D>();
        spikyVine = GameObject.Find("SpikyVine");
        spikyVineStartPosition = spikyVine.transform.position;
        spikyVineEndPosition = new Vector2(spikyVineStartPosition.x, transform.position.y);
        deathWallController = GameObject.Find("SpikyDeathWall").GetComponent<SpikyDeathWallController>();
        phaseTwoObjectActivator = GameObject.Find("PhaseTwoObjectActivator").GetComponent<PhaseTwoObjectActivator>();
        health = GetComponent<Health>();
        targetHealth = target.GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        if (skipToSecondPhase)
        {
            health.TakeDamage(health.GetMaxHealth() / 2 - 1);
        }
    }

    // Update is called once per frame
    void Update()
    {

        // If bosses health goes down more that 50%, change phase.
        if ((health.GetHealth() <= health.GetMaxHealth() * 0.5f && !isPhaseTwoInitiated) || (skipToSecondPhase && !isPhaseTwoInitiated))
        {
            state = PlantState.PhaseTwo;
            ChangeToDefaultLayer();
            isPhaseTwoInitiated = true;
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

        if (!isCovered) // Checks if the boss is covered (visual only).
            Cover();

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
        }

        if(deathWallController.GetPlayerSurvived() && !isPhaseThreeTransitioning)
        {
            StartCoroutine(PhaseThreeTransition());
        }


    }

    private void HandleChargeState()
    {
        // Visual something to indicate that charge is coming
        // Locks to position after a while and jumps in straight line there
        // Attached to the point it was jumped
        if(!isCharging)
        {
            StartCoroutine(Charge());
        }
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
        gameObject.GetComponent<CircleCollider2D>().radius = 2f;
        spikyVine.transform.DOMove(spikyVineEndPosition, 1);
        isCovered = true;
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
                workerDroneOneInstance = Instantiate(workerDroneOnePrefab, new Vector2(transform.position.x - 5, transform.position.y + 15), Quaternion.identity);
                workerDroneTwoInstance = Instantiate(workerDroneTwoPrefab, new Vector2(transform.position.x + 5, transform.position.y + 15), Quaternion.identity);
                workerDroneOneInstance.GetComponent<GroundEnemyAI>().target = target.transform;
                workerDroneTwoInstance.GetComponent<GroundEnemyAI>().target = target.transform;
                workerDroneOneInstance.GetComponent<GroundEnemyAI>().bossMode = true;
                workerDroneTwoInstance.GetComponent<GroundEnemyAI>().bossMode = true;
                spawnWorkerDrones = false;
            }
            else
            {
                flyingDroneOneInstance = Instantiate(flyingDroneOnePrefab, new Vector2(transform.position.x - 5, transform.position.y + 15), Quaternion.identity);
                flyingDroneTwoInstance = Instantiate(flyingDroneTwoPrefab, new Vector2(transform.position.x + 5, transform.position.y + 15), Quaternion.identity);
                flyingDroneOneInstance.GetComponent<FlyingEnemyAI>().target = target.transform;
                flyingDroneTwoInstance.GetComponent<FlyingEnemyAI>().target = target.transform;
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

        if (collision.collider.gameObject.name == "Player" && state != PlantState.Stunned && !knockbackOnCooldown)
        {
            PlayerPushback();
        }

        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
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


    // Coroutine spawns vines with a simple calculation that uses the remaining and maximum boss health as a vine spawn amount.
    private IEnumerator SpawnVine()
    {
        int spawnAmount = ((int)health.GetMaxHealth() - (int)health.GetHealth()) / 3;
        vineSpeed = health.GetHealth() / health.GetMaxHealth();

        // The operation would normally result in 0 as it is rounded to integer. In this case spawn only one vine.
        if (spawnAmount <= 1)
        {
            attackVineInstances.Add(Instantiate(attackVine, new Vector2(Random.Range(transform.position.x - 10, transform.position.x + 10), transform.position.y + 18), new Quaternion(0, 0, -180, 0)));
        }
        else
        {
            for (int i = 0; i < spawnAmount; i++)
            {
                attackVineInstances.Add(Instantiate(attackVine, new Vector2(Random.Range(transform.position.x - 10, transform.position.x + 10), transform.position.y + 18), new Quaternion(0, 0, -180, 0)));
                yield return new WaitForSeconds(vineSpeed);
            }
        }
        yield return new WaitForSeconds(2);     
    }

    private IEnumerator PhaseTwoTransition()
    {
        isPhaseTwoTransitioning = true;
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
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

    private IEnumerator Charge()
    {
        isCharging = true;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        for(int i = 0; i < 4; i++)
        {
            grappleVineInstances.Add(Instantiate(grappleVine, new Vector2(Random.Range(transform.position.x - 1, transform.position.x + 1), Random.Range(transform.position.y - 1, transform.position.y + 1)), Quaternion.identity));
            grappleVineInstances[i].transform.SetParent(transform);
        }
        yield return new WaitForSeconds(1);
    }

    private IEnumerator ShootTheSeeds()
    {
        for(int i = 0; i < 10; i++)
        {

        }
        yield return new WaitForSeconds(0);
    }

    private IEnumerator Roar()
    {
        isRoaring = true;
        Debug.Log("roar!");
        yield return new WaitForSeconds(2);
        state = PlantState.Protected;
        isRoaring = false;
    }

    // Boss layer is changed for a certain amount of time. Hit the boss hard.
    // Returns to protected state after.
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

    // Vinespeed is needed in AttackVineController script.
    public float GetVineSpeed()
    {
        return vineSpeed;
    }

    public void SetIsCharging(bool b)
    {
        isCharging = b;
    }

    public bool GetisCharging()
    {
        return isCharging;
    }
}
