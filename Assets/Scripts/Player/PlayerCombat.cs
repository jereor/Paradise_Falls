using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Variables")]
    [SerializeField] private float meleeDamage; // Normal hits currentComboHit 1, 2
    [SerializeField] private float meleeComboLastDamage; // Last hit currentComboHit 3
    [SerializeField] private float knockbackForceNormal;
    [SerializeField] private float knockbackForceLast;


    [SerializeField] private float meleeAttackRate; // Time between combo attacks
    [SerializeField] private float comboCooldown; // Timer of when we need to start new combo
    [SerializeField] private float comboBetweenTimer; // If this runs out we set combo on cooldown

    [SerializeField] private float throwingForce;

    [Header("Attack Detection Variables")]
    public LayerMask enemyLayer;
    public Transform attackPoint; // Center of the hit point box we draw to check collisions
    public float attackRangeX; // Width of the check box
    public float attackRangeY; // Heigth

    [Header("Weapon")]
    [SerializeField] private GameObject meleeWeaponPrefab;
    [SerializeField] private GameObject weaponInstance;
    [SerializeField] private bool isWeaponWielded; 

    // State variables
    //private float? meleeButtonPressedTime;
    private float comboEndTime = 0; // Set here to zero so we are sure it starts from zero
    private float lastTimeMeleed = 0;
    private bool meleeThrow; // True if we are holdling Throw input
    private int currentComboHit = 1; // Combo counter 1 normal, 2 normal, 3 last hit -> comboCooldown -> Combo can be done again

    private bool comboOnCooldown; // boolean to check if we can attack
    public bool comboOnGoing; // Used in ComboCounter() calculates if we aren't attacking before comboBetweenTimer then we reset combo

    // Start is called before the first frame update
    void Start()
    {
        // We don't have to wait cooldown on start
        comboEndTime = comboEndTime - comboCooldown;
    }

    private void Update()
    {
        // Calculates when we can combo and if combo will end
        ComboCounter();
    }

    private void ComboCounter()
    {
        if (Time.time - comboEndTime > comboCooldown)
        {
            if (comboOnCooldown)
                comboOnCooldown = false;
            
            if (comboOnGoing)
            {
                if (Time.time - lastTimeMeleed > comboBetweenTimer)
                {
                    //Debug.Log("End Combo due no attack");
                    ResetCombo();
                }
            }
        }
    }

    private bool CheckAttackRate()
    {
        return (Time.time - lastTimeMeleed >= meleeAttackRate);
    }

    public void Melee(InputAction.CallbackContext context)
    {
        // Throwing
        if (context.performed && isWeaponWielded && meleeThrow)
        {
            // Instantiate meleeWeaponPrefab on attackPoint
            weaponInstance = Instantiate(meleeWeaponPrefab, attackPoint.position, Quaternion.identity);
            // Give force to weaponInstance to throw
            weaponInstance.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Sign(transform.localScale.x) * throwingForce, 0), ForceMode2D.Impulse);
            // We don't have weapon anymore
            isWeaponWielded = false;
        }
        // Melee
        else if (context.performed && isWeaponWielded && CheckAttackRate() && !meleeThrow && !comboOnCooldown)
        {
            // We start combo if we havent one started
            if(!comboOnGoing)
                comboOnGoing = true;

            // Draws box and if this overlaps with object on enemyLayer, stores their colliders in array
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRangeX, attackRangeY), 0f, enemyLayer);

            // First and second attack
            if (currentComboHit < 3)
            {
                Debug.Log("Combo hit: " + currentComboHit);

                // If there is elements on hitEnemies array go through it
                foreach (Collider2D enemy in hitEnemies)
                {
                    // Error check if there isn't Health script attached don't do damage
                    if (enemy.GetComponent<Health>() != null)
                    {
                        // Deal damage
                        enemy.GetComponent<Health>().TakeDamage(meleeDamage);
                        // Knockback enemy
                        Knockback(enemy.gameObject, gameObject, knockbackForceNormal);
                    }
                }
                // Next combo attack
                currentComboHit++;
            }
            // Third attack or if bug happens (currentComboHit = 4) this will deal 3. damage and reset combo
            else
            {
                Debug.Log("Last hit: " + currentComboHit);

                foreach (Collider2D enemy in hitEnemies)
                {
                    if (enemy.GetComponent<Health>() != null)
                    {
                        enemy.GetComponent<Health>().TakeDamage(meleeComboLastDamage);
                        Knockback(enemy.gameObject, gameObject, knockbackForceLast);
                    }
                }
                //Debug.Log("End Combo due last attack");
                ResetCombo();
            }
            //meleeButtonPressedTime = null;
            lastTimeMeleed = Time.time;
        }
        // Pull weapon if thrown
        else if(context.performed && !isWeaponWielded && CheckAttackRate())
        {
            Debug.Log("Trying to pull weapon");
            weaponInstance.GetComponent<MeleeWeapon>().PullWeapon(gameObject);          
        }
    }

    public void MeleeAimThrowing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            meleeThrow = true;
            //Debug.Log("Throw ini");
        }

        if (context.canceled)
        {
            meleeThrow = false;
            //Debug.Log("Throw cancel");
        }
    }

    // Debug info draws hit point in this case cube
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 0f));
    }

    // Sets correct variables to disable melee attacks
    private void ResetCombo()
    {
        comboOnCooldown = true;
        comboOnGoing = false;
        currentComboHit = 1;
        comboEndTime = Time.time;
    }

    // Small knockback to the target when too close to the enemy unit. Knockback knocks slightly upwards so the friction doesn't stop the target right away.
    private void Knockback(GameObject target, GameObject from, float knockbackForce)
    {
        float pushbackX = target.transform.position.x - from.transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Mathf.Abs(pushbackX / 4)).normalized;
        target.GetComponent<Rigidbody2D>().AddForce(knockbackDirection * knockbackForce);
    }

    // Called from Weapon script if pulled or we Interact with weapon
    public void PickUpWeapon()
    {
        Debug.Log("Picked weapon");
        isWeaponWielded = true;
    }
}