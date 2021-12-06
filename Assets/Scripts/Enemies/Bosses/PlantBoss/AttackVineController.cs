using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackVineController : MonoBehaviour
{
    private BigPlantController plantController;

    private GameObject plantBoss;

    private GameObject target;
    private Rigidbody2D targetRB;
    private Health targetHealth;

    private Animator animator;

    private Vector2 velocityPlayer;

    [SerializeField] private float attackVineStretchDuration; // Duration the vine changes the local scale.
    [SerializeField] private float attackVineRotateDuration; // Duration when it turns itself towards the player.
    [SerializeField] private float attackVineMoveDuration; // Duration the vine moves to reveal itself from the wall.
    [SerializeField] private float attackVineWaitTime; // Time it waits after locking it's charge direction.
    [SerializeField] private float attackVineStretchAmount; // How far the vine reaches.
    [SerializeField] private float attackVineMoveAmount; // How far from the spawn point it moves.
    [SerializeField] private float vineSpeed = 1; // Local vine speed used in all other situations except when boss is spawning them personally.

    public bool isAttackVineActivated = false;
    public bool isRotatingTowardsTarget = false;

    [SerializeField] private float knockbackForce; // Knockback force it applies to the player when hit.
    private bool knockbackOnCooldown = false;
    // Start is called before the first frame update
    void Start()
    {
        plantBoss = GameObject.Find("PlantBoss");
        plantController = plantBoss.GetComponent<BigPlantController>();
        target = GameObject.Find("Player");
        targetRB = target.GetComponent<Rigidbody2D>();
        targetHealth = target.GetComponent<Health>();
        animator = GetComponent<Animator>();


    }

    // Update is called once per frame
    void Update()
    {
        if (!isAttackVineActivated)
        {
            StartCoroutine(SpawnVine());
            isRotatingTowardsTarget = true;
        }

        if (isRotatingTowardsTarget)
            RotateVineTowardsTheTarget();
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

    private void RotateVineTowardsTheTarget()
    {
        // Rotating object to point player
        Vector3 vectorToTarget = target.transform.position - transform.parent.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, q, Time.deltaTime);
    }

    // THe whole behaviour for the vine except the rotation.
    private IEnumerator SpawnVine()
    {
        // If the boss state is Angri, the speed is got from the boss object. Otherwise the script uses the speed given to it in inspector.
        if(plantController.state == BigPlantController.PlantState.Angri)
            vineSpeed = plantController.GetVineSpeed();

        isAttackVineActivated = true;
        //transform.position = new Vector2(Random.Range(plantBoss.transform.position.x - 5, plantBoss.transform.position.x + 5), plantBoss.transform.position.y + 15);
        transform.parent.DOMove((target.transform.position - transform.parent.position).normalized * attackVineMoveAmount + transform.parent.position, attackVineMoveDuration * vineSpeed); // Moves the vine towards the player for the given amount.
        yield return new WaitForSeconds(attackVineRotateDuration * vineSpeed);
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        isRotatingTowardsTarget = false; // Stops the object rotation meaning that it's lovked in place.
        animator.SetBool("Charge", true);
        Vector2 attackDirection = target.transform.position; // Gets the player's last position.
        yield return new WaitForSeconds(attackVineWaitTime * vineSpeed);
        transform.parent.DOScaleY(attackVineStretchAmount, attackVineStretchDuration * vineSpeed); // After waiting the given time it stretches in the direction given.
        yield return new WaitForSeconds(attackVineStretchDuration * vineSpeed);
        transform.parent.DOScaleY(1, attackVineStretchDuration * vineSpeed); // And back to normal.
        yield return new WaitForSeconds(attackVineStretchDuration * vineSpeed);
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        animator.SetBool("Charge", false);
        transform.parent.DOMove(-((target.transform.position - transform.parent.position).normalized * attackVineMoveAmount) + transform.parent.position, attackVineMoveDuration * vineSpeed); // Moves the vine back where it came from.

        yield return new WaitForSeconds(attackVineMoveDuration * vineSpeed);
        isAttackVineActivated = false;
        plantController.attackVineInstances.Remove(gameObject.transform.parent.gameObject); // Remove the vine from the list. Destroy the game object afterwards.
        Destroy(gameObject.transform.parent.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // AttackVine hits the boss. Stun state activated.
        if (collision.transform.name == "PlantBoss" && plantController.state != BigPlantController.PlantState.Stunned)
        {
            plantController.state = BigPlantController.PlantState.Stunned;
        }

        if (collision.transform.name == "Player" && !knockbackOnCooldown)
        {
            PlayerPushback();
            targetHealth.TakeDamage(1);
        }
    }
}
