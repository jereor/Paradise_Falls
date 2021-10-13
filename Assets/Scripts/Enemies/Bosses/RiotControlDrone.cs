using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiotControlDrone : MonoBehaviour
{


    [Header("Current State")]
    [SerializeField] private RiotState state = RiotState.Idle;

    [SerializeField] LayerMask playerLayer;

    [Header("Movement and Detection Areas")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private bool isEnraged = false;
    [SerializeField] private float walkStepInterval;
    [SerializeField] private float runStepInterval;
    [SerializeField] private Vector2 areaToCharge;
    [SerializeField] private Vector2 chargeOffset;
    [SerializeField] private Vector2 areaToAttack;
    [SerializeField] private Vector2 hitOffset;

    [Header("AttackPower and Hit Cooldowns")]
    [SerializeField] private float lightAttackCoolDown;
    [SerializeField] private float lightAttackDamage;
    [SerializeField] private float heavyAttackCoolDown;
    [SerializeField] private float heavyAttackDamage;

    private Transform target;
    private Health targetHealth;

    private Rigidbody2D rb;
    private Health health;
    
    private bool isFacingRight = false;
    private Vector2 vectorToTarget;

    private bool canMove = true;
    private bool canLightAttack = true;
    private bool canHeavyAttack = true;
    
    

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player").transform;
        targetHealth = target.GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        // Used to determine the direction where boss is going.
        vectorToTarget = (target.position - transform.position).normalized;

        // Flip the localscale of the boss.
        if (!isFacingRight && rb.velocity.x > 0.5f)
            Flip();
        else if (isFacingRight && rb.velocity.x < -0.5f)
            Flip();

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

            case RiotState.LightAttack:
                HandleLightAttackState();
                break;

            case RiotState.HeavyAttack:
                HandleHeavyAttackState();
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
        if(!isEnraged && canMove)
        {
            rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * walkingSpeed * Time.deltaTime, 0));
            StartCoroutine(WalkCoolDown());
        }
        else if(isEnraged && canMove)
        {
            rb.AddForce(new Vector2((vectorToTarget.x > 0 ? 1 : -1) * runningSpeed * Time.deltaTime, 0));
            StartCoroutine(WalkCoolDown());
        }
        if(IsTargetInHitRange())
        {
            state = RiotState.LightAttack;
        }
        
    }

    private void HandleShieldChargeState()
    {

    }

    // Simple light attack state where boss swings the weapon. Checks if target was in the hit area before doing damage to it.
    private void HandleLightAttackState()
    {
        if(canLightAttack)
        {
            StartCoroutine(LightAttack());
        }

    }

    private void HandleHeavyAttackState()
    {

    }

    private void HandleTaserShootState()
    {

    }

    private void HandleStunnedState()
    {

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



    private IEnumerator LightAttack()
    {
        canLightAttack = false;
        yield return new WaitForSeconds(lightAttackCoolDown);
        // Deal damage to player if still in range.
        if(IsTargetInHitRange())
        {
            targetHealth.TakeDamage(lightAttackDamage);
            Debug.Log("RiotHit!");
        }
        else
        {
            Debug.Log("Missed.");
            state = RiotState.Moving;
        }

        canLightAttack = true;
    }

    // Overlaps for various checks.
    private bool IsTargetInHitRange()
    {
        return Physics2D.OverlapBox(new Vector2(transform.position.x + (transform.localScale.x * hitOffset.x), transform.position.y + hitOffset.y), areaToAttack, 0, playerLayer);
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x * aggroOffset.x), transform.position.y + aggroOffset.y), aggroDistance);
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
        if(collision.collider.tag == "Player")
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    // State names.
    public enum RiotState
    {
        Idle,
        Moving,
        ShieldCharge,
        LightAttack,
        HeavyAttack,
        TaserShoot,
        Stunned,
        SeedShoot

    }

}
