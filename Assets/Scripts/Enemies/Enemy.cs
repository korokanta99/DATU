using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    protected MessyController player;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;

    protected bool beenHit = false;

    protected float recoilTimer;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected AudioManager audioManager;
    //[Header("Health")]
    //[SerializeField][Range(0, 5)] float attack_range = 1;
    //[SerializeField][Range(0, 5)] float attack_delay = 1;
    //[SerializeField][Range(0, 10)] float attack_cooldown = 1;
    //float attack_duration;
    //float cooldown;
    //[SerializeField] GameObject Attackbox;
    //float player_distance;
    //[SerializeField] bool ready_to_attack;



    public void Init(MessyController player, Vector3 coordinates)
    {
        // populates the private fields of this Enemy object to be used by whatever methods nad logic the enemy class has.
        this.player = player;
        transform.position = coordinates;
                
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        player = MessyController.Instance;
        audioManager = AudioManager.Instance;
    }

    protected virtual void Update()
    {
        if (health <= 0)
        {
            //Destroy(gameObject);
        }
        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        beenHit = true;

        audioManager.PlaySFX(audioManager.hurt2);
        StartCoroutine(HurtEffect());



        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }

    IEnumerator HurtEffect()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }
    protected void OnTriggerStay2D(Collider2D _other)
    {
        if (_other.CompareTag("Player") && !MessyController.Instance.pState.invincible)
        {
            Attack();
        }
    }
    protected virtual void Attack()
    {
        MessyController.Instance.TakeDamage(damage);
    }

    

}
