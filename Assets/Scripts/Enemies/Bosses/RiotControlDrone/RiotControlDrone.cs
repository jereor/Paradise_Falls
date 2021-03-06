using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RiotControlDrone : MonoBehaviour
{
    [SerializeField] private GameObject taserBeam;
    [SerializeField] private GameObject seed;
    [SerializeField] private GameObject boxInstance;
    [SerializeField] private BoxCollider2D bodyCollider;
    [SerializeField] private BoxCollider2D headCollider;
    [SerializeField] private BoxCollider2D phaseTwoKnockbackBox;
    [SerializeField] private Transform[] colliders;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private GameObject doorOne;
    [SerializeField] private Animator riotAnimator;
    [SerializeField] private RiotPhaseTwoPlatformController phaseTwoPlatformController;

    [Header("Current State")]
    public RiotState state = RiotState.Idle;
    private RiotState stateChangeIdentfier; // Used to determine right animations and so it doesn't loop the function for no reason in update.

    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask breakableLayer;

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

    private GameObject target;
    private Health targetHealth;

    private Rigidbody2D rb;
    private Health health;
    
    [SerializeField] private bool isFacingRight = false;
    private Vector2 vectorToTarget;
    private Vector2 startPosition;

    private bool canStart = false;
    private bool canMove = true;
    private bool canAttack = true;
    private bool canDashAttack = true;
    private bool canChargeToTarget = false;
    private bool chargeOnCooldown = true;
    private bool dashAttackOnCooldown = false;
    private bool readyToCharge = false;
    //private bool chargedToWall = false;
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
    private bool phaseTwoComplete = false;
    private bool isDead = false;

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

    // SFX
    private bool playMeleeSound = false;
    private bool playTazerShootSound = false;
    private bool playSeedShootSound = false;
    private bool playStunnedSound = false;
    private bool playChargeSound = false;
    private bool playStepSound = false;
    private bool playRunSound = false;


    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player");
        playerRB = target.GetComponent<Rigidbody2D>();
        targetHealth = target.GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        phaseTwoPlatformController = GameObject.Find("RiotPhaseTwoController").GetComponent<RiotPhaseTwoPlatformController>();

        riotAnimator = transform.GetChild(0).GetComponent<Animator>();

        colliders = gameObject.GetComponentsInChildren<Transform>();
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();

        chargeCooldownRandomizer = UnityEngine.Random.Range(1, 11);
        seedShootChanceRandomizer = UnityEngine.Random.Range(1, 101);
        taserChanceRandomizer = UnityEngine.Random.Range(1, 101);
        dashAttackCooldownRandomizer = UnityEngine.Random.Range(10, 21);

        startPosition = transform.position;
        batonRotation = gameObject.transform.GetChild(4).rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {


        if (target == null && state != RiotState.PlayerIsDead) 
        {
            canStart = false;
            isWaiting = false;
            canMove = true;
            StopAllCoroutines();
            state = RiotState.PlayerIsDead; 
        }
        // Used to determine the direction where boss is going.
        if(target != null)
            vectorToTarget = (target.transform.position - transform.position).normalized;

        if (health.GetHealth() <= 0 && !isDead)
        {
            StopAllCoroutines();
            DOTween.PauseAll();
            foreach (GameObject enemy in phaseTwoPlatformController.droneInstances)
                Destroy(enemy);
            state = RiotState.Die;
        }

        velocity = rb.velocity;

        // Flip the localscale of the boss to the moving direction.
        if(state != RiotState.ShieldCharge && state != RiotState.Backstepping && !isDead)
        {
            if (!isFacingRight && rb.velocity.x > 5f)
                Flip();
            else if (isFacingRight && rb.velocity.x < -5f)
                Flip();
        }

        // Flip the localscale towards the player when backstepping.
        if (state == RiotState.Backstepping && !isDead)
        {
            if (isFacingRight && rb.velocity.x > 5f)
                Flip();
            else if (!isFacingRight && rb.velocity.x < -5f)
                Flip();
        }

        // If bosses health goes down more that 50%, change phase. The way of handling the state variation changes most likely in the future.
        if (health.GetHealth() <= health.GetMaxHealth() * 0.5f && changePhase && !isDead)
        {

            changePhase = false;
            StopAllCoroutines();
            state = RiotState.PhaseTwoRun;
        }

        Timers();

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

            case RiotState.Die:
                HandleDie();
                break;

            // PLAYER IS DEAD
            //------------------------------------------------------

            case RiotState.PlayerIsDead:
                HandlePlayerIsDead();
                break;
        }

        if (state != stateChangeIdentfier)
        {
            HandleAnimations();
            stateChangeIdentfier = state;
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
                StartCoroutine(Wait(waitTime));
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
        if (IsTargetInHitRange() || IsBoxInHitRange())
        {
            state = RiotState.Attack;
            return;
        }
        if (IsTargetInChargeRange() && state != RiotState.Attack && canChargeToTarget)
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
            StartCoroutine(ReadyToCharge());
            canChargeToTarget = false;
        }
        // Ready to charge.
        if (canMove && readyToCharge)
        {
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
        if(target.transform.position.x - transform.position.x >= 0)
        {
            transform.localScale = new Vector2(1, 1);
            isFacingRight = true;
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            isFacingRight = false;
        }
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
            riotAnimator.SetBool("Stunned", true);
            StartCoroutine(Wait(3));
        }
        
        if (!flashingRed)
        {
            StartCoroutine(FlashRed());
        }
        
        if (!isWaiting)
        {
            riotAnimator.SetBool("FastRun", true);
            riotAnimator.SetBool("Stunned", false);
            isEnraged = true;
            ChangeToPassableLayer();
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
        if (phaseTwoComplete)
        {
            if(GameObject.Find("RiotShield").GetComponent<Health>().GetHealth() == 0)
            {
                Destroy(GameObject.Find("RiotShield"));
                GameObject.Find("PhaseTwoKnockbackBox").GetComponent<BoxCollider2D>().size = new Vector2(1.2f, 0.46f);
                GameObject.Find("PhaseTwoKnockbackBox").GetComponent<BoxCollider2D>().offset = new Vector2(0.03f, 0);
            }
            ChangeToBossLayer(); // Set the boss layer so player can hit it whenever and doesn't get knocked back when hitting the body.
            stunned = false;
            isBossLayer = true;
            GameObject.Find("PhaseTwoKnockbackBox").SetActive(true);

            state = RiotState.PhaseThreeMoving;
            return;
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
        if (target.transform.position.x - transform.position.x >= 0)
        {
            transform.localScale = new Vector2(1, 1);
            isFacingRight = true;
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            isFacingRight = false;
        }
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
    private void HandleDie()
    {
        if(!isDead)
        {
            isDead = true;
            bodyCollider.enabled = false;
            headCollider.enabled = false;
            phaseTwoKnockbackBox.enabled = false;
            StartCoroutine(BossIsDead());

        }
    }

    private void HandlePlayerIsDead()
    {
        if (!canStart && !isWaiting)
        {
            StartCoroutine(Wait(waitTime));
            canStart = true;
        }

        if (!isWaiting && canStart && canMove)
        {
            Vector2 vectorToStart = new Vector2(startPosition.x - transform.position.x, 0);
            rb.AddForce(new Vector2((vectorToStart.x > 0 ? 1 : -1) * walkingSpeed * Time.fixedDeltaTime, 0));
            StartCoroutine(WalkCoolDown());
            if(Vector2.Distance(transform.position, startPosition) < 0.5)
            {
                transform.localScale = new Vector2(-1, 1);
                StartCoroutine(Wait(Mathf.Infinity));
            }
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
        playStepSound = true;
        yield return new WaitForSeconds(walkStepInterval);
        taserChanceRandomizer = UnityEngine.Random.Range(1, 101);
        backstepCounter++;
        canMove = true;
    }

    private IEnumerator RunCoolDown()
    {
        canMove = false;
        playRunSound = true;
        yield return new WaitForSeconds(runStepInterval);
        seedShootChanceRandomizer = UnityEngine.Random.Range(1, 101);
        canMove = true;
    }

    // Adds force with a cooldown to the drone.
    private IEnumerator ShieldChargeCoolDown()
    {
        canMove = false;
        yield return new WaitForSeconds(chargeStepInterval);

        rb.AddForce(new Vector2(chargeDirection * chargeSpeed * Time.fixedDeltaTime, 0));

        playChargeSound = true;

        if(velocity.x < 2 && velocity.x > -2 && IsTargetInHitRange())
        {
            // Player is between the wall and the riot drone. Deal huge damage and make space for the player to get out.
            targetHealth.TakeDamage(shieldSquishDamage);
            rb.velocity = new Vector2(0,0);
            backstepCounter = 0;
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

    private IEnumerator Wait( float waitT)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitT);
        isWaiting = false;
    }

    // Riot drone is stunned. Set a timer when it cannot charge. Resets the charge direction.
    private IEnumerator Stunned()
    {
        stunned = true;
        playStunnedSound = true;

        yield return new WaitForSeconds(stunTime);
        stunned = false;
        chargeDirectionCalculated = false;
        chargeOnCooldown = true;
        chargeCooldownRandomizer = UnityEngine.Random.Range(1, 11);
        state = RiotState.Moving;
        ChangeToDefaultLayer();
        isBossLayer = false;
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

        riotAnimator.SetBool("LightAttackCharge", true);
        yield return new WaitForSeconds(lightAttackCoolDown);
        
        playMeleeSound = true;
        riotAnimator.SetBool("LightAttackCharge", false);
        riotAnimator.SetBool("LightAttackSwing", true);
        // Deal damage to player if still in range.
        if (IsTargetInHitRange())
        {
            targetHealth.TakeDamage(lightAttackDamage);
            PlayerPushback();
            yield return new WaitForSeconds(lightAttackCoolDown);

        }
        else if(boxInstance != null)
        {
            Destroy(boxInstance);
            boxInstance = null;

            yield return new WaitForSeconds(lightAttackCoolDown);
            if (!isEnraged)
            {
                state = RiotState.Moving;
            }
            else
                state = RiotState.PhaseThreeMoving;
        }
        else
        {
            yield return new WaitForSeconds(lightAttackCoolDown);
            if (!isEnraged)
            {
                state = RiotState.Moving;
            }
            else 
                state = RiotState.PhaseThreeMoving;

        }
        riotAnimator.SetBool("LightAttackSwing", false);
        attackRandomizer = UnityEngine.Random.Range(1, 101);
        canAttack = true;
    }

    private IEnumerator HeavyAttack()
    {
        canAttack = false;
        riotAnimator.SetBool("HeavyAttackCharge", true);

        yield return new WaitForSeconds(heavyAttackChargeTime);

        playMeleeSound = true;
        riotAnimator.SetBool("HeavyAttackSwing", true);
        riotAnimator.SetBool("HeavyAttackCharge", false);
        // Deal damage to player if still in range.
        if (IsTargetInHitRange())
        {
            targetHealth.TakeDamage(heavyAttackDamage);
            PlayerPushback();
            yield return new WaitForSeconds(heavyAttackCoolDown);

            
        }
        else
        {
            yield return new WaitForSeconds(heavyAttackCoolDown);
            if (!isEnraged)
            {
                state = RiotState.Moving;
            }
            else 
                state = RiotState.PhaseThreeMoving;
        }
        riotAnimator.SetBool("HeavyAttackSwing", false);
        gameObject.GetComponentsInChildren<SpriteRenderer>()[0].color = Color.white;
        attackRandomizer = UnityEngine.Random.Range(1, 101);
        canAttack = true;
    }

    // Instantiates taserbeam gameobject
    private IEnumerator TaserShoot()
    {
        taserOnCooldown = true;
        riotAnimator.SetBool("TaserShoot", true);
        yield return new WaitForSeconds(taserCooldown);

        playTazerShootSound = true;
        
        Instantiate(taserBeam, new Vector2(transform.position.x + (transform.localScale.x * 2), transform.position.y), Quaternion.identity);
        yield return new WaitForSeconds(taserCooldown);
        riotAnimator.SetBool("TaserShoot", false);
        taserChanceRandomizer = UnityEngine.Random.Range(1, 101);
        state = RiotState.Moving;
        taserOnCooldown = false;
    }

    // Shoots a random amount of seeds between 1-3 towards the player.
    private IEnumerator SeedShoot()
    {
        seedShootOnCooldown = true;
        int seedCount = 0;
        seedCount = UnityEngine.Random.Range(1, 4); // Amount of seeds to shoot.
        riotAnimator.SetBool("TaserShoot", true);
        yield return new WaitForSeconds(seedShootCooldown);
        for (int i = 0; i <= seedCount; i++)
        {
            yield return new WaitForSeconds(0.5f);

            playSeedShootSound = true;

            Instantiate(seed, gameObject.transform.GetChild(2).position, Quaternion.identity); // Instantiates all seeds and shoots them towards the player.
        }
        yield return new WaitForSeconds(seedShootCooldown);
        riotAnimator.SetBool("TaserShoot", false);
        seedShootChanceRandomizer = UnityEngine.Random.Range(1, 101);
        state = RiotState.PhaseThreeMoving;
        seedShootOnCooldown = false;
    }

    // A quick dash towards the player during third phase.
    private IEnumerator DashAttack()
    {
        bool takenDamage = false; // Bool so player doesn't take damage more than once during the dash.
        dashAttackOnCooldown = true;

        yield return new WaitForSeconds(dashAttackCooldown);
        for(int i = 0; i <= dashForceCount; i++)
        {
            playStepSound = true;
            rb.AddForce(new Vector2(chargeDirection * walkingSpeed * Time.fixedDeltaTime, 0));


            if (IsTargetInHitRange() && !takenDamage)
            {
                playMeleeSound = true;
                targetHealth.TakeDamage(heavyAttackDamage);
                PlayerPushback();
                takenDamage = true;
            }
            yield return new WaitForSeconds(0.05f);
            i++;
        }
        yield return new WaitForSeconds(0.5f);
        chargeDirectionCalculated = false;
        dashAttackCooldownRandomizer = UnityEngine.Random.Range(10, 21);
        state = RiotState.PhaseThreeMoving;
        canDashAttack = false;
        dashAttackOnCooldown = false;
    }

    private IEnumerator FlashRed()
    {
        flashingRed = true;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[0].color = Color.red;
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponentsInChildren<SpriteRenderer>()[0].color = Color.white;
        yield return new WaitForSeconds(0.5f);
        flashingRed = false;
    }

    private IEnumerator BossIsDead()
    {
        ChangeToDefaultLayer();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(1);
        PlayerCamera.Instance.CameraShake(0.7f, 4);
        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            transform.DOScaleX(1.15f, 0.2f);
            transform.DOScaleY(1f, 0.2f);
            yield return new WaitForSeconds(0.2f);
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
            transform.DOScaleX(1f, 0.2f);
            transform.DOScaleY(1.15f, 0.2f);
            yield return new WaitForSeconds(0.2f);
        }
        transform.DOScaleX(1.15f, 0.2f);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
        float endValue = 0;
        float duration = 1;
        float elapsedTime = 0;
        float startValue = transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(transform.GetChild(0).GetComponent<SpriteRenderer>().color.r, transform.GetChild(0).GetComponent<SpriteRenderer>().color.g, transform.GetChild(0).GetComponent<SpriteRenderer>().color.b, newAlpha);
            yield return null;
        }
        phaseTwoPlatformController.OpenDoors();
        yield return new WaitForSeconds(1);

        //victoryMusic.Invoke();

        Destroy(gameObject);
    }

    // Overlaps for various checks.
    private bool IsTargetInHitRange()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * hitOffset.x), transform.position.y + hitOffset.y), areaToAttack, 0, playerLayer);
    }

    private bool IsBoxInHitRange()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * hitOffset.x), transform.position.y + hitOffset.y), areaToAttack, 0, breakableLayer);
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
    public void PlayerPushback()
    {
        velocityPlayer = new Vector2(target.transform.position.x - transform.position.x > 0 ? knockbackForce * 1 : knockbackForce * -1, knockbackForce / 3);
        playerRB.MovePosition(playerRB.position + velocityPlayer * Time.deltaTime);
        StartCoroutine(KnockbackCooldown());
    }

    private void HandleAnimations()
    {
        if(state == RiotState.Idle)
        {
            riotAnimator.SetBool("Idle", true);
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("TaserShoot", false);
            riotAnimator.SetBool("LightAttackSwing", false);
            riotAnimator.SetBool("BackStepping", false);
        }

        if (state == RiotState.Moving)
        {
            riotAnimator.SetBool("SlowWalk", true);
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("TaserShoot", false);
            riotAnimator.SetBool("LightAttackSwing", false);
            riotAnimator.SetBool("BackStepping", false);
        }

        if (state == RiotState.ShieldCharge)
        {
            riotAnimator.SetBool("ShieldCharge", true);
            riotAnimator.SetBool("Idle", false);
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("TaserShoot", false);
            riotAnimator.SetBool("SlowWalk", false);
            riotAnimator.SetBool("BackStepping", false);
            riotAnimator.SetBool("LightAttackSwing", false);

        }

        if (state == RiotState.Attack)
        {
            riotAnimator.SetBool("Idle", false);
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("TaserShoot", false);
            riotAnimator.SetBool("SlowWalk", false);
            riotAnimator.SetBool("BackStepping", false);
            riotAnimator.SetBool("ShieldCharge", false);
        }

        if (state == RiotState.TaserShoot)
        {
            riotAnimator.SetBool("TaserShoot", true);
            riotAnimator.SetBool("Idle", false);
            riotAnimator.SetBool("LightAttack", false);
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("SlowWalk", false);
            riotAnimator.SetBool("BackStepping", false);
            riotAnimator.SetBool("LightAttackCharge", false);
            riotAnimator.SetBool("LightAttackSwing", false);

        }

        if (state == RiotState.Stunned)
        {
            riotAnimator.SetBool("Stunned", true);
            riotAnimator.SetBool("ShieldCharge", false);
            riotAnimator.SetBool("TaserShoot", false);
            riotAnimator.SetBool("Idle", false);
            riotAnimator.SetBool("SlowWalk", false);
            riotAnimator.SetBool("LightAttackCharge", false);
            riotAnimator.SetBool("LightAttackSwing", false);

        }

        if (state == RiotState.Backstepping)
        {
            riotAnimator.SetBool("BackStepping", true);
            riotAnimator.SetBool("ShieldCharge", false);
            riotAnimator.SetBool("LightAttack", false);
            riotAnimator.SetBool("LightAttackCharge", false);
            riotAnimator.SetBool("LightAttackSwing", false);


        }

        // PHASE THREE
        //-----------------------------------------------------

        if (state == RiotState.PhaseTwoRun)
        {
            riotAnimator.SetBool("PhaseTwoRun", false);
            riotAnimator.SetBool("Run", false); // Placeholder
            riotAnimator.SetBool("ShieldCharge", false);
            //riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("BackStepping", false);
            riotAnimator.SetBool("SlowWalk", false);
        }

        // PHASE THREE
        //-----------------------------------------------------

        if (state == RiotState.PhaseThreeStun)
        {
            riotAnimator.SetBool("PhaseThreeStun", true);
            riotAnimator.SetBool("Stunned", true); // placeholder
            riotAnimator.SetBool("FastRun", false);
        }

        if (state == RiotState.PhaseThreeMoving)
        {
            riotAnimator.SetBool("Run", true);
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("ShieldCharge", false);
            riotAnimator.SetBool("SeedShoot", false);
            riotAnimator.SetBool("TaserShoot", false);
            riotAnimator.SetBool("HeavyAttackSwing", false);
            riotAnimator.SetBool("LightAttackSwing", false);
        }

        if (state == RiotState.PhaseThreeAttack)
        {
            riotAnimator.SetBool("Idle", false); 
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("ShieldCharge", false);
            riotAnimator.SetBool("SeedShoot", false);
            riotAnimator.SetBool("TaserShoot", false);
            riotAnimator.SetBool("Run", false);
        }

        if (state == RiotState.SeedShoot)
        {
            riotAnimator.SetBool("SeedShoot", true);
            riotAnimator.SetBool("TaserShoot", true); // Placeholder
            riotAnimator.SetBool("Idle", false);
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("ShieldCharge", false);
            riotAnimator.SetBool("Run", false);
            riotAnimator.SetBool("HeavyAttackSwing", false);
            riotAnimator.SetBool("LightAttackSwing", false);
        }

        if (state == RiotState.DashAttack)
        {
            riotAnimator.SetBool("Dash", true);
            riotAnimator.SetBool("ShieldCharge", true); // Placeholder, change false when right animations
            riotAnimator.SetBool("Stunned", false);
            riotAnimator.SetBool("Run", false);
            riotAnimator.SetBool("SeedShoot", false);
            riotAnimator.SetBool("TaserShoot", false);
            riotAnimator.SetBool("HeavyAttackSwing", false);
            riotAnimator.SetBool("LightAttackSwing", false);
        }
        if(state == RiotState.Die)
        {
            riotAnimator.SetBool("SeedShoot", false);
            riotAnimator.SetBool("TaserShoot", false); // Placeholder
            riotAnimator.SetBool("Idle", false);
            riotAnimator.SetBool("Stunned", true);
            riotAnimator.SetBool("ShieldCharge", false);
            riotAnimator.SetBool("Run", false);
            riotAnimator.SetBool("HeavyAttackSwing", false);
            riotAnimator.SetBool("LightAttackSwing", false);
        }
    }


    // COLLISIONS
    //---------------------------------------------------------------------------------------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if(collision.gameObject.tag == "Box")
        {
            //Debug.Log("Box velocity: " + collision.gameObject.GetComponent<Rigidbody2D>().velocity.y);
            if (state == RiotState.ShieldCharge || collision.gameObject.GetComponent<Rigidbody2D>().velocity.y < 0)
            {
                health.TakeDamage(5);
                Destroy(collision.collider.gameObject);
            }
        }

        if(collision.gameObject.tag == "MeleeWeapon" && state == RiotState.ShieldCharge)
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "MeleeWeapon" && state == RiotState.ShieldCharge)
        {
            Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
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
        DashAttack,
        Die,
        PlayerIsDead

    }

    // SFX

    public float getWalkStepInterval()
    {
        return walkStepInterval;
    }

    public float getRunStepInterval()
    {
        return runStepInterval;
    }
    public float getChargeStepInterval()
    {
        return chargeStepInterval;
    }

    public bool getPhaseTwoComplete()
    {
        return phaseTwoComplete;
    }

    public void setPhaseTwoComplete(bool b)
    {
        phaseTwoComplete = b;
    }

    public bool getIsEnraged()
    {
        return isEnraged;
    }

    public void setSpawnEnemies(bool b)
    {
        spawnEnemies = b;
    }

    public void setBoxInstance(GameObject b)
    {
        boxInstance = b;
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

    public bool getPlaySoundTazer()
    {
        if (playTazerShootSound)
        {
            playTazerShootSound = false;
            return true;
        }
        else
            return false;
    }
    public bool getPlaySoundSeed()
    {
        if (playSeedShootSound)
        {
            playSeedShootSound = false;
            return true;
        }
        else
            return false;
    }
    public bool getPlaySoundStunned()
    {
        if (playStunnedSound)
        {
            playStunnedSound = false;
            return true;
        }
        else
            return false;
    }
    public bool getPlaySoundCharge()
    {
        if (playChargeSound)
        {
            playChargeSound = false;
            return true;
        }
        else
            return false;
    }

    public bool getPlaySoundStep()
    {
        if (playStepSound)
        {
            playStepSound = false;
            return true;
        }
        else
            return false;
    }

    public bool getPlaySoundRun()
    {
        if (playRunSound)
        {
            playRunSound = false;
            return true;
        }
        else
            return false;
    }
}
