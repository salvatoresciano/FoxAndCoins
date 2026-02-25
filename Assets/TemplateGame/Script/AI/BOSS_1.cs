using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class BOSS_1 : MonoBehaviour, ICanTakeDamage
{
    [Header("Settings")]
    public float speed = 1f;
    [Range(10, 1000)] public float health = 100f;
    public float damagePerHit = 10f;
    public float distanceDetectPlayer = 10f;
    public float attackZone = 2f;
    public float delayHit = 0.1f;
    public float coolDown = 2f;

    [Header("Components")]
    public Rigidbody2D rig;
    public Animator anim;
    public HealthBarEnemy HealthBar;
    public Transform centerPoint;
    public LayerMask playerLayer;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip deadSound;

    [Header("Damage to Player")]
    public int DamageToPlayer;
    public float rateDamage = 0.2f;
    public Vector2 pushPlayer = new Vector2(0, 10);
    public bool canBeKillOnHead = false;
    public float givePlayerDamage = 30f;

    [Header("Events")]
    public UnityEvent OnDieEvent;

    private Player player;
    private float nextDamage;
    private float originalScaleX;
    public bool moving;
    private bool isDead = false;
    public bool isAttacking = false;

    // --- Hash dei PARAMETRI (da impostare nell'Animator) ---
    private static readonly int WalkBool = Animator.StringToHash("IsWalking");
    private static readonly int AttackTrigger1 = Animator.StringToHash("Attack1");
    private static readonly int AttackTrigger2 = Animator.StringToHash("Attack2");
    private static readonly int AttackTrigger3 = Animator.StringToHash("Attack3");
    private static readonly int AttackTrigger4 = Animator.StringToHash("Attack4");
    private static readonly int HitTrigger = Animator.StringToHash("Hit");
    private static readonly int DieTrigger = Animator.StringToHash("Die");

    void Start()
    {
        if (rig == null) rig = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();

        player = FindObjectOfType<Player>();
        originalScaleX = transform.localScale.x;

        if (HealthBar != null)
        {
            HealthBar.maxHealth = health;
            HealthBar.currentHealth = health;
        }
    }

    public void Play() => moving = true;

    void Update()
    {
        if (isDead || player == null || player.isFinish) return;

        HandleMovementAndDetection();
    }

    private void HandleMovementAndDetection()
    {
        if (!moving || isAttacking)
        {
            anim.SetBool(WalkBool, false);
            return;
        }

        Vector2 rayOrigin = new Vector2(transform.position.x, centerPoint.position.y);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, distanceDetectPlayer, playerLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rayOrigin, Vector2.right, distanceDetectPlayer, playerLayer);

        bool foundPlayer = false;

        if (hitLeft)
        {
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0));
            transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
            foundPlayer = true;
        }
        else if (hitRight)
        {
            transform.Translate(new Vector3(speed * Time.deltaTime, 0));
            transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
            foundPlayer = true;
        }

        // Imposta il parametro Walk (Bool)
        anim.SetBool(WalkBool, foundPlayer);

        if (foundPlayer && Physics2D.OverlapCircle(centerPoint.position, attackZone, playerLayer))
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        moving = false;
        isAttacking = true;
        anim.SetBool(WalkBool, false);

        int rndAttack = Random.Range(0, 5);

        switch (rndAttack)
        {
            case 0:
                anim.SetTrigger(AttackTrigger1);
                break;
            case 1:
                anim.SetTrigger(AttackTrigger2);
                break;
            case 2:
                anim.SetTrigger(AttackTrigger3);
                break;
            case 3:
                anim.SetTrigger(AttackTrigger4);
                break;
        }
        // Attiva il Trigger dell'attacco
        //anim.SetTrigger(AttackTrigger);

        if (attackSound) SoundManager.PlaySfx(attackSound);

        yield return new WaitForSeconds(delayHit);

        if (Physics2D.OverlapCircle(centerPoint.position, attackZone, playerLayer))
        {
            player.TakeDamage(givePlayerDamage, new Vector2(0, 3), gameObject);
        }

        yield return new WaitForSeconds(coolDown);

        isAttacking = false;
        moving = true;
    }

    public void TakeDamage(float damage, Vector2 force, GameObject instigator)
    {
        if (isDead) return;

        health -= damagePerHit;
        if (HealthBar != null) HealthBar.currentHealth = health;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            anim.SetTrigger(HitTrigger);
        }
    }

    private void Die()
    {
        isDead = true;
        moving = false;

        SoundManager.PlaySfx(deadSound);

        // Gestione morte con Trigger e Bool per lo stato loop di morte
        anim.SetTrigger(DieTrigger);

        if (HealthBar != null) HealthBar.gameObject.SetActive(false);

        foreach (var col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        rig.linearVelocity = Vector2.zero;
        rig.isKinematic = true;

        OnDieEvent?.Invoke();
        GameManager.Instance.GameFinish();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isDead || !player.isPlaying || Time.time < nextDamage + rateDamage) return;

        if (other.CompareTag("Player"))
        {
            nextDamage = Time.time;

            if (canBeKillOnHead && player.transform.position.y > transform.position.y + 0.5f)
            {
                player.SetForce(new Vector2(transform.localScale.x > 0 ? -pushPlayer.x : pushPlayer.x, pushPlayer.y));
                TakeDamage(damagePerHit, Vector2.zero, gameObject);
                return;
            }

            float side = Mathf.Sign(player.transform.position.x - transform.position.x);
            player.SetForce(new Vector2(Mathf.Clamp(Mathf.Abs(player.velocity.x), 10, 15) * side, 10f));

            if (DamageToPlayer > 0)
            {
                player.TakeDamage(DamageToPlayer, Vector2.zero, gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (centerPoint == null) return;
        Gizmos.color = Color.yellow;
        Vector3 rayPos = new Vector3(transform.position.x, centerPoint.position.y, 0);
        Gizmos.DrawRay(rayPos, Vector2.left * distanceDetectPlayer);
        Gizmos.DrawRay(rayPos, Vector2.right * distanceDetectPlayer);
        Gizmos.DrawWireSphere(centerPoint.position, attackZone);
    }


    //add health recovery if player if out of boss view
}