using System.Collections;
using UnityEngine;

public class EnemyMelee : Enemy
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolRange = 3f;
    [SerializeField] private float idleWaitTime = 2f;
    private Vector2 patrolStartPos;
    private bool movingRight = true;
    private bool isIdle = false;

    [Header("Aggro Settings")]
    //[SerializeField] private float aggroRange = 6f;
    [SerializeField] private float attackRange;

    [SerializeField] private LayerMask attackableLayer;
    private bool isAggroed = false;

    [SerializeField] private Transform AggroBoxTransform; //the middle of the side attack area
    [SerializeField] private Vector2 AggroBoxArea; //how large the area of side attack is
    //[SerializeField] private float jumpForce = 20f;
    //[SerializeField] private float heightDifferenceThreshold = 2f;
    [SerializeField] private float AggroTimer = 5f;

    [Header("Chase Settings")]
    [SerializeField] private float ChaseSpeed;

    [Header("Attack Settings")]
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private Transform AttackBoxTransform; //the middle of the side attack area
    [SerializeField] private Vector2 AttackBoxArea; //how large the area of side attack is
    [SerializeField] private GameObject slashEffect1; //the effect of the slash 1

    private bool isAttacking = false;
    private bool isDead = false;
    private Animator anim;

    protected void Start()
    {
        anim = GetComponent<Animator>();
        patrolStartPos = transform.position;
        health = 5f; // default health
        attackRange = AttackBoxArea.x;
        rb.freezeRotation = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(AttackBoxTransform.position, AttackBoxArea);
        Gizmos.DrawWireCube(AggroBoxTransform.position, AggroBoxArea);
    }

    IEnumerator AggroCooldown()
    {
        yield return new WaitForSeconds(AggroTimer);
        isAggroed = false;
        beenHit = false;
    }

    protected override void Update()
    {
        anim.SetBool("Aggro", isAggroed);

        base.Update();
        if (isDead || isRecoiling) return;

        Collider2D[] aggroHits = Physics2D.OverlapBoxAll(AggroBoxTransform.position, AggroBoxArea, 0f, LayerMask.GetMask("Player"));
        if (aggroHits.Length > 0 || beenHit)
        {
            isAggroed = true;
            StartCoroutine(AggroCooldown());
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (isAggroed)
        {
            if (distance > attackRange)
            {
                ChasePlayer();
            }
            else if (!isAttacking)
            {
                StartCoroutine(AttackRoutine());
            }
                
        }
        else
        {
            Patrol();
        }

    }

    private void Patrol()
    {
        if (isIdle) return;

        float targetX = movingRight ? patrolStartPos.x + patrolRange : patrolStartPos.x - patrolRange;
        float direction = Mathf.Sign(targetX - transform.position.x);

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        anim.SetBool("Walking", true);

        if (Mathf.Abs(transform.position.x - targetX) < 0.1f)
            StartCoroutine(SwitchPatrolDirection());
    }


    private IEnumerator SwitchPatrolDirection()
    {
        isIdle = true;
        anim.SetBool("Walking", false);
        yield return new WaitForSeconds(idleWaitTime);
        movingRight = !movingRight;
        FlipSprite(movingRight);
        isIdle = false;
    }

    private void ChasePlayer()
    {
        Vector2 playerPos = player.transform.position;
        float direction = Mathf.Sign(playerPos.x - transform.position.x);

        rb.linearVelocity = new Vector2(direction * ChaseSpeed, rb.linearVelocity.y);
        anim.SetBool("Walking", true);
        FlipSprite(direction > 0);

        //// Jump if height difference
        //float heightDiff = playerPos.y - transform.position.y;
        //if (heightDiff > heightDifferenceThreshold && Grounded())
        //{
        //    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        //    anim.SetTrigger("Jump");
        //}
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("Walking", false);

        yield return new WaitForSeconds(attackDelay);
        anim.SetTrigger("Attack");
        MessyController.Instance.TakeDamage(damage);

        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(AttackBoxTransform.position, AttackBoxArea, 0, attackableLayer);

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<MessyController>() != null)
            {
                objectsToHit[i].GetComponent<MessyController>().TakeDamage(1);
            }
        }

        SlashEffectAtAngle(slashEffect1, 0, AttackBoxTransform);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);

        audioManager.PlaySFX(audioManager.hurt2);

        if (health <= 0)
        {
            Die();
        }
    }
       void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        GameObject slash = Instantiate(_slashEffect, _attackTransform.position, Quaternion.identity);
        slash.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        slash.transform.localScale = new Vector2(_slashEffect.transform.localScale.x, _slashEffect.transform.localScale.y);
    }

    private IEnumerator Hitstun()
    {
        isRecoiling = true;
        //anim.SetTrigger("Hurt");
        yield return new WaitForSeconds(0.5f);
        isRecoiling = false;
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

    private void FlipSprite(bool faceRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    

    //private bool Grounded()
    //{
    //    // Simple ground check
    //    return Physics2D.Raycast(transform.position, Vector2.down, 0.2f, LayerMask.GetMask("Ground"));
    //}
}
