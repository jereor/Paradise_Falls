using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public static PlayerCombat Instance; // Make instance so we can call this script from animations

    [Header("Melee Variables")]
    [SerializeField] private float lightDamage; // Light hits 1, 2 and 3 = lightDamage + lightDamage/2 (pyoristettyna ylospain) 
    [SerializeField] private float heavyDamage; // Same as above but with different float
    [SerializeField] private float lastHitMultiplier;
    [SerializeField] private float weakPointMultiplier;
    [SerializeField] private float knockbackForceLight;
    [SerializeField] private float knockbackForceHeavy;
    [SerializeField] private float dashSpeed; // Velocity for dash if 0 no dash 
    [SerializeField] private float dashDistance; // Distance of dash
    [SerializeField] private float playerPullForce;
    [SerializeField] private bool kbOnLight;
    [SerializeField] private bool kbOnLightLast;

    [Header("Combo Variables")]
    private bool comboOnCooldown = false; // Boolean of active combo cooldown
    private bool comboActive = false; // Boolean of active combo set
    private int currentComboIndex = 0; // What hit we did last
    private Coroutine comboTimerCoroutine; // Used to stop comboTimer for attack animation duration if we detect input
    [SerializeField] private float comboCooldownTime; // Time between combo sets example: we hit once wait comboTimerCoroutine to finish -> ComboCooldown starts we cannot attack during this

    [Header("Attack Detection Variables")]
    public LayerMask enemyLayer;
    public LayerMask bossLayer;
    public LayerMask bossWeakPointLayer;
    public Transform attackPoint; // Center of the hit point box we draw to check collisions
    public float attackRangeX; // Width of the check box
    public float attackRangeY; // Height

    [Header("Throwing")]
    [SerializeField] private GameObject throwIndicator; // Object used to rotate throwPoint and pointPrefabs
    [SerializeField] private Transform throwPoint; // Point where the weapon will be Instantiated
    [SerializeField] private float defaultThrowingForce;
    [SerializeField] private float maxChargeTime;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistanceBetween;

    [Header("Throw projection points")]
    public GameObject pointPrefab;
    GameObject[] points; // Array of pointsPrefabs Instantiated
    private int numberOfPoints; // Amount should be same as maxDistance we can throw, 1 point = 1 distance unit
    [SerializeField] private float spaceBetweenPoints;
    [SerializeField] private int pointsShown;
    [SerializeField] private float pointScaleRatio;

    [Header("Weapon")]
    [SerializeField] private GameObject meleeWeaponPrefab;
    private GameObject weaponInstance; // Players weapon in scene after throwing used in pull backs
    [SerializeField] private bool isWeaponWielded = false;
    private bool multitoolUnlocked = false;

    [Header("Variables used with pulling player towards the weapon")]
    [SerializeField] private float pullCollisionCounter = 0; // If player hits a collider during the pull, release the pull after a certain time.
    [SerializeField] private GameObject magnetTether; // Magnet tether for the visual representation of the pull.
    [SerializeField] private float timeBeforeRelease = 1f;
    private bool grapplingUnlocked = false;

    // State variables
    private bool throwAimHold; // True if we are holdling Throw input (MouseR)

    public bool heavyHold; // True if we are holding HeavyMelee input (LShift)

    private float? throwButtonPressedTime; // Time when we start throwing
    private float throwChargeStartTime; // Time when we start charging
    private float ratio; // Ratio float to grow desired parameters in same ratio. "One ratio to rule them all"

    Mouse mouse = Mouse.current; // Mouse in use on Unity player

    Camera mainCamera;  // MainCamera
    Ray mousePosRay; // Ray from MainCamera to mouse position in screen (endpoint aka .origin vector is what we can use)

    Vector2 vectorToTarget; // Vector to mousepos from player gameobject

    [Header("Placeholder anim debugs")]
    public bool onIdle = true;
    public bool onTran1 = false;
    public bool onTran2 = false;
    public bool onTran3 = false;
    
    // These will stay 
    public bool canReceiveInputMelee = false; // If this is true we can melee (no attack animation ongoing)
    public bool canReceiveInputThrow = false; // If this is true we can melee (no attack animation ongoing)
    public bool meleeInputReceived = false; // Used in transitions and idle to tell animator to start correct attack if this turns true
    public bool throwInputReceived = false;

    Rigidbody2D rb;
    PlayerMovement movementScript;

    // Used in dash 
    private Vector3 posBeforeDash;
    private bool dash; // State variable if we are currently dashing

    private bool isPlayerBeingPulled; // Is player being pulled

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // MainCamera
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        numberOfPoints = (int)maxDistance;
        // Instantiate points and adjust their look
        InitPoints();

        EnableInputMelee();
        EnableInputThrowAim();
        
        //InputManager();

        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<PlayerMovement>();

        // Check if we have these unlocks (example on loading or debug testing)
        CheckUnlocksOnStart();
    }

    private void CheckUnlocksOnStart()
    {
        // Multitool
        if (Player.Instance.MultitoolUnlocked())
        {
            multitoolUnlocked = true;
            isWeaponWielded = true;
        }
        else
        {
            multitoolUnlocked = false;
            isWeaponWielded = false;
        }
        // Grappling
        if (Player.Instance.GrapplingUnlocked())
            grapplingUnlocked = true;
        else
            grapplingUnlocked = false;
    }

    private void CheckUnlocksOnUpdate()
    {
        // Check only when false save resources
        // Multitool
        if (!multitoolUnlocked && Player.Instance.MultitoolUnlocked() != multitoolUnlocked)
        {
            multitoolUnlocked = Player.Instance.MultitoolUnlocked();
            isWeaponWielded = Player.Instance.MultitoolUnlocked();
        }

        // Grappling
        if (!grapplingUnlocked && Player.Instance.GrapplingUnlocked() != grapplingUnlocked)
            grapplingUnlocked = Player.Instance.GrapplingUnlocked();
    }

    private void Update()
    {
        // Check of we aquire new unlocks
        CheckUnlocksOnUpdate();
    }

    private void FixedUpdate()
    {
        // Update vectorToTarget only when aim button held down
        if(throwAimHold)
            vectorToTarget = new Vector2(mousePosRay.origin.x - gameObject.transform.position.x, mousePosRay.origin.y - gameObject.transform.position.y);

        if (weaponInstance)
        {
            Debug.DrawRay(transform.position, weaponInstance.transform.position - transform.position, Color.red);
        }

        CheckIfMaxDistanceChanged();

        RotateIndicator();

        Throwing();

        CheckMaxDistance();

        CheckAttackDashDistance();

        PullingPlayer();

    }

    // --- INPUT FUNCITONS ---

    // Input from mouse left
    public void Melee(InputAction.CallbackContext context)
    {
        if (!Player.Instance.MultitoolUnlocked()) return;

        // Throw and melee
        if (context.performed && isWeaponWielded && canReceiveInputMelee)
        {
            // Start Throwing
            if (throwAimHold && meleeWeaponPrefab)
            {
                // Set time here since we start charging
                throwButtonPressedTime = Time.time;
            }

            // Input for melee 
            // Melee type checked in Idle and transitions if heavyHold is true or false
            else if (!throwAimHold && !getComboOnCooldown())
            {
                meleeInputReceived = true;
            }
        }

        // Pull (and grappling hook control) 
        else if (context.performed && !isWeaponWielded && weaponInstance != null && weaponInstance.TryGetComponent<MeleeWeapon>(out var weaponScript))
        {
            // Just left click
            if (!throwAimHold)
            {
                Debug.Log("Trying to pull weapon");
                weaponScript.PullWeapon(gameObject);
            }
            // Left click when right is held down
            else if (throwAimHold && grapplingUnlocked)
            {                
                //weaponScript.PullWeapon(gameObject);

                Vector2 vectorToWeapon = weaponInstance.transform.position - transform.position;
                RaycastHit2D hitGround;
                RaycastHit2D hitGrapplePoint;
                hitGround = Physics2D.Raycast(transform.position, vectorToWeapon, vectorToWeapon.magnitude, LayerMask.GetMask("Ground"));
                hitGrapplePoint = Physics2D.Raycast(transform.position, vectorToWeapon, vectorToWeapon.magnitude, LayerMask.GetMask("GrapplePoint"));

                //Sets the collision between the player and weapon false again. Magnet tether becomes active during the flight to the weapon.
                if (hitGround && hitGround.collider.tag == "MeleeWeapon" && !hitGrapplePoint)
                {
                    Debug.Log("Grappling to point!");
                    Physics2D.IgnoreLayerCollision(3, 13);
                    isPlayerBeingPulled = true;
                    pullCollisionCounter = 0f;
                    magnetTether.SetActive(true);
                }
            }
        }

        // Throw on button release
        if(context.canceled && isWeaponWielded && throwAimHold && meleeWeaponPrefab && throwButtonPressedTime != null)
        {
            throwInputReceived = true;

            ThrowWeapon();

            // We don't have weapon anymore
            isWeaponWielded = false;
            throwButtonPressedTime = null;

            // Weapon is thrown hide points
            HideAllProjPoints();
        }
    }

    // Input from left shift
    public void HeavyMelee(InputAction.CallbackContext context)
    {
        if (!Player.Instance.MultitoolUnlocked()) return;

        if (context.performed)
        {
            heavyHold = true;
        }

        if (context.canceled)
        {
            heavyHold = false;
        }
    }

    // Input from mouse right
    public void MeleeAimThrowing(InputAction.CallbackContext context)
    {
        if (!Player.Instance.MultitoolUnlocked()) return;

        if (context.performed && canReceiveInputThrow)
        {
            throwAimHold = true;

            // Show weapon throw min distance direction
            if (isWeaponWielded)
            {
                // Show minDistance amount of points
                ShowProjPoints((int)minDistance);
            }
        }

        if (context.canceled)
        {
            throwAimHold = false;

            // We release aim button hide points
            HideAllProjPoints();
        }
    }

    private void PullingPlayer()
    {
        if (isPlayerBeingPulled)
        {
            Vector3 vectorToTargetWeapon = weaponInstance.transform.position - transform.position;
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0f;
            gameObject.GetComponent<Rigidbody2D>().velocity = vectorToTargetWeapon.normalized * playerPullForce * Time.deltaTime;
        }

    }


    // --- MELEE ---


    // --- COMBO ---
    
    // Called from Transition animation scripts they know their attack animation and how long current transition would be
    public void UpdateCombo(int attackIndex, float transitionTime)
    {
        currentComboIndex = attackIndex;

        // Start timer if this runs out we go cooldown no spamming
        comboTimerCoroutine = StartCoroutine(ComboTimer(transitionTime));
    }

    // Stops timer (kills coroutine) for until started again
    public void StopComboTimer()
    {
        StopCoroutine(comboTimerCoroutine);
    }

    // Calculates time between attacks (attack transition time) if it runs out we set combo on cooldown if we melee before Player.cs calls StopComboTimer() and 
    // timer starts again when LTran# animation starts
    private IEnumerator ComboTimer(float transitionTime)
    {
        yield return new WaitForSeconds(transitionTime);
        // If we go here we didn't melee before attack transition (if we started running transitionTime) ended
        setComboOnCooldown(true);
    }

    private IEnumerator ComboCooldown()
    {
        // We are on cooldown and we don not have combo active
        setComboActive(false);
        yield return new WaitForSeconds(comboCooldownTime);
        // Set this to false so we do not attack instatly when we com out of cooldown
        meleeInputReceived = false;
        // We did our time
        setComboOnCooldown(false);
        // We start from combo index 0 aka first hit of combo
        currentComboIndex = 0;
    }

    public bool getComboOnCooldown()
    {
        return comboOnCooldown;
    }
    public void setComboOnCooldown(bool b)
    {
        // If we set (true) combo on cooldown start coroutine to calculate passed time
        if (b)
            StartCoroutine(ComboCooldown());

        comboOnCooldown = b;
    }

    // Called from Player.cs
    public bool getComboActive()
    {
        return comboActive;
    }
    public void setComboActive(bool b)
    {
        comboActive = b;
    }

    // Used in Player.cs HandleAnimations() to play correct attack animation
    public int getCurrentComboIndex()
    {
        return currentComboIndex;
    }

    // --- DASH ---

    // Put these functions in PlayerMovement????
    public void AttackDash()
    {
        if (dashSpeed != 0)
        {
            posBeforeDash = transform.position;
            dash = true;
            StartCoroutine(DashCounter(dashDistance / dashSpeed));
        }
    }
    // Stops applying force if max distance is not reached due collider in the way
    IEnumerator DashCounter(float time)
    {
        yield return new WaitForSeconds(time);
        dash = false;
    }
    private void CheckAttackDashDistance()
    {
        // We reach the dashDistance
        if (dash && (posBeforeDash - transform.position).magnitude >= dashDistance)
        {
            rb.velocity = Vector2.zero;
            dash = false;
        }
        // Dash needs continuous input since inactive
        else if (dash)
        {
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, rb.velocity.y);
        }
    }

    // Change canReceiveInput boolean to opposite
    public void EnableInputMelee()
    {
        canReceiveInputMelee = true;
    }

    public void DisableInputMelee()
    {
        canReceiveInputMelee = false;
    }

    public void EnableInputThrowAim()
    {
        canReceiveInputThrow = true;
    }

    public void DisableInputThrowAim()
    {
        canReceiveInputThrow = false;
    }

    // Made to own function less copy pasta 
    // colliders to what collider array we are going to be deal dmg
    // kb bool from kbOnLight or kbOnLightLast aka if we wish to knockback
    // layer to identify to what kind of enemy we are dealing dmg
    private void DealDamageTo(Collider2D[] colliders, float dmg, bool kb, float kbForce, LayerMask layer)
    {
        foreach (Collider2D collider in colliders)
        {
            //Debug.Log("wfea");
            if (layer == bossWeakPointLayer || layer == bossLayer)
            {
                // Error check if there isn't Health script attached don't do damage
                if (collider.TryGetComponent<Health>(out var healthScript))
                {
                    // Deal damage
                    healthScript.TakeDamage(dmg);
                    // No knockback on bosses
                }
                // If object doesn't have health script find health script from one of its parent ( there will be only one health script in each boss / part ) 
                else
                {
                    collider.gameObject.GetComponentInParent<Health>().TakeDamage(dmg);
                }
            }
            else if(layer == enemyLayer)
            {
                // Error check if there isn't Health script attached don't do damage
                if (collider.TryGetComponent<Health>(out var healthScript))
                {
                    // Deal damage
                    healthScript.TakeDamage(dmg);

                    // Knockback enemy if true
                    if (kb)
                        Knockback(collider.gameObject, gameObject, kbForce);
                }
            }
        }
    }

    // Call this function on melee animation end when melee visually hits something
    public void DealDamage(int comboIndex, bool heavyHit)
    {
        // Draws a box in scene if objects from enemyLayer overlap with this box store them in hitEnemies array
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRangeX, attackRangeY), 0f, enemyLayer);

        Collider2D[] hitBosses = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRangeX, attackRangeY), 0f, bossLayer);

        Collider2D[] hitBossesWeakPoint = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRangeX, attackRangeY), 0f, bossWeakPointLayer);

        Debug.Log("DEALING DMG!! combo hit: " + comboIndex + " Heavy?: " + heavyHit);
        // Normal combo hits 1 and 2
        if (comboIndex < 3 && !heavyHit)
        {
            // Dealing damage to enemies
            if (hitEnemies.Length != 0)
                DealDamageTo(hitEnemies, lightDamage, kbOnLight, knockbackForceLight, enemyLayer);

            // Dealing damage to bosses 
            // If we hit weakpoint we deal only the amount from weakpoint hit and "skip" checkin hitBosses colliders (prevent from dealing weakpoint + normal damage on one hit)
            if (hitBossesWeakPoint.Length != 0)
            {
                // Deal damage
                // Ceil since example: weakPointMultiplier = 1.5 lightDamage = 1 without rounding dmg = 1.5 with Floor dmg 1 with Ceil dmg = 2
                // most likely not needed since weakPointMult 2x is standard in games 
                DealDamageTo(hitBossesWeakPoint, Mathf.Ceil(lightDamage * weakPointMultiplier), kbOnLight, knockbackForceLight, bossWeakPointLayer);
            }
            else if (hitBosses.Length != 0)
            {
                DealDamageTo(hitBosses, lightDamage, kbOnLight, knockbackForceLight, bossLayer);
            }
        }
        // Normal combo hit 3 aka last hit of combo
        else if (comboIndex == 3 && !heavyHit)
        {
            if (hitEnemies.Length != 0)
            {
                DealDamageTo(hitEnemies, Mathf.Ceil(lightDamage * lastHitMultiplier), kbOnLightLast, knockbackForceLight, enemyLayer);
            }

            if (hitBossesWeakPoint.Length != 0)
            {
                // Deal damage
                DealDamageTo(hitBossesWeakPoint, Mathf.Ceil(lightDamage * weakPointMultiplier * lastHitMultiplier), kbOnLightLast, knockbackForceLight, bossWeakPointLayer);
            }
            else if (hitBosses.Length != 0)
            {
                 DealDamageTo(hitBosses, Mathf.Ceil(lightDamage * lastHitMultiplier), kbOnLightLast, knockbackForceLight, bossLayer);
            }
        }


        // Heavy combo hits 1 and 2
        if (comboIndex < 3 && heavyHit)
        {
            // If there is elements on hitEnemies array go through it
            foreach (Collider2D enemy in hitEnemies)
            {
                // Error check if there isn't Health script attached don't do damage
                if (enemy.TryGetComponent<Health>(out var healthScript))
                {
                    // Deal damage
                    healthScript.TakeDamage(heavyDamage);
                    // Knockback enemy
                    Knockback(enemy.gameObject, gameObject, knockbackForceHeavy);
                }
            }
        }
        // Heavy combo hit 3 aka last hit of combo
        else if (comboIndex == 3 && heavyHit)
        {
            // If there is elements on hitEnemies array go through it
            foreach (Collider2D enemy in hitEnemies)
            {
                // Error check if there isn't Health script attached don't do damage
                if (enemy.TryGetComponent<Health>(out var healthScript))
                {
                    // Deal damage
                    if (heavyDamage % 2 == 1) // Example if heavyDamage = 3 --> 3 / 2 = 1.5 --> Floor(1.5) = 1 total damage 4 OR Ceil(1.5) = 2 total damage 5 
                    {
                        healthScript.TakeDamage(heavyDamage + Mathf.Floor(heavyDamage / 2));
                    }
                    else
                    {
                        healthScript.TakeDamage(heavyDamage + Mathf.Ceil(heavyDamage / 2));
                    }
                    // Knockback enemy
                    Knockback(enemy.gameObject, gameObject, knockbackForceHeavy);
                }
            }
        }
    }

    // Debug info draws hit point in this case cube
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRangeX, attackRangeY, 0f));
    }

    // Small knockback to the target when too close to the enemy unit. Knockback knocks slightly upwards so the friction doesn't stop the target right away.
    private void Knockback(GameObject target, GameObject from, float knockbackForce)
    {
        float pushbackX = target.transform.position.x - from.transform.position.x;
        Vector2 knockbackDirection = new Vector2(pushbackX, Mathf.Abs(pushbackX / 4)).normalized;
        target.GetComponent<Rigidbody2D>().AddForce(knockbackDirection * knockbackForce);
    }


    // --- THROWING ---

    // Instantiate throwing projection points and scale, color, modify them here
    private void InitPoints()
    {
        points = new GameObject[numberOfPoints];

        // Go through the array and set colors of different red and scales
        for (int i = 0; i < numberOfPoints; i++)
        {
            // Instantiate and set parent
            points[i] = Instantiate(pointPrefab, throwPoint.position, Quaternion.identity);
            points[i].transform.parent = throwIndicator.transform;

            // Naming easier to read hierarchy
            points[i].name = points[i].name.Replace("(Clone)", "(" + i + ")");

            // Color
            Color pointC = new Color(1f, 1f - 1f * i / (numberOfPoints - 1), 1f - 1f * i / (numberOfPoints - 1));
            points[i].GetComponent<SpriteRenderer>().color = pointC;

            // Scale 
            if (i != 0)
                points[i].transform.localScale = points[i - 1].transform.localScale * pointScaleRatio;

            points[i].SetActive(false);
        }
    }

    // Calculate the position of next point
    Vector2 PointPosition(float t, Vector2 toTarget)
    {
        // Please don't ask the logic of this calculation it just works via trial and error
        Vector2 position = (Vector2)throwPoint.position + (toTarget.normalized * defaultThrowingForce * t) + 0.5f * Physics2D.gravity * (t * t);
        return position;
    }

    // Rotates indicator to mouse position and adjusts the positions of points[] gameObjects
    private void RotateIndicator()
    {
        if (throwAimHold)
        {
            // Get mouse position from mainCamera ScreenPointToRay
            mousePosRay = mainCamera.ScreenPointToRay(mouse.position.ReadValue());

            //vectorToTarget = new Vector2(mousePosRay.origin.x - gameObject.transform.position.x, mousePosRay.origin.y - gameObject.transform.position.y);

            // Positions of points[]
            for (int i = 0; i < numberOfPoints; i++)
            {
                points[i].transform.position = PointPosition(i * spaceBetweenPoints, vectorToTarget);
            }

            // Check if we need to flip vector, player flip angles are different
            if (gameObject.transform.localScale.x == 1)
                throwIndicator.transform.right = vectorToTarget;
            else
                throwIndicator.transform.right = -vectorToTarget;
        }
    }

    private void Throwing()
    {
        // If we are throwing do these
        if (throwAimHold)
        {
            // We are holding throw and melee button down
            if (Time.time - throwButtonPressedTime <= maxChargeTime)
            {
                throwChargeStartTime = Time.time;

                // Ratio calculation tap ratio = 0 and hold for maxChargeTime = 1
                ratio = (throwChargeStartTime - (float)throwButtonPressedTime) / maxChargeTime;

                // Floor ratio to int to show correct amount of points
                int pointsToShow = Mathf.FloorToInt(ratio * numberOfPoints);
                // Check if we need to show more points since ratio is growing
                if (pointsShown < pointsToShow + 1)
                {
                    points[pointsShown].SetActive(true);
                    pointsShown++;
                }
            }
        }
    }

    // Show amount of points
    private void ShowProjPoints(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            points[i].SetActive(true);
            pointsShown++;
        }
    }

    // Hide
    private void HideAllProjPoints()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            points[i].SetActive(false);
        }
        // Show minDistance amount of projections when aiming
        pointsShown = 0;
    }

    private void CheckMaxDistance()
    {
        // We have no weapon wielded and we have thrown weapon we know the instance
        if (!isWeaponWielded && weaponInstance != null)
        {
            // If weapon is power boosted, stop boost after certain distance
            if (weaponInstance.GetComponent<MeleeWeapon>().isPowerBoosted()
                && (weaponInstance.transform.position - gameObject.transform.position).magnitude >= maxDistanceBetween)
            {
                weaponInstance.GetComponent<MeleeWeapon>().StopPowerBoost();
            }
            // Else if distance between player and weapon is greater than maxDistance
            else if ((weaponInstance.transform.position - gameObject.transform.position).magnitude >= maxDistanceBetween)
            {
                weaponInstance.GetComponent<MeleeWeapon>().PullWeapon(gameObject);
            }
        }

        CheckIfMaxDistanceChanged();
    }

    private void CheckIfMaxDistanceChanged()
    {
        // If we updated maxDistance via pick up or debuff set numberOfPoints to same, aka show 1 pointPrefab per 1 distance unit 
        if (numberOfPoints != (int)maxDistance)
        {
            numberOfPoints = (int)maxDistance;
            InitPoints();
        }
    }

    // Instantiate weaponPrefab and launch it to mouse position
    public void ThrowWeapon()
    {
        // Instantiate meleeWeaponPrefab on attackPoint
        weaponInstance = Instantiate(meleeWeaponPrefab, throwPoint.position, Quaternion.identity);

        weaponInstance.transform.right = vectorToTarget.normalized;

        // Give force to weaponInstance to throw
        if (ratio * maxDistance >= minDistance)
        {
            // Throw with charged maxDistance (hold)
            if (weaponInstance.GetComponent<MeleeWeapon>().getMaxDistance() != maxDistance)
            {
                weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(maxDistance * ratio); // Favor distance set in this script easier upgrade handling
            }
            // Throw with default minDistance (tap)
            else
            {
                weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(weaponInstance.GetComponent<MeleeWeapon>().getMaxDistance() * ratio);
            }
        }
        else
        {
            weaponInstance.GetComponent<MeleeWeapon>().setMaxDistance(minDistance);
        }

        // Give force to vector mousePosRay - gameObjectPos, use default force and adjust length of throw iva MaxDistance calculations above
        weaponInstance.GetComponent<Rigidbody2D>().AddForce(vectorToTarget.normalized * defaultThrowingForce, ForceMode2D.Impulse);
    }

    // Called from Weapon script if pulled or we Interact with weapon
    public void PickUpWeapon()
    {
        //Debug.Log("Picked weapon");
        isWeaponWielded = true;

        //Deactivate the tether. Weapon reached.
        magnetTether.SetActive(false);
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        // If player gets stuck during a pull, release.
        if (isPlayerBeingPulled)
        {
            pullCollisionCounter += Time.deltaTime;
            if (pullCollisionCounter >= timeBeforeRelease)
            {
                isPlayerBeingPulled = false;
                magnetTether.SetActive(false);
                gameObject.GetComponent<Rigidbody2D>().gravityScale = 5f;
                gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                pullCollisionCounter = 0;
            }
        }
    }

    // --- SAVING / LOADING ---

    public float getLightDamage()
    {
        return lightDamage;
    }

    public void setLightDamage(float dmg)
    {
        lightDamage = dmg;
    }

    public float getThrowChargeTime()
    {
        return maxChargeTime;
    }

    public void setThrowChargeTime(float time)
    {
        maxChargeTime = time;
    }


    // --- GET / SET ---

    public bool getWeaponWielded()
    {
        return isWeaponWielded;
    }

    public void setWeaponWielded(bool wield)
    {
        isWeaponWielded = wield;
    }


    public GameObject getWeaponInstance()
    {
        return weaponInstance;
    }

    public bool getIsPlayerBeingPulled()
    {
        return isPlayerBeingPulled;
    }

    public void setIsPlayerBeingPulled(bool isPulled)
    {
        isPlayerBeingPulled = isPulled;
    }

    public bool getThrowAiming()
    {
        return throwAimHold;
    }

    public Vector2 getVectorToMouse()
    {
        return vectorToTarget;
    }


    // --- UPGRADES ---
    // Called from pickups / small upgrades

    public void UpgradeLightDamage(float dmg)
    {
        lightDamage += dmg;
    }

    public void UpgradeThrowDamage(float dmg)
    {
        meleeWeaponPrefab.GetComponent<MeleeWeapon>().UpgradeThrowDamage(dmg);
    }

    public void UpgradePullDamage(float dmg)
    {
        meleeWeaponPrefab.GetComponent<MeleeWeapon>().UpgradePullDamage(dmg);
    }

    public void UpgradePowerBoostedDamage(float dmg)
    {
        meleeWeaponPrefab.GetComponent<MeleeWeapon>().UpgradePowerBoostedDamage(dmg);
    }

    public void UpgradeThrowMaxChargeTime(float time)
    {
        maxChargeTime -= time;
    }
}