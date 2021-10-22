using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RiotControlDrone : MonoBehaviour
{
    [SerializeField] private GameObject taserBeam;
    [SerializeField] private GameObject seed;
    [SerializeField] private BoxCollider2D bodyCollider;
    [SerializeField] private BoxCollider2D headCollider;
    [SerializeField] private BoxCollider2D phaseTwoKnockbackBox;
    [SerializeField] private Transform[] colliders;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private GameObject doorOne;
    [SerializeField] private GameObject[] bossEnemies;

    [Header("Current State")]
    public RiotState state = RiotState.Idle;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Movement and Detection Areas")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float chargeSpeed;
    [SerializeField] private bool isEnraged = false;
    [SerializeField] private float walkStepInterval;
    [SerializeField] private float runStepInterval;
    [SerializeField] private float chargeStepInterval;
    [SerializeField] private float stunTime;
    [SerializeField] private float chargeReadyTime;
    [SerializeField] private float timesToBackstep;
    [SerializeField] private Vector2 areaToCharge;
    [SerializeField] private Vector2 chargeOffset;
    [SerializeField] private Vector2 areaToAttack;
    [SerializeField] private Vector2 hitOffset;
    [SerializeField] private Vector2 areaToDash;
    [SerializeField] private Vector2 dashOffset;
    [SerializeField] private Vector2 velocity;
    [SerializeField] private Vector2 velocityPlayer;
    

    [Header("AttackPower and Hit Cooldowns")]
    [SerializeField] private float lightAttackCoolDown;
    [SerializeField] private float lightAttackDamage;
    [SerializeField] private float heavyAttackChargeTime;
    [SerializeField] private float heavyAttackCoolDown;
    [SerializeField] private float heavyAttackDamage;
    [SerializeField] private float shieldSquishDamage;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float dashForceCount; // Amount of times boss uses force in DashAttack. NOT FORCE VALUE!
    [SerializeField] private float taserCooldown;
    [SerializeField] private float seedShootCooldown;
    [SerializeField] private float dashAttackCooldown;

    [Header("Values between 1-100")]
    [SerializeField] private int taserChance; // Value between 1-100.
    [SerializeField] private int seedShootChance; // Value between 1-100.

    [Header("Randomizer values")]
    [SerializeField] private int taserChanceRandomizer = 1;
    [SerializeField] private int seedShootChanceRandomizer = 1;

    private Transform target;
    private Health targetHealth;

    private Rigidbody2D rb;
    private Health health;
    
    [SerializeField] private bool isFacingRight = false;
    private Vector2 vectorToTarget;

    private bool canStart = false;
    private bool canMove = true;
    private bool canAttack = true;
    private bool canDashAttack = true;
    private bool canChargeToTarget = false;
    private bool chargeOnCooldown = true;
    private bool dashAttackOnCooldown = false;
    private bool readyToCharge = false;
    private bool chargedToWall = false;
    private bool stunned = false;
    private bool knockbackOnCooldown = false;
    private bool isBossLayer = false;
    private bool taserOnCooldown = false;
    private bool seedShootOnCooldown = false;
    private bool flashingRed = false;
    private bool changePhase = true;
    private bool openDoor = true;
    private bool isWaiting = false;
    public bool spawnEnemies = false;
    private bool isWaveOneEnemiesDestroyed = false;
    private bool isWaveTwoEnemiesDestroyed = false;
    [SerializeField] private bool phaseTwoEnemiesDestroyed = false;

    private bool chargeDirectionCalculated;

    private float lastChargeCounter; // Counter which check if the charge is out of cooldown.
    private float lastDashAttackCounter; // Counter which check if the DashAttack is out of cooldown.
    private float chargeDirection;
    private int backstepCounter;
    private int chargeCooldownRandomizer = 3;
    private int dashAttackCooldownRandomizer = 3;

    private float waitTime = 3f;

    private int attackRandomizer = 1; // Value between 1-100.

    private Quaternion batonRotation;



    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player").transform;
        targetHealth = target.GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        //health.TakeDamage(20);
        colliders = gameObject.GetComponentsInChildren<Transform>();
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();

        chargeCooldownRandomizer = UnityEngine.Random.Range(1, 11);
        seedShootChanceRandomizer = UnityEngine.Random.Range(1, 101);
        taserChanceRandomizer = UnityEngine.Random.Range(1, 101);
        dashAttackCooldownRandomizer = UnityEngine.Random.Range(10, 21);


        batonRotation = gameObject.transform.GetChild(4).rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Used to determine the direction where boss is going.
        vectorToTarget = (target.position - transform.position).normalized;
        velocity = rb.velocity;

        // Flip the localscale of the boss to the moving direction.
        if(state != RiotState.ShieldCharge && state != RiotState.Backstepping)
        {
            if (!isFacingRight && rb.velocity.x > 5f)
                Flip();
            else if (isFacingRight && rb.velocity.x < -5f)
                Flip();
        }

        // Flip the localscale towards the player when backstepping.
        if (state == RiotState.Backstepping)
        {
            if (isFacingRight && rb.velocity.x > 5f)
                Flip();
            else if (!isFacingRight && rb.velocity.x < -5f)
                Flip();
        }

        // If bosses health goes down more that 50%, change phase. The way of handling the state variation changes most likely in the future.
        if (health.CurrentHealth <= health.MaxHealth * 0.5f && changePhase)
        {

            changePhase = false;
            StopAllCoroutines();
            state = RiotState.PhaseTwoRun;
        }

        Timers();
        Debug.Log(state);
        // Riot Drone state machine.
        switch (state)
        {
            
            case RiotState.Idle:
                HandleIdleState();
                break;

            case RiotState.Moving:
                HandleMovingState();
                break;

            case RiotState.ShieldCharge:
                HandleShieldChargeState();
                break;

            case RiotState.Attack:
                HandleAttackState();
                break;

            case RiotState.TaserShoot:
                HandleTaserShootState();
                break;

            case RiotState.Stunned:
                HandleStunnedState();
                break;

            case RiotState.Backstepping:
                HandleBackstepping();
                break;

            // PHASE THREE
            //-----------------------------------------------------

            case RiotState.PhaseTwoRun:
                HandlePhaseTwoRun();
                break;

            // PHASE THREE
            //-----------------------------------------------------

            case RiotState.PhaseThreeStun:
                HandlePhaseThreeStun();
                if(spawnEnemies)
                {
                    HandleWaveOne();
                    HandleWaveTwo();
                }
                break;

            case RiotState.PhaseThreeMoving:
                HandlePhaseThreeMoving();
                break;

            case RiotState.PhaseThreeAttack:
                HandlePhaseThreeAttack();
                break;

            case RiotState.SeedShoot:
                HandleSeedShootState();
                break;

            case RiotState.DashAttack:
                HandleDashAttack();
                break;
        }


    }

    // STATE HANDLING
    //---------------------------------------------------------------------------------------------------------------------------------

    // PHASE ONE
    //---------------------------------------------------------------------------------------------------------------------------------

    // Boss is here only in the beginning of the fight.
    private void HandleIdleState()
    {
        if(!canStart)
        {
            if(!isWaiting)
                StartCoroutine(Wait(3));
            canStart = true;
        }

        if(!isWaiting && canStart)
        {
            state = RiotState.Moving;
        }
    }

    // Moves the boss in the direction of player only on X-axis. If target is in hit range, change state.
    private void HandleMovingState()
    {
        if (IsTargetInHitRange())
        {
            state = RiotState.Attack;
            return;
        }
        if (IsTargetInChargeRange() && canChargeToTarget)
        {
            state = RiotState.ShieldCharge;
            return;
        }
        if(IsTargetInChargeRange() && !canChargeToTarget && taserChanceRandomizer <= taserChance && !taserOnCooldown)
        {
            state = RiotState.TaserShoot;
            
            return;
        }

        // Moves the drone in desired direction, in this case towards the player on X-axis.
        if (!IsTargetInHitRange() && canMove)
        {
            
            //velocity = new Vector2(vectorToTarget.x * walkingSpeed, 0);
            //rb.MovePosition(rb.position + velocity * Time.deltaTime);
            rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * walkingSpeed * Time.fixedDeltaTime, 0));
            StartCoroutine(WalkCoolDown());
        } 
    }

    private void HandleShieldChargeState()
    {
        // Sets the direction where riot drone is going to charge and so it doesn't turn when player is on the other side of it.
        if(!chargeDirectionCalculated)
        {
            chargeDirectionCalculated = true;
            chargeDirection = (vectorToTarget.x > 0 ? 1 : -1);
        }
        // Can the drone charge to target / charge timer is not on cooldown.
        if(canChargeToTarget)
        {
            Debug.Log("GettingReadyToCharge");
            StartCoroutine(ReadyToCharge());
            canChargeToTarget = false;
            gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.red;
        }
        // Ready to charge.
        if (canMove && readyToCharge)
        {
            Debug.Log("Chaaarge!");
            StartCoroutine(ShieldChargeCoolDown());
        }

    }

    // Simple attack state where boss swings the weapon. Checks if target was in the hit area before doing damage to it.
    private void HandleAttackState()
    {
        if(canAttack)
        {
            StartCoroutine(LightAttack());
        }
    }


    private void HandleTaserShootState()
    {
        if(!taserOnCooldown)
        {
            StartCoroutine(TaserShoot());
        }

    }

    // Riot drone is stunned for a certain amount of time when collided with a wall. State change is in RiotShield script.
    private void HandleStunnedState()
    {
        if(!stunned)
        {
            StartCoroutine(Stunned());
            if (!isBossLayer)
            {
                ChangeToBossLayer();
                isBossLayer = true;
            }
        }
    }


    // Backsteps when needed for the amount of time specified. Used after shield charge has squished player between the wall and the riot shield.
    private void HandleBackstepping()
    {
        if (canMove && backstepCounter < timesToBackstep)
        {
            rb.AddForce(new Vector2((vectorToTarget.x < 0 ? 1 : -1) * walkingSpeed * Time.fixedDeltaTime, 0));
            StartCoroutine(WalkCoolDown());
        }
        else if(backstepCounter >= timesToBackstep)
        {
            backstepCounter = 0;
            state = RiotState.Moving;
        }
    }

    // PHASE TWO STATE HANDLERS
    //---------------------------------------------------------------------------------------------------------------------------------------
    private void HandlePhaseTwoRun()
    {
        if(openDoor)
        {
            doorOne.GetComponent<DoorController>().Work();
            openDoor = false;
            stunned = false;
            chargeDirectionCalculated = false;
            ChangeToDefaultLayer();
            StartCoroutine(Wait(3));
        }
        
        if (!flashingRed)
        {
            StartCoroutine(FlashRed());
        }
        
        if (!isWaiting)
        {
            isEnraged = true;
            ChangeToPassableLayer();
            //velocity = new Vector2(vectorToTarget.x * runningSpeed, 0);
            //rb.MovePosition(rb.position + velocity * Time.deltaTime);
            rb.AddForce(new Vector2(1,0) * runningSpeed * Time.fixedDeltaTime);
            StartCoroutine(RunCoolDown());
        }

    }


    // PHASE THREE STATE HANDLERS
    //---------------------------------------------------------------------------------------------------------------------------------------

    // This state is run only one when player fights the small enemies.
    private void HandlePhaseThreeStun()
    {
        stunned = true;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.cyan;
        if (phaseTwoEnemiesDestroyed)
        {
            Destroy(GameObject.Find("RiotShield"));
            ChangeToBossLayer(); // Set the boss layer so player can hit it whenever and doesn't get knocked back when hitting the body.
            stunned = false;
            isBossLayer = true;

            gameObject.GetComponentsInChildren<SpriteRenderer>()[0].color = Color.red;
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.red;
            gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
            GameObject.Find("PhaseTwoKnockbackBox").SetActive(true);

            state = RiotState.PhaseThreeMoving;
            return;
        }
    }

    private void HandleWaveOne()
    {
        if(phaseTwoEnemiesDestroyed || isWaveOneEnemiesDestroyed) { return; }

        //if(!isWaiting)
        //{
        //    StartCoroutine(Wait(3));
        //}

        if(!isWaiting && !isWaveOneEnemiesDestroyed)
        {
            if(bossEnemies[0] != null && bossEnemies[1] != null)
            {
                bossEnemies[0].SetActive(true);
                bossEnemies[1].SetActive(true);
            }

            if(bossEnemies[0] == null && bossEnemies[1] == null)
            {
                isWaveOneEnemiesDestroyed = true;
            }
            
        }
    }

    private void HandleWaveTwo()
    {
        if (!isWaveOneEnemiesDestroyed || phaseTwoEnemiesDestroyed || isWaveTwoEnemiesDestroyed) { return; }

        //if (!isWaiting)
        //{
        //    StartCoroutine(Wait(3));
        //}

        if (!isWaiting && !isWaveTwoEnemiesDestroyed)
        {
            if (bossEnemies[2] != null && bossEnemies[3] != null)
            {
                bossEnemies[2].SetActive(true);
                bossEnemies[3].SetActive(true);
            }
            if (bossEnemies[2] == null && bossEnemies[3] == null)
            {
                isWaveTwoEnemiesDestroyed = true;
                phaseTwoEnemiesDestroyed = true;
            }

        }
    }

    private void HandlePhaseThreeMoving()
    {
        if (IsTargetInHitRange())
        {
            state = RiotState.PhaseThreeAttack;
            return;
        }
        // Checks if boss can dash to player and doesn't hit the wall while doing so.
        if(IsTargetInDashAttackRange() && !AmIGoingToHitAWallAgain() && canDashAttack)
        {
            state = RiotState.DashAttack;
            return;
        }
        if (seedShootChanceRandomizer <= seedShootChance && !seedShootOnCooldown)
        {
            state = RiotState.SeedShoot;

            return;
        }
        if(!IsTargetInHitRange() && canMove)
        {
            //velocity = new Vector2(vectorToTarget.x * runningSpeed, 0);
            //rb.MovePosition(rb.position + velocity * Time.deltaTime);
            rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * runningSpeed * Time.fixedDeltaTime, 0));
            StartCoroutine(RunCoolDown());
        }

    }

    // Varies between the light and heavy attack randomly.
    private void HandlePhaseThreeAttack()
    {
        if (canAttack && attackRandomizer <= 50)
        {
            StartCoroutine(LightAttack());
        }

        if (canAttack && attackRandomizer > 50)
        {
            StartCoroutine(HeavyAttack());
        }
    }
    private void HandleSeedShootState()
    {
        if (!seedShootOnCooldown)
        {
            StartCoroutine(SeedShoot());
        }
    }

    private void HandleDashAttack()
    {
        if (!chargeDirectionCalculated)
        {
            chargeDirectionCalculated = true;
            chargeDirection = (vectorToTarget.x > 0 ? 1 : -1);
        }
        if(!dashAttackOnCooldown)
        {
            StartCoroutine(DashAttack());
        }

    }

    //-----------------------------------------------------------------------------------------------------------------------

    // LAYER CHANGES
    //------------------------------------------------------------------------------------------------------------------------
    private void ChangeToBossLayer()
    {
        bodyCollider.gameObject.layer = LayerMask.NameToLayer("Boss");
        phaseTwoKnockbackBox.gameObject.layer = LayerMask.NameToLayer("MovementBounds");
        headCollider.gameObject.layer = LayerMask.NameToLayer("BossWeakPoint");
    }

    private void ChangeToDefaultLayer()
    {
        bodyCollider.gameObject.layer = LayerMask.NameToLayer("Default");
        phaseTwoKnockbackBox.gameObject.layer = LayerMask.NameToLayer("MovementBounds");
        headCollider.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void ChangeToPassableLayer()
    {
        foreach(Transform collider in colliders)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("Player Projectile");
        }
        gameObject.layer = LayerMask.NameToLayer("Boss");
    }

    // COOLDOWNS AND ACTION BEHAVIOURS
    //------------------------------------------------------------------------------------------------------------------------
    private IEnumerator WalkCoolDown()
    {
        canMove = false;      
        yield return new WaitForSeconds(walkStepInterval);
        taserChanceRandomizer = UnityEngine.Random.Range(1, 101);
        backstepCounter++;
        canMove = true;
    }

    private IEnumerator RunCoolDown()
    {
        canMove = false;
        yield return new WaitForSeconds(runStepInterval);
        seedShootChanceRandomizer = UnityEngine.Random.Range(1, 101);
        canMove = true;
    }

    // Adds force with a cooldown to the drone.
    private IEnumerator ShieldChargeCoolDown()
    {
        canMove = false;
        yield return new WaitForSeconds(chargeStepInterval);

        //rb.MovePosition(rb.position + velocity * Time.deltaTime);
        rb.AddForce(new Vector2(chargeDirection * chargeSpeed * Time.fixedDeltaTime, 0));
        if(velocity.x < 2 && velocity.x > -2 && IsTargetInHitRange())
        {
            // Player is between the wall and the riot drone. Deal huge damage and make space for the player to get out.
            targetHealth.TakeDamage(shieldSquishDamage);
            StartCoroutine(PlayerHit());
            rb.velocity = new Vector2(0,0);
            backstepCounter = 0;
            gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
            chargeOnCooldown = true;
            chargeCooldownRandomizer = UnityEngine.Random.Range(1, 11);
            chargeDirectionCalculated = false;
            state = RiotState.Backstepping;
        }
        canMove = true;
    }

    // Prepares the riot drone for the upcoming charge.
    private IEnumerator ReadyToCharge()
    {
        readyToCharge = false;
        yield return new WaitForSeconds(chargeReadyTime);
        readyToCharge = true;
    }

    private IEnumerator Wait( int waitT)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitT);
        isWaiting = false;
    }

    // Flashes player sprite red.
    private IEnumerator PlayerHit()
    {
        GameObject.Find("Player").GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.1f);
        GameObject.Find("Player").GetComponent<SpriteRenderer>().color = Color.white;
    }

    // Riot drone is stunned. Set a timer when it cannot charge. Resets the charge direction.
    private IEnumerator Stunned()
    {
        stunned = true;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.cyan;
        yield return new WaitForSeconds(stunTime);
        stunned = false;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
        chargeDirectionCalculated = false;
        chargeOnCooldown = true;
        chargeCooldownRandomizer = UnityEngine.Random.Range(1, 11);
        state = RiotState.Moving;
        ChangeToDefaultLayer();
        isBossLayer = false;
    }

    private IEnumerator PhaseTwoStunned()
    {
        stunned = true;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.cyan;
        yield return new WaitForSeconds(Mathf.Infinity);
        stunned = false;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
    }

    // Cooldown for the player knockback.
    private IEnumerator KnockbackCooldown()
    {
        knockbackOnCooldown = true;
        yield return new WaitForSeconds(chargeReadyTime);
        knockbackOnCooldown = false;
    }


    // Attacks and their cooldowns.
    private IEnumerator LightAttack()
    {
        canAttack = false;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.blue;

        float collisionAngle = Vector2.SignedAngle(Vector2.right, Vector2.up);
        Quaternion q = Quaternion.AngleAxis(collisionAngle, Vector3.forward);
        if(!isFacingRight)
            gameObject.transform.GetChild(4).DORotate(new Vector3(0, 0, -90), lightAttackCoolDown); // Simple visual effect for baton so player knows when to hit and not.
        else
            gameObject.transform.GetChild(4).DORotate(new Vector3(0, 0, 90), lightAttackCoolDown);

        yield return new WaitForSeconds(lightAttackCoolDown);
        // Deal damage to player if still in range.
        if(IsTargetInHitRange())
        {
            targetHealth.TakeDamage(lightAttackDamage);
            Debug.Log("RiotHitLight!");
            PlayerPushback();
            gameObject.transform.GetChild(4).rotation = batonRotation;
            StartCoroutine(PlayerHit());
            yield return new WaitForSeconds(lightAttackCoolDown);
            
        }
        else
        {
            Debug.Log("Missed.");
            yield return new WaitForSeconds(lightAttackCoolDown);
            gameObject.transform.GetChild(4).rotation = batonRotation;
            if (!isEnraged)
            {
                state = RiotState.Moving;
            }
            else 
                state = RiotState.PhaseThreeMoving;

        }
        

        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
        attackRandomizer = UnityEngine.Random.Range(1, 101);
        canAttack = true;
    }

    private IEnumerator HeavyAttack()
    {
        canAttack = false;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.magenta;

        float collisionAngle = Vector2.SignedAngle(Vector2.right, Vector2.up);
        Quaternion q = Quaternion.AngleAxis(collisionAngle, Vector3.forward);
        if (!isFacingRight)
            gameObject.transform.GetChild(4).DORotate(new Vector3(0, 0, -90), heavyAttackCoolDown); // Simple visual effect for baton so player knows when to hit and not.
        else
            gameObject.transform.GetChild(4).DORotate(new Vector3(0, 0, 90), heavyAttackCoolDown);

        yield return new WaitForSeconds(heavyAttackChargeTime);
        // Deal damage to player if still in range.
        if (IsTargetInHitRange())
        {
            targetHealth.TakeDamage(heavyAttackDamage);
            Debug.Log("RiotHitHeavy!");
            PlayerPushback();
            gameObject.transform.GetChild(4).rotation = batonRotation;
            StartCoroutine(PlayerHit());
            yield return new WaitForSeconds(heavyAttackCoolDown);

            
        }
        else
        {
            Debug.Log("MissedHeavy.");
            gameObject.transform.GetChild(4).rotation = batonRotation;
            yield return new WaitForSeconds(heavyAttackCoolDown);
            if (!isEnraged)
            {
                state = RiotState.Moving;
            }
            else 
                state = RiotState.PhaseThreeMoving;
        }
        gameObject.transform.GetChild(4).rotation = batonRotation;

        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
        attackRandomizer = UnityEngine.Random.Range(1, 101);
        canAttack = true;
    }

    // Instantiates taserbeam gameobject
    private IEnumerator TaserShoot()
    {
        taserOnCooldown = true;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.yellow;
        yield return new WaitForSeconds(taserCooldown);
        Instantiate(taserBeam, new Vector2(transform.position.x + (transform.localScale.x * 2), transform.position.y), Quaternion.identity);
        yield return new WaitForSeconds(taserCooldown);
        taserChanceRandomizer = UnityEngine.Random.Range(1, 101);
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
        state = RiotState.Moving;
        taserOnCooldown = false;
    }

    // Shoots a random amount of seeds between 1-3 towards the player.
    private IEnumerator SeedShoot()
    {
        seedShootOnCooldown = true;
        int seedCount = 0;
        seedCount = UnityEngine.Random.Range(1, 4); // Amount of seeds to shoot.
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.yellow;
        yield return new WaitForSeconds(seedShootCooldown);
        for (int i = 0; i <= seedCount; i++)
        {
            yield return new WaitForSeconds(0.5f);
            Instantiate(seed, gameObject.transform.GetChild(2).position, Quaternion.identity); // Instantiates all seeds and shoots them towards the player.
        }
        yield return new WaitForSeconds(seedShootCooldown);
        seedShootChanceRandomizer = UnityEngine.Random.Range(1, 101);
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
        state = RiotState.PhaseThreeMoving;
        seedShootOnCooldown = false;
    }

    // A quick dash towards the player during third phase.
    private IEnumerator DashAttack()
    {
        bool takenDamage = false; // Bool so player doesn't take damage more than once during the dash.
        dashAttackOnCooldown = true;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[0].color = Color.black;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.black;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.red;

        yield return new WaitForSeconds(dashAttackCooldown);
        for(int i = 0; i <= dashForceCount; i++)
        {
            
            rb.AddForce(new Vector2(chargeDirection * walkingSpeed * Time.fixedDeltaTime, 0));
            
            if (IsTargetInHitRange() && !takenDamage)
            {
                targetHealth.TakeDamage(heavyAttackDamage);
                StartCoroutine(PlayerHit());
                PlayerPushback();
                takenDamage = true;
            }
            yield return new WaitForSeconds(0.05f);
            i++;
        }
        yield return new WaitForSeconds(0.5f);
        chargeDirectionCalculated = false;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[0].color = Color.red;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.red;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
        dashAttackCooldownRandomizer = UnityEngine.Random.Range(10, 21);
        state = RiotState.PhaseThreeMoving;
        canDashAttack = false;
        dashAttackOnCooldown = false;
    }

    private IEnumerator FlashRed()
    {
        flashingRed = true;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[0].color = Color.red;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.red;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponentsInChildren<SpriteRenderer>()[0].color = Color.blue;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.blue;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[2].color = Color.black;
        flashingRed = false;
    }

    // Overlaps for various checks.
    private bool IsTargetInHitRange()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * hitOffset.x), transform.position.y + hitOffset.y), areaToAttack, 0, playerLayer);
    }

    private bool IsTargetInChargeRange()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * chargeOffset.x), transform.position.y + chargeOffset.y), areaToCharge, 0, playerLayer);
    }

    private bool IsTargetInDashAttackRange()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * dashOffset.x), transform.position.y + dashOffset.y), areaToDash, 0, playerLayer);
    }

    // Check during third phase to see if the dash is going to hit the wall or not. If is, don't dash.
    private bool AmIGoingToHitAWallAgain()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * dashOffset.x), transform.position.y + dashOffset.y), areaToDash, 0, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x * chargeOffset.x), transform.position.y + chargeOffset.y), areaToCharge);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x * hitOffset.x), transform.position.y + hitOffset.y), areaToAttack);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x * dashOffset.x), transform.position.y + dashOffset.y), areaToDash);
    }

    public void Flip()
    {
        // Character flip
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // Timers to randomize action timings.
    private void Timers()
    {
        // Shield charge counter
        if (chargeOnCooldown)
        {
            lastChargeCounter += Time.deltaTime;
        }
        if (lastChargeCounter >= chargeCooldownRandomizer)
        {
            chargeOnCooldown = false;
            canChargeToTarget = true;
            lastChargeCounter = 0;
        }

        // Dashattack counter.
        if(!canDashAttack)
        {
            lastDashAttackCounter += Time.deltaTime;
        }
        if(lastDashAttackCounter >= dashAttackCooldownRandomizer)
        {
            canDashAttack = true;
            lastDashAttackCounter = 0;
        }

    }

    // Pushbacks the player when hit with riot drone collider. Uses velocity for the knockback instead of force.
    void PlayerPushback()
    {
        //float pushbackX = (target.position.x - transform.position.x > 0 ? 1 : -1);

        //Vector2 knockbackDirection = new Vector2(pushbackX, 0);
        //playerRB.AddForce(knockbackDirection * knockbackForce * Time.deltaTime);
        velocityPlayer = new Vector2(target.position.x - transform.position.x > 0 ? knockbackForce * 1 : knockbackForce * -1, 0);
        playerRB.MovePosition(playerRB.position + velocityPlayer * Time.deltaTime);
        StartCoroutine(KnockbackCooldown());
    }


    // COLLISIONS WITH PLAYER
    //---------------------------------------------------------------------------------------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Pushes player back when collider is hit and knockback is not on cooldown.
        if (collision.collider.tag == "Player" && !knockbackOnCooldown && !stunned && !isEnraged)
        {
            PlayerPushback();
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        // If player stays in contact with the boss, knockback.
        if (collision.collider.tag == "Player" && !knockbackOnCooldown && !stunned && !isEnraged)
        {
            PlayerPushback();
        }
    }

    // State names.
    public enum RiotState
    {
        Idle,
        Moving,
        ShieldCharge,
        Attack,
        TaserShoot,
        Stunned,
        SeedShoot,
        Backstepping,
        PhaseTwoRun,
        PhaseThreeStun,
        PhaseThreeMoving,
        PhaseThreeAttack,
        DashAttack

    }

    public bool getPhaseTwoEnemiesDestroyed()
    {
        return phaseTwoEnemiesDestroyed;
    }

    public void setPhaseTwoEnemiesDestroyed(bool b)
    {
        phaseTwoEnemiesDestroyed = b;
    }

    public bool getIsEnraged()
    {
        return isEnraged;
    }

    public void setSpawnEnemies(bool b)
    {
        spawnEnemies = b;
    }
}
