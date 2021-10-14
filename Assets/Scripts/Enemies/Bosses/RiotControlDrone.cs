using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotControlDrone : MonoBehaviour
{
    [SerializeField] private GameObject shield;

    [Header("Current State")]
    [SerializeField] private RiotState state = RiotState.Idle;

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
    [SerializeField] private Vector2 areaToCharge;
    [SerializeField] private Vector2 chargeOffset;
    [SerializeField] private Vector2 areaToAttack;
    [SerializeField] private Vector2 hitOffset;
    [SerializeField] private Vector2 velocity;
    [SerializeField] private float velocityX;

    [Header("AttackPower and Hit Cooldowns")]
    [SerializeField] private float lightAttackCoolDown;
    [SerializeField] private float lightAttackDamage;
    [SerializeField] private float heavyAttackChargeTime;
    [SerializeField] private float heavyAttackCoolDown;
    [SerializeField] private float heavyAttackDamage;

    private Transform target;
    private Health targetHealth;

    private Rigidbody2D rb;
    private Health health;
    
    [SerializeField] private bool isFacingRight = false;
    private Vector2 vectorToTarget;

    private bool canMove = true;
    private bool canAttack = true;
    private bool chargedToWall = false;

    private bool chargeDirectionCalculated;
    
    

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player").transform;
        targetHealth = target.GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        shield = GameObject.Find("RiotShield");
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        // Used to determine the direction where boss is going.
        vectorToTarget = (target.position - transform.position).normalized;

        // Flip the localscale of the boss.
        //if (!isFacingRight && rb.velocity.x > 0.05f)
        //    Flip();
        //else if (isFacingRight && rb.velocity.x < -0.05f)
        //    Flip();

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
        if (IsTargetInHitRange() && canMove)
        {
            state = RiotState.Attack;
        }
        if (IsTargetInChargeRange() && canMove)
        {
            state = RiotState.ShieldCharge;
        }

        if (!isEnraged && canMove)
        {
            velocity = new Vector2(vectorToTarget.x * walkingSpeed, 0);
            if (!isFacingRight && velocity.x > 0.05f)
                Flip();
            else if (isFacingRight && velocity.x < -0.05f)
                Flip();
            //rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * walkingSpeed * Time.deltaTime, 0));
            rb.MovePosition(rb.position + velocity * Time.deltaTime);
            StartCoroutine(WalkCoolDown());
        }
        else if(isEnraged && canMove)
        {
            velocity = new Vector2(vectorToTarget.x * runningSpeed, 0);
            //rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * runningSpeed * Time.deltaTime, 0));
            rb.MovePosition(rb.position + velocity * Time.deltaTime);
            StartCoroutine(WalkCoolDown());
        }

        
    }

    private void HandleShieldChargeState()
    {
        if(!chargeDirectionCalculated)
        {
            chargeDirectionCalculated = true;
            velocity = new Vector2(vectorToTarget.x * chargeSpeed, 0);
        }
        if(canMove)
        {
            StartCoroutine(ShieldChargeCoolDown());
        }
        if(chargedToWall)
        {
            state = RiotState.Stunned;
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
        
    }

    private void HandleStunnedState()
    {
        StartCoroutine(Stunned());
        if(canMove)
        {
            chargeDirectionCalculated = false;
            state = RiotState.Moving;
        }
    }

    private void HandleSeedShootState()
    {

    }

    // Cooldowns for walking, running etc.
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

    private IEnumerator ShieldChargeCoolDown()
    {
        canMove = false;
        yield return new WaitForSeconds(chargeStepInterval);
        
        rb.MovePosition(rb.position + velocity * Time.deltaTime);
        //rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * chargeSpeed * Time.deltaTime, 0));
        canMove = true;
    }

    private IEnumerator Stunned()
    {
        canMove = false;
        yield return new WaitForSeconds(stunTime);
        canMove = true;
    }



    private IEnumerator LightAttack()
    {
        canAttack = false;
        yield return new WaitForSeconds(lightAttackCoolDown);
        // Deal damage to player if still in range.
        if(IsTargetInHitRange())
        {
            targetHealth.TakeDamage(lightAttackDamage);
            yield return new WaitForSeconds(lightAttackCoolDown);
            Debug.Log("RiotHit!");
        }
        else
        {
            Debug.Log("Missed.");
            yield return new WaitForSeconds(lightAttackCoolDown);
            state = RiotState.Moving;
        }

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
            yield return new WaitForSeconds(heavyAttackCoolDown);
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


    // COLLISIONS WITH PLAYER
    //---------------------------------------------------------------------------------------------------------------------------------
    // Used for stopping the target from moving the boss. Not sure if this is a good method of doing it. Otherwise we would have to increase mass and gravity, meaning the force amounts are going to be massive.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.collider.gameObject.layer == 6)
        //{
        //    state = RiotState.Stunned;
        //}

        //if ()
        //{
        //    chargedToWall = true;
        //    state = RiotState.Stunned;
        //}
    }

    //private void OnCollisionStay2D(Collision2D collision)
    //{
    //    if (collision.collider.tag == "Player" && state != RiotState.ShieldCharge)
    //    {
    //        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (collision.collider.tag == "Player" && state != RiotState.ShieldCharge)
    //    {
    //        rb.constraints = RigidbodyConstraints2D.None;
    //        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    //    }
    //}

    // State names.
    public enum RiotState
    {
        Idle,
        Moving,
        ShieldCharge,
        Attack,
        TaserShoot,
        Stunned,
        SeedShoot

    }

    public RiotState getState()
    {
        return state;
    }

}
