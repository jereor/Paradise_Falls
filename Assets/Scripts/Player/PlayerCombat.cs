using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    [Header("Player Variables")]
    [SerializeField] private float meleeDamage;
    [SerializeField] private float meleeChargedDamage;
    [SerializeField] private float meleeAttackRate;
    [SerializeField] private float maxChargeTime;
    [SerializeField] private float throwingForce;

    [Header("Attack Detection Variables")]
    public LayerMask enemyLayer;
    public Transform attackPoint;
    public float attackRangeX;
    public float attackRangeY;
    
    [Header("Weapon")]
    [SerializeField] private GameObject meleeWeaponPrefab;
    [SerializeField] private GameObject weaponInstance;
    [SerializeField] private bool isWeaponWielded;

    // State variables
    private float? meleeButtonPressedTime;
    private float lastTimeMeleed;
    private bool meleeThrow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time - meleeButtonPressedTime >= maxChargeTime)
        {
            Debug.Log("Fully Charged");
        }
    }

    public void Melee(InputAction.CallbackContext context)
    {
        if(context.performed && isWeaponWielded && meleeThrow)
        {
            weaponInstance = Instantiate(meleeWeaponPrefab, attackPoint.position, Quaternion.identity);
            weaponInstance.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Sign(transform.localScale.x) * throwingForce, 0), ForceMode2D.Impulse);
            isWeaponWielded = false;
            
        }
        else if (context.performed && isWeaponWielded && Time.time - lastTimeMeleed >= meleeAttackRate && !meleeThrow)
        {
            //Debug.Log("Started melee charge");
            meleeButtonPressedTime = Time.time;
        }



        if (context.canceled && isWeaponWielded && Time.time - lastTimeMeleed >= meleeAttackRate && !meleeThrow)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRangeX, attackRangeY), 0f, enemyLayer);

            if (Time.time - meleeButtonPressedTime >= maxChargeTime)
            {
                Debug.Log("Charged attack");

                foreach (Collider2D enemy in hitEnemies)
                {
                    if (enemy.GetComponent<Health>() != null)
                        enemy.GetComponent<Health>().TakeDamage(meleeChargedDamage);
                }
                
            }
            else
            {
                Debug.Log("Normal attack");

                foreach (Collider2D enemy in hitEnemies)
                {
                    if (enemy.GetComponent<Health>() != null)
                        enemy.GetComponent<Health>().TakeDamage(meleeDamage);
                }
                
            }
            
            meleeButtonPressedTime = null;
            lastTimeMeleed = Time.time;
        }
    }

    public void MeleeAimThrowing(InputAction.CallbackContext context)
    {
        if(context.performed && isWeaponWielded)
        {
            meleeThrow = true;
            Debug.Log("Throw ini");
        }

        if (context.canceled && isWeaponWielded)
        {
            meleeThrow = false;
            Debug.Log("Throw cancel");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 0f));
    }

    public void PickUpWeapon()
    {
        Debug.Log("Picked weapon");
        isWeaponWielded = true;
    }
}
