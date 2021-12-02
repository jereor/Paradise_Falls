using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GrappleVineController : MonoBehaviour
{
    private BigPlantController plantController;

    private GameObject plantBoss;

    private GameObject target;
    private Rigidbody2D targetRB;

    [SerializeField] LayerMask groundLayer;

    private Animator animator;

    private Vector2 velocityPlayer;
    private Vector2 vectorToWall;
    private Vector2 endPointStretchedVine;
    [SerializeField] private Transform vineEndPoint;
    RaycastHit2D hit;

    private Quaternion grappleVineRotation;

    [SerializeField] private float grappleVineStretchDuration; // Duration the vine changes the local scale.
    [SerializeField] private float grappleVineRotateDuration; // Duration when it turns itself towards the player.
    [SerializeField] private float grappleVineMoveDuration; // Duration the vine moves to reveal itself from the wall.
    [SerializeField] private float grappleVineWaitTime; // Time it waits after locking it's charge direction.
    [SerializeField] private float grappleVineStretchAmount; // How far the vine reaches.
    [SerializeField] private float grappleVineMoveAmount; // How far from the spawn point it moves.
    [SerializeField] private float vineSpeed = 1; // Local vine speed used in all other situations except when boss is spawning them personally.
    [SerializeField] private float plantBossTransportSpeed;

    [SerializeField] private bool isAttackVineActivated = false;
    [SerializeField] private bool isRotatingTowardsTarget = false;
    [SerializeField] private bool rotateTowardsTarget = true;
    [SerializeField] private bool bossCanBeTransported = false;

    [SerializeField] private float knockbackForce; // Knockback force it applies to the player when hit.
    private bool knockbackOnCooldown = false;
    // Start is called before the first frame update
    void Start()
    {
        plantBoss = GameObject.Find("PlantBoss");
        plantController = plantBoss.GetComponent<BigPlantController>();
        target = GameObject.Find("Player");
        targetRB = target.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        transform.parent.localScale = new Vector2(1, 0.1f);

        Vector3 vectorToTarget = target.transform.position - transform.parent.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, q, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAttackVineActivated)
        {
            StartCoroutine(SpawnVine());
            isRotatingTowardsTarget = true;
        }

        if (isRotatingTowardsTarget && rotateTowardsTarget)
            RotateVineTowardsTheTarget();

        if(bossCanBeTransported)
        {
            //plantBoss.transform.position = Vector2.MoveTowards(plantBoss.transform.position, endPointStretchedVine, Time.deltaTime * plantBossTransportSpeed);
            plantBoss.transform.DOJump(vectorToWall, 0, 0, 2);
            bossCanBeTransported = false;
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

    private void RotateVineTowardsTheTarget()
    {
        //if(target.transform.position.y >= plantBoss.transform.position.y)
        //{
            // Rotating object to point player
            Vector3 vectorToTarget = target.transform.position - transform.parent.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, q, 1);
        //}
        //else if(target.transform.position.y < plantBoss.transform.position.y)
        //{
        //    // Rotating object to point above the player because of the plant bosses' big collider.
        //    Vector3 vectorToTarget = new Vector2(target.transform.position.x - transform.parent.position.x, 0);
        //    float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        //    Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        //    transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, q, Time.deltaTime);
        //}

    }

    // THe whole behaviour for the vine except the rotation.
    private IEnumerator SpawnVine()
    {
        //If the boss state is Angri, the speed is got from the boss object.Otherwise the script uses the speed given to it in inspector.
        //if (plantController.state == BigPlantController.PlantState.Angri)
        //    vineSpeed = plantController.GetVineSpeed();

        isAttackVineActivated = true;
        // transform.parent.DOMove((target.transform.position - transform.parent.position).normalized * grappleVineMoveAmount + transform.parent.position, grappleVineMoveDuration * vineSpeed); // Moves the vine towards the player for the given amount.
        transform.parent.DOScaleY(1,3);
        yield return new WaitForSeconds(3);

        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        isRotatingTowardsTarget = false; // Stops the object rotation meaning that it's locked in place.
        animator.SetBool("Charge", true);
        vectorToWall = target.transform.position;
        hit = Physics2D.Raycast(plantBoss.transform.position, (target.transform.position - plantBoss.transform.position).normalized, 0, groundLayer);
        transform.parent.DOScaleY(0.5f, grappleVineWaitTime * vineSpeed);
        

        Debug.DrawRay(plantBoss.transform.position, vectorToWall, Color.red);
        //Vector2 attackDirection = target.transform.position; // Gets the player's last position.
        yield return new WaitForSeconds(grappleVineWaitTime * vineSpeed);

        transform.parent.DOScaleY(grappleVineStretchAmount, grappleVineStretchDuration * vineSpeed); // After waiting the given time it stretches in the direction given.
        yield return new WaitForSeconds(grappleVineStretchDuration * vineSpeed);

        endPointStretchedVine = vineEndPoint.position;
        bossCanBeTransported = true;
        transform.parent.DOScaleY(0.1f, grappleVineStretchDuration * vineSpeed); // And back to normal.
        yield return new WaitForSeconds(grappleVineStretchDuration * vineSpeed);

        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        animator.SetBool("Charge", false);
        //transform.parent.DOMove(-((target.transform.position - transform.parent.position).normalized * grappleVineMoveAmount) + transform.parent.position, grappleVineMoveDuration * vineSpeed); // Moves the vine back where it came from.
        yield return new WaitForSeconds(grappleVineMoveDuration * vineSpeed);
        yield return new WaitForSeconds(1);

        isAttackVineActivated = false;
        plantBoss.GetComponent<BigPlantController>().SetIsCharging(false);
        plantBoss.GetComponent<BigPlantController>().SetHasCharged(true);
        plantController.grappleVineInstances.Remove(gameObject.transform.parent.gameObject); // Remove the vine from the list. Destroy the game object afterwards.
        Destroy(gameObject.transform.parent.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // AttackVine hits the boss. Stun state activated.
        //if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //{
        //    bossCanBeTransported = false;
        //}

        if (collision.transform.name == "Player" && !knockbackOnCooldown)
        {
            PlayerPushback();
        }
    }
}
