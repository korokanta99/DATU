using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class MessyController : MonoBehaviour
{
    [Header("Horizontal Movement Settings:")]
    [SerializeField] private float walkSpeed = 1; //sets the player's movement speed on the ground
    [Space(5)]



    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 45f; //sets how hight the player can jump

    private int jumpBufferCounter = 0; //stores the jump button input
    [SerializeField] private int jumpBufferFrames; //sets the max amount of frames the jump buffer input is stored

    private float coyoteTimeCounter = 0; //stores the Grounded() bool
    [SerializeField] private float coyoteTime; ////sets the max amount of frames the Grounded() bool is stored

    //Disabled airJumping
    //public int airJumpCounter = 0; //keeps track of how many times the player has jumped in the air
    //[SerializeField] private int maxAirJumps; //the max no. of air jumps

    private float gravity; //stores the gravity scale at start
    [Space(5)]



    [Header("Ground Check Settings:")]
    [SerializeField] private Transform groundCheckPoint; //point at which ground check happens
    [SerializeField] private float groundCheckY = 0.2f; //how far down from ground chekc point is Grounded() checked
    [SerializeField] private float groundCheckX = 0.5f; //how far horizontally from ground chekc point to the edge of the player is
    [SerializeField] private LayerMask whatIsGround; //sets the ground layer
    [Space(5)]



    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed; //speed of the dash
    [SerializeField] private float dashTime; //amount of time spent dashing
    [SerializeField] private float dashCooldown; //amount of time between dashes
    [SerializeField] GameObject dashEffect;
    private bool canDash = true, dashed;
    [Space(5)]

    [Header("Attack Settings:")]
    [SerializeField] private Transform SideAttackTransform; //the middle of the side attack area
    [SerializeField] private Vector2 SideAttackArea; //how large the area of side attack is

    [SerializeField] private Transform UpAttackTransform; //the middle of the up attack area
    [SerializeField] private Vector2 UpAttackArea; //how large the area of side attack is

    [SerializeField] private Transform DownAttackTransform; //the middle of the down attack area
    [SerializeField] private Vector2 DownAttackArea; //how large the area of down attack is

    [SerializeField] private LayerMask attackableLayer; //the layer the player can attack and recoil off of

    [SerializeField] private float attackTimerLimit = 0.25f;
    private float attackTimer = 0f;

    [SerializeField] private int damage1 = 1; //the damage the player does to an enemy
    [SerializeField] private int damage2 = 1; //the damage the player does to an enemy
    [SerializeField] private int damage3 = 2; //the damage the player does to an enemy

    [SerializeField] private GameObject slashEffect1; //the effect of the slash 1
    [SerializeField] private GameObject slashEffect2; //the effect of the slash 2

    //Handling the amount of attack types, and cycling between them
    private int AttackCounter = 0;
    private int AttackCounterPlusOne;
    private int MaxAttack = 3;

    //Resets attack type after timer has gone
    [SerializeField] private float ComboTimer = 0f;
    [SerializeField] private float ComboTimerLimit = 1f;
    private bool comboActive = false;

    [Space(5)]


    [Header("Recoil Settings:")]
    [SerializeField] private int recoilXSteps = 5; //how many FixedUpdates() the player recoils horizontally for
    [SerializeField] private int recoilYSteps = 5; //how many FixedUpdates() the player recoils vertically for

    [SerializeField] private float recoilXSpeed = 100; //the speed of horizontal recoil
    [SerializeField] private float recoilYSpeed = 100; //the speed of vertical recoil

    private int stepsXRecoiled, stepsYRecoiled; //the no. of steps recoiled horizontally and verticall
    [Space(5)]

    [Header("Projectile Ability")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileCooldown = 3f;

    private bool canUseProjectile = true;
    private bool aiming = false;
    private GameObject spiritObj;
    [Space(5)]

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    [Space(5)]

    [Header("Spirits")]
    [SerializeField] GameObject ProjectileGod;
    [SerializeField] GameObject DashGod;
    [SerializeField] GameObject HealingGod;

    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint; // assign the start of the stage
    [SerializeField] private Transform fallThresholdY; // y position considered "fall off map"
    private bool isDead = false;
    AudioManager audioManager;

    [Header("Heal Settings")]
    private bool canHeal = true;

    private int healAmount = 2;

    [SerializeField] float HealTimer = 10f;

    [Header("Debug Settings")]
    // [SerializeField] private bool debugTextOn = true;

    [Space(5)]

    [HideInInspector] public PlayerStateList pState;
    private Transform tf;
    private Animator anim;
    private Rigidbody2D rb;
    private Camera mainCam;
    AttackData[] attackType;

    private float scale;

    //Input Variables
    private float xAxis, yAxis;
    private bool attack = false;
    private bool block = false;
    public static MessyController Instance { get; private set; }
    private void Awake()
    {

        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        health = maxHealth;

        
    }

    // Start is called before the first frame update
    void Start()
    {
        audioManager = AudioManager.Instance;

        respawnPoint.transform.SetParent(respawnPoint.transform, true);
        fallThresholdY.transform.SetParent(fallThresholdY.transform, true);
        pState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();

        tf = GetComponent<Transform>();

        anim = GetComponent<Animator>();

        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        gravity = rb.gravityScale;
        scale = tf.transform.localScale.x;

        //Declaring a struct to cycle between different attack properties
        attackType = new AttackData[]
        {
            new AttackData(damage1, slashEffect1, "Attack1", audioManager.attack),
            new AttackData(damage2, slashEffect1, "Attack2", audioManager.attack),
            new AttackData(damage3, slashEffect2, "Attack3", audioManager.attack2),
        };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update is called once per frame
    void Update()
    {

        if (OptionsManager.Instance != null && OptionsManager.Instance.IsMenuOpen)
        return;


        if (isDead) return;

        if (transform.position.y < fallThresholdY.position.y || health <= 0)
        {
            Die();
            return;
        }
        GetInputs();
        UpdateJumpVariables();

        if (pState.dashing) return;
        Flip();


        Move();
        if (pState.canJump) Jump();
        StartDash();
        Attack();
        JumpAttack();

        if (comboActive)
        {
            ComboTimer += Time.deltaTime;
            if (ComboTimer >= ComboTimerLimit)
            {
                comboActive = false;
                AttackCounter = 0;

                anim.SetBool("Attack1", false);
                anim.SetBool("Attack2", false);
                anim.SetBool("Attack3", false);
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && canHeal)
        {
            Heal(healAmount); // heals 2 HP
        }

        HandleProjectileMode();

        // debugText.text = $"Health: {health}\n Can Dash: {canDash} \nCan Throw: {canUseProjectile} \nCan Move: {pState.canMove}\n";

    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        rb.linearVelocity = Vector2.zero;       // Stop movement
        anim.SetTrigger("Die");           // Play death animation

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        // Wait for death animation to finish
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        // Show "Game Over" UI (optional)
        // GameOverUI.SetActive(true);

        // Reset player
        transform.position = respawnPoint.position;
        isDead = false;
        health = maxHealth;               // restore health
        anim.SetTrigger("Idle");          // reset animation
    }

    private void FixedUpdate()
    {
        anim.SetFloat("Velocity", rb.linearVelocity.y);

        if (pState.dashing) return;

        if (Grounded() && pState.GroundLock)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        
            
        Recoil();
    }

    void GetInputs()
    {
        if (pState.canMove)
            xAxis = Input.GetAxisRaw("Horizontal");

        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetMouseButtonDown(0);
        block = Input.GetMouseButtonDown(1);
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-scale, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(scale, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    private void Move()
    { 
        rb.linearVelocity = new Vector2(walkSpeed * xAxis, rb.linearVelocity.y);
        if(!pState.canAttack || pState.canMove) anim.SetBool("Walking", rb.linearVelocity.x != 0 && Grounded());
    }

    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            audioManager.PlaySFX(audioManager.dash);
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        pState.invincible = true;
        anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0);

        Vector3 shoulderOffset = new Vector3(-0.5f * (pState.lookingRight ? 1 : -1), 1f, 0);
                spiritObj = SummonSpirit(DashGod, shoulderOffset);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = gravity;
        pState.dashing = false;
        pState.invincible = false;
        Destroy(spiritObj, 0.5f);


        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Block()
    {
        if (block)
        {
        anim.SetBool("Blocking", true);
        pState.Blocking = true;
        } else
        {
            anim.SetBool("Blocking", false);
            pState.Blocking = false;
        }

    }

    IEnumerator AttackCooldown()
    {
        attackTimer = 0f;

        while (attackTimer <= attackTimerLimit)
        {

            pState.canAttack = false;
            pState.canMove = false;
            attackTimer += Time.deltaTime;
            yield return null;
        }

        
        pState.canMove = true;
        pState.canAttack = true;

    }

    IEnumerator Groundlock()
    {
        float timeSinceAttack = 0f;

        while (timeSinceAttack <= attackTimerLimit)
        {
            pState.GroundLock = true;
            timeSinceAttack += Time.deltaTime;
            yield return null;
        }
        
        pState.GroundLock = false;
    }

    void Attack()
    {
        if (attack && pState.canAttack && !pState.jumping)
        {

            StartCoroutine(AttackCooldown());
            StartCoroutine(Groundlock());

            if (!comboActive)
            {
                comboActive = true;
                ComboTimer = 0f;
            }
            else
            {
                ComboTimer = 0f; // reset combo timer when chaining attacks
            }

            ComboTimer = 0f;

            //Debug.Log($"AttackCounter = {AttackCounter}, animation = {attackType[AttackCounter].animation}");
            anim.SetBool(attackType[AttackCounter].animation, true);
            audioManager.PlaySFX(attackType[AttackCounter].sfx);
            AttackCounterPlusOne = AttackCounter + 1;


            //Turns other animations off
            for (int i = 0; i < attackType.Length - 1; i++)
            {
                anim.SetBool(attackType[(AttackCounterPlusOne) % attackType.Length].animation, false);
                AttackCounterPlusOne++;
            }
            int damageType = attackType[AttackCounter].damage;
            GameObject slashType = attackType[AttackCounter].effect;

                Hit(damageType, SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
                SlashEffectAtAngle(slashType, 0, SideAttackTransform);

            AttackCounter++;
        }
        if (AttackCounter >= MaxAttack) AttackCounter = 0;
        //if (timeSinceAttck >= timeBetweenAttack) AttackCounter = 0;

    }

    void JumpAttack()
    {
        if (attack && pState.canAttack && pState.jumping)
        {

            StartCoroutine(AttackCooldown());

            anim.SetTrigger("JumpAttack");

            int damageType = 1;

            audioManager.PlaySFX(audioManager.attack);


            if (yAxis == 0)
            {
                Hit(damageType, SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
                SlashEffectAtAngle(slashEffect1, 0, SideAttackTransform);

                Debug.Log("Side");
            }
            else if (yAxis > 0)
            {
                Hit(damageType, UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
                SlashEffectAtAngle(slashEffect1, 90, UpAttackTransform);
                Debug.Log("Up");
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(damageType, DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
                SlashEffectAtAngle(slashEffect1, -90, DownAttackTransform);
                Debug.Log("Down");
            }
        }
    }

    void Hit(int damage, Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                objectsToHit[i].GetComponent<Enemy>().EnemyHit
                    (damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }
        }
    }
   void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        GameObject slash = Instantiate(_slashEffect, _attackTransform.position, Quaternion.identity);
        slash.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        slash.transform.localScale = new Vector2(_slashEffect.transform.localScale.x, _slashEffect.transform.localScale.y);
    }
    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.linearVelocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.linearVelocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, recoilYSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -recoilYSpeed);
            }
            //airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        //stop recoil
        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }
    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }
    public void TakeDamage(float _damage)
    {

            health -= Mathf.RoundToInt(_damage);
            StartCoroutine(StopTakingDamage());
            audioManager.PlaySFX(audioManager.hurt);
        
       
    }
    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        anim.SetTrigger("TakeDamage");
        ClampHealth();

        yield return new WaitForSeconds(2f);
        pState.invincible = false;
    }
    void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }
    public bool Grounded()
    {

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb.linearVelocity.y > 0) return false;
        
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Jump()
    {

        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {

                //Terresquall method
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);

                audioManager.PlaySFX(audioManager.jump);

                pState.jumping = true;
            }
            //Disabled airjump
            //else if(!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            //{
            //    pState.jumping = true;

            //    airJumpCounter++;

            //    //Tutorialvania method, doesn't work with max airJumps
            //    //rb.AddForce(transform.up * jumpForce * 100);

            //    //Terresquall method
            //    rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
            //}
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

            pState.jumping = false;
        }

        anim.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }


    void HandleProjectileMode()
    {
        if (Input.GetKey(KeyCode.E) && canUseProjectile)
        {
            if (!aiming)
            {
                aiming = true;
                Vector3 shoulderOffset = new Vector3(-0.5f * (pState.lookingRight ? 1 : -1), 1f, 0);
                spiritObj = SummonSpirit(ProjectileGod, shoulderOffset);
            }

            Vector3 mousePos = mainCam.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                Mathf.Abs(mainCam.transform.position.z)
            ));
            mousePos.z = 0f;
            Vector2 aimDir = (mousePos - spiritObj.transform.position).normalized;

            float rotZ = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

            spiritObj.transform.rotation = Quaternion.Euler(0, 0, rotZ);


            if (Input.GetMouseButtonDown(0))
            {
                FireProjectile(aimDir);
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            aiming = false;
            if (spiritObj != null) Destroy(spiritObj);
        }
    }

    void FireProjectile(Vector2 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
        prb.linearVelocity = direction * projectileSpeed;

        audioManager.PlaySFX(audioManager.projectile);

        Destroy(projectile, 3f);

        StartCoroutine(ProjectileCooldown());
    }

    IEnumerator ProjectileCooldown()
    {
        canUseProjectile = false;
        yield return new WaitForSeconds(projectileCooldown);
        canUseProjectile = true;
    }

    void Heal(int amount)
    {

        health = Mathf.Clamp(health + amount, 0, maxHealth);

        Debug.Log($"Healed {amount}, current HP: {health}");

        Vector3 healOffset = new Vector3(-0.3f, 0, 0);
        GameObject healSpirit = SummonSpirit(HealingGod, healOffset);
        audioManager.PlaySFX(audioManager.heal);
        Animator healAnim = healSpirit.GetComponent<Animator>();
        if (healAnim != null)
        {
            healAnim.SetTrigger("Heal");
            float animLength = healAnim.GetCurrentAnimatorStateInfo(0).length;
            Destroy(healSpirit, animLength);
        }
        else
        {
            Destroy(healSpirit, 2f);
        }

        StartCoroutine(HealCooldown());
    }

    IEnumerator HealCooldown()
    {
        canHeal = false;
        yield return new WaitForSeconds(HealTimer);
        canHeal = true;
    }

    public GameObject SummonSpirit(GameObject spirit, Vector3 offset)
    {
        GameObject summoned = Instantiate(spirit, transform.position + offset, Quaternion.identity);
        summoned.transform.SetParent(transform);
        return summoned;
    }

    public bool GetCanDash()
    {
        return canDash;
    }
    
    public bool GetCanUseProjectile()
    {
        return canUseProjectile;
    }
    
    public bool GetCanHeal()
    {
        return canHeal;
    }


}
