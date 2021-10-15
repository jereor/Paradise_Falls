using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotControlDrone : MonoBehaviour
{
    [SerializeField] private GameObject shield;
    [SerializeField] private GameObject taserBeam;
    [SerializeField] private Rigidbody2D playerRB;

    [Header("Current State")]
    public RiotState state = RiotState.Idle;

    [SerializeField] LayerMask playerLayer;

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

    private Transform target;
    private Health targetHealth;

    private Rigidbody2D rb;
    private Health health;
    private Transform[] colliders;
    
    [SerializeField] private bool isFacingRight = false;
    private Vector2 vectorToTarget;

    private bool canMove = true;
    private bool canAttack = true;
    private bool canChargeToTarget = true;
    private bool chargeOnCooldown = false;
    private bool readyToCharge = false;
    private bool chargedToWall = false;
    private bool stunned = false;
    private bool knockbackOnCooldown = false;
    private bool isBossLayer = false;
    private bool taserOnCooldown = false;

    private bool chargeDirectionCalculated;

    [SerializeField] private float lastChargeCounter; // Counter which check if the charge is out of cooldown.
    private float chargeDirection;
    private int backstepCounter;
    private int chargeCooldownRandomizer = 3;
    [SerializeField] private int taserChanceRandomizer = 1;
    [SerializeField] private int taserChance; // Number between 1-100.
    [SerializeField] private float taserCooldown;
    
    

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player").transform;
        targetHealth = target.GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        shield = GameObject.Find("RiotShield");
        colliders = GetComponentsInChildren<Transform>();

        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
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

        ChargeTimer();

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

            case RiotState.SeedShoot:
                HandleSeedShootState();
                break;

            case RiotState.Backstepping:
                HandleBackstepping();
                break;
        }

        // If bosses health goes down more that 50%, change phase. The way of handling the state variation changes most likely in the future.
        if(health.CurrentHealth <= health.MaxHealth * 0.5f)
        {
            isEnraged = true;
        }
    }

    // STATE HANDLING
    //---------------------------------------------------------------------------------------------------------------------------------

    private void HandleIdleState()
    {

    }

    // Moves the boss in the direction of player only on X-axis. If target is in hit range, change state.
    private void HandleMovingState()
    {
        if (IsTargetInHitRange())
        {
            Debug.Log("Attack");
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
        if (!isEnraged && !IsTargetInHitRange() && canMove)
        {
            
            //velocity = new Vector2(vectorToTarget.x * walkingSpeed, 0);
            //rb.MovePosition(rb.position + velocity * Time.deltaTime);
            rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * walkingSpeed * Time.fixedDeltaTime, 0));
            StartCoroutine(WalkCoolDown());
        }
        else if(isEnraged && !IsTargetInHitRange() && canMove)
        {
            //velocity = new Vector2(vectorToTarget.x * runningSpeed, 0);
            //rb.MovePosition(rb.position + velocity * Time.deltaTime);
            rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * runningSpeed * Time.fixedDeltaTime, 0));
            StartCoroutine(RunCoolDown());
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
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.red;
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
        if(canAttack && !isEnraged)
        {
            StartCoroutine(LightAttack());
        }

        if (canAttack && isEnraged)
        {
            StartCoroutine(HeavyAttack());
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
            Debug.Log("RiotStunned");
            StartCoroutine(Stunned());
            if (!isBossLayer)
            {
                ChangeToBossLayer();
                isBossLayer = true;
            }


            //Debug.Log("Stun ended");
            //chargeDirectionCalculated = false;
            //canChargeToTarget = false;
            //state = RiotState.Moving;

        }

    }

    private void HandleSeedShootState()
    {

    }

    // Backsteps when needed for the amount of time specified. Used after shield charge has squished player between the wall and the riot shield.
    private void HandleBackstepping()
    {
        if (!isEnraged && canMove && backstepCounter < timesToBackstep)
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

    private void ChangeToBossLayer()
    {
        foreach (Transform bossCollider in colliders)
        {
            bossCollider.gameObject.layer = LayerMask.NameToLayer("Boss");
        }
    }

    private void ChangeToDefaultLayer()
    {
        foreach (Transform bossCollider in colliders)
        {
            bossCollider.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    // Cooldowns for walking, running etc.
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
            Debug.Log("Squished");
            targetHealth.TakeDamage(shieldSquishDamage);
            StartCoroutine(PlayerHit());
            rb.velocity = new Vector2(0,0);
            backstepCounter = 0;
            gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.black;
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
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.cyan;
        yield return new WaitForSeconds(stunTime);
        stunned = false;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.black;
        Debug.Log("Stun ended");
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
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.blue;
        yield return new WaitForSeconds(lightAttackCoolDown);
        // Deal damage to player if still in range.
        if(IsTargetInHitRange())
        {
            targetHealth.TakeDamage(lightAttackDamage);
            StartCoroutine(PlayerHit());
            yield return new WaitForSeconds(lightAttackCoolDown);
            if (!knockbackOnCooldown)
            {
                PlayerPushback();
            }
            Debug.Log("RiotHit!");
        }
        else
        {
            Debug.Log("Missed.");
            yield return new WaitForSeconds(lightAttackCoolDown);
            state = RiotState.Moving;
        }
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.black;
        canAttack = true;
    }

    private IEnumerator HeavyAttack()
    {
        canAttack = false;
        yield return new WaitForSeconds(heavyAttackChargeTime);
        // Deal damage to player if still in range.
        if (IsTargetInHitRange())
        {
            targetHealth.TakeDamage(heavyAttackDamage);
            StartCoroutine(PlayerHit());
            yield return new WaitForSeconds(heavyAttackCoolDown);
            if(!knockbackOnCooldown)
            {
                PlayerPushback();
            }

            Debug.Log("RiotHitHeavy!");
        }
        else
        {
            Debug.Log("MissedHeavy.");
            yield return new WaitForSeconds(heavyAttackCoolDown);
            state = RiotState.Moving;
        }

        canAttack = true;
    }

    private IEnumerator TaserShoot()
    {
        taserOnCooldown = true;
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.yellow;
        yield return new WaitForSeconds(taserCooldown);
        Instantiate(taserBeam, new Vector2(transform.position.x + (transform.localScale.x * 2), transform.position.y), Quaternion.identity);
        yield return new WaitForSeconds(taserCooldown);
        taserChanceRandomizer = UnityEngine.Random.Range(1, 101);
        gameObject.GetComponentsInChildren<SpriteRenderer>()[1].color = Color.black;
        state = RiotState.Moving;
        taserOnCooldown = false;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x * chargeOffset.x), transform.position.y + chargeOffset.y), areaToCharge);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x * hitOffset.x), transform.position.y + hitOffset.y), areaToAttack);

    }

    private void Flip()
    {
        // Character flip
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // Checks if last charge was over 10 seconds ago.
    private void ChargeTimer()
    {
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
        if (collision.collider.tag == "Player" && !knockbackOnCooldown && !stunned)
        {
            PlayerPushback();
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        // If player stays in contact with the boss, knockback.
        if (collision.collider.tag == "Player" && !knockbackOnCooldown && !stunned)
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
        Backstepping

    }

}
