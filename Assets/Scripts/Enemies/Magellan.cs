using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Magellan : Enemy
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

    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private GameObject slashEffect1; //the effect of the slash 1

    private bool isAttacking = false;
    private bool isDead = false;
    private Animator anim;
    
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    protected void                 Start()
    {

        anim = GetComponent<Animator>();
        patrolStartPos = transform.position;
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

 // ... (Your existing code)

// In Magellan.cs

private void Die()
    {
        isDead = true;
        
        // 1. Parameter Fix: This MUST match the Animator's TRIGGER parameter name (MagellansTransformation)
        anim.SetTrigger("Magellans Transformation"); 
        
        rb.linearVelocity = Vector2.zero;
        
        // 2. Clip Name Fix: This MUST match the exact animation clip name, 
        // including the apostrophe and space, which is causing the 'not found' error.
        float animationLength = GetAnimationLength("Magellan's Transformation");
        
        StartCoroutine(DeathSequence(animationLength));
    }
    
    // NEW HELPER METHOD: Get the duration of a specific animation clip
    private float GetAnimationLength(string clipName)
    {
        if (anim == null) return 0f;

        // Get the current animator controller
        RuntimeAnimatorController ac = anim.runtimeAnimatorController;
        
        // Loop through all animation clips in the controller
        foreach (AnimationClip clip in ac.animationClips)
        {
            // The clip name must exactly match the string passed in Die()
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        
        Debug.LogWarning($"Animation clip '{clipName}' not found in the Animator Controller.");
        return 0f; // Return 0 if the clip is not found
    }

private IEnumerator DeathSequence(float animationDuration)
{
    // ------------------------------------------------------------------
    // NEW: Wait for the animation to finish playing before starting the fade.
    yield return new WaitForSeconds(animationDuration);
    // ------------------------------------------------------------------

    GameObject fadeObj = new GameObject("FadeOverlay");
    Canvas canvas = fadeObj.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 9999; // Render on top of everything
    
    CanvasScaler scaler = fadeObj.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    
    GameObject imageObj = new GameObject("FadeImage");
    imageObj.transform.SetParent(fadeObj.transform, false);
    
    UnityEngine.UI.Image fadeImage = imageObj.AddComponent<UnityEngine.UI.Image>();
    fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
    fadeImage.raycastTarget = false;
    
    RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
    rectTransform.anchorMin = Vector2.zero;
    rectTransform.anchorMax = Vector2.one;
    rectTransform.sizeDelta = Vector2.zero;
    rectTransform.anchoredPosition = Vector2.zero;
    
    // Fade to black over 1 second
    float fadeDuration = 1f;
    float elapsed = 0f;
    
    while (elapsed < fadeDuration)
    {
        elapsed += Time.deltaTime;
        float alpha = Mathf.Clamp01(elapsed / fadeDuration);
        fadeImage.color = new Color(0, 0, 0, alpha);
        yield return null;
    }
    
    fadeImage.color = Color.black; // Ensure fully black
    
    // Wait 1 second in black screen
    yield return new WaitForSeconds(1f);

    // No need for an extra delay here since we waited for the animation
    // yield return new WaitForSeconds(0.5f); 

    SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
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
