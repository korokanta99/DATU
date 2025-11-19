using System.Collections;
using UnityEngine;

public class EnemyRanged : Enemy
{
    [Header("Ranged Settings")]
    [SerializeField] private float aggroRange = 6f;
    [SerializeField] private float tooCloseRange = 2f;
    [SerializeField] private float chaseDuration = 10f;
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRange = 3f;      // How far from start pos enemy patrols
    [SerializeField] private float idleWaitTime = 2f;     // Wait time at edges
    private Vector2 patrolStartPos;                        // Starting position
    private bool movingRight = true;                       // Patrol direction
    private bool isIdle = false;                           // Waiting at edge
    private float chaseTimer;
    private float shootTimer;
    private bool isAggro;
    private bool isDead;

    private Animator anim;    

    protected void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        anim.SetBool("Aggro", isAggro);


        Collider2D[] aggroHits = Physics2D.OverlapCircleAll(transform.position, aggroRange, LayerMask.GetMask("Player"));
        if (aggroHits.Length > 0 || beenHit)
        {
            isAggro = true;
            chaseTimer = chaseDuration;
        }

        if (health <= 0)
        {
            Die();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (isAggro)

        {

            FacePlayer();

            chaseTimer -= Time.deltaTime;

            if (distToPlayer > aggroRange)
            {
                // Continue chasing for limited time
                if (chaseTimer > 0)
                    ChasePlayer();
                else
                    isAggro = false;
                beenHit = false;
            }
            else
            {
                // Actively attack / reposition
                if (distToPlayer <= tooCloseRange)
                    KiteAway();
                else
                    AttackPlayer();
            }
        }
        else
        {
            Patrol();
        }
    }
    private void FacePlayer()
{
    if (player == null) return;

    // Flip sprite based on player position
    if (player.transform.position.x > transform.position.x)
        sr.flipX = false; // facing right
    else
        sr.flipX = true;  // facing left
}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.DrawWireSphere(transform.position, tooCloseRange);
    }

    private void Patrol()
    {
        if (isIdle) return;

        float targetX = movingRight ? patrolStartPos.x + patrolRange : patrolStartPos.x - patrolRange;
        float direction = Mathf.Sign(targetX - transform.position.x);

        // Move horizontally
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        anim.SetBool("isMoving", true);

        // Flip sprite to face direction
        sr.flipX = direction < 0;

        // Check if reached edge
        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
            StartCoroutine(SwitchPatrolDirection());
    }

    private IEnumerator SwitchPatrolDirection()
{
    isIdle = true;
    rb.linearVelocity = Vector2.zero;
    anim.SetBool("isMoving", false);

    yield return new WaitForSeconds(idleWaitTime);

    movingRight = !movingRight;
    isIdle = false;
}

    private void ChasePlayer()
    {
        Vector2 dir = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = dir * speed;
        anim.SetBool("isMoving", true);
    }

    private void AttackPlayer()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            StartCoroutine(Shoot());
            shootTimer = shootCooldown;
        }
    }

    private IEnumerator Shoot()
    {
        anim.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.2f); // sync with animation
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }

    private void KiteAway()
    {
        Vector2 dir = (transform.position - player.transform.position).normalized;
        rb.linearVelocity = dir * (speed * 1.2f); // back away a bit faster
        anim.SetBool("isMoving", true);
    }

    private void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        // Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
        StartCoroutine(DeathSequence());
    }

        private IEnumerator DeathSequence()
{
    // Wait until the death animation finishes
    yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length * 0.95f);

    // Create corpse sprite object before destroying
    GameObject corpse = new GameObject("EnemyCorpse");
    SpriteRenderer sr = corpse.AddComponent<SpriteRenderer>();
    sr.sprite = GetComponent<SpriteRenderer>().sprite; // current frame of the dead enemy
    sr.sortingLayerID = GetComponent<SpriteRenderer>().sortingLayerID;
    sr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
    sr.flipX = GetComponent<SpriteRenderer>().flipX;
    corpse.transform.position = transform.position;
    corpse.transform.localScale = transform.localScale;

    // Optional: give corpse its own layer or fade effect later
    // corpse.layer = LayerMask.NameToLayer("Decor");

    Destroy(gameObject); // remove the enemy logic and collider
}
}
