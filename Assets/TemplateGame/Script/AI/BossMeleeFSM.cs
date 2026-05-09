using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class BossMeleeFSM : MonoBehaviour, ICanTakeDamage
{
    public enum EnemyState { Idle, Walk, Attack, Hurt, Die }

    [Header("Current State")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("Settings")]
    public float speed = 1f;
    [Range(10, 1000)] public float health = 100f;
    public float distanceDetectPlayer = 10f;
    public float attackZone = 2f;
    public float coolDown = 2f;
    public float delayHit = 0.1f;

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
    public float givePlayerDamage = 30f;
    public Vector2 pushPlayer = new Vector2(5, 5);
    public bool canBeKillOnHead = true; // Abilita il danno da salto sulla testa
    public float rateDamage = 0.2f;    // Frequenza danno da contatto
    private float nextDamage;

    [Header("Events")]
    public UnityEvent OnDieEvent;

    private Player player;
    private float lastAttackTime;
    private float originalScaleX;
    private bool isMoving = true;

    // Hash dei parametri Animator per prestazioni migliori
    private static readonly int WalkBool = Animator.StringToHash("IsWalking");
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

        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        if (currentState == EnemyState.Die || player == null || player.isFinish) return;

        UpdateFSM();
    }

    private void UpdateFSM()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= distanceDetectPlayer) ChangeState(EnemyState.Walk);
                break;

            case EnemyState.Walk:
                if (distanceToPlayer <= attackZone)
                {
                    if (Time.time >= lastAttackTime + coolDown) ChangeState(EnemyState.Attack);
                    else ChangeState(EnemyState.Idle);
                }
                else if (distanceToPlayer > distanceDetectPlayer)
                {
                    ChangeState(EnemyState.Idle);
                }
                else
                {
                    MoveTowardsPlayer();
                }
                break;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == EnemyState.Die) return;

        // Reset parametri prima di cambiare
        anim.SetBool(WalkBool, false);

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Walk:
                anim.SetBool(WalkBool, true);
                break;

            case EnemyState.Attack:
                StartCoroutine(AttackRoutine());
                break;

            case EnemyState.Hurt:
                StartCoroutine(HurtRoutine());
                break;

            case EnemyState.Die:
                HandleDeath();
                break;
        }
    }

    private void MoveTowardsPlayer()
    {
        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
        transform.Translate(new Vector3(direction * speed * Time.deltaTime, 0));

        // Gestione flipping
        transform.localScale = new Vector3(direction > 0 ? -originalScaleX : originalScaleX, transform.localScale.y, transform.localScale.z);
    }

    IEnumerator AttackRoutine()
    {
        

        yield return new WaitForSeconds(delayHit);

        // Scelta attacco casuale[cite: 1]
        int rndAttack = Random.Range(1, 5);
        anim.SetTrigger("Attack" + rndAttack);

        if (attackSound) SoundManager.PlaySfx(attackSound);

        // Controllo danno nel range[cite: 1]
        if (Physics2D.OverlapCircle(centerPoint.position, attackZone, playerLayer))
        {
            // Danno da contatto normale (il boss danneggia il player)[cite: 1]
            float side = Mathf.Sign(player.transform.position.x - transform.position.x);
            player.SetForce(new Vector2(Mathf.Abs(player.velocity.x) * side, 10f));
            player.TakeDamage(givePlayerDamage, pushPlayer, gameObject);
        }

        yield return new WaitForSeconds(1f); // Durata stimata animazione
        lastAttackTime = Time.time;
        ChangeState(EnemyState.Idle);
    }

    IEnumerator HurtRoutine()
    {
        anim.SetTrigger(HitTrigger);
        yield return new WaitForSeconds(0.5f); // Lock temporaneo durante il colpo
        ChangeState(EnemyState.Idle);
    }

    public void TakeDamage(float damage, Vector2 force, GameObject instigator)
    {
        if (currentState == EnemyState.Die) return;

        health -= damage;
        if (HealthBar != null) HealthBar.currentHealth = health;

        if (health <= 0)
            ChangeState(EnemyState.Die);
        else
            ChangeState(EnemyState.Hurt);
    }

    private void HandleDeath()
    {
        anim.SetTrigger(DieTrigger);
        if (deadSound) SoundManager.PlaySfx(deadSound);

        foreach (var col in GetComponents<Collider2D>()) col.enabled = false;
        rig.linearVelocity = Vector2.zero;
        rig.isKinematic = true;

        OnDieEvent?.Invoke();
        GameManager.Instance.GameFinish();
    }

    // Metodo TRIGGER per gestire il salto sulla testa o contatto
    void OnTriggerStay2D(Collider2D other)
    {
        if (currentState == EnemyState.Die || !player.isPlaying || Time.time < nextDamage + rateDamage) return;

        if (other.CompareTag("Player"))
        {
            nextDamage = Time.time;

            // Se il giocatore cade sulla testa del boss[cite: 1]
            if (canBeKillOnHead && player.transform.position.y > transform.position.y + 0.5f)
            {
                // Spinge il giocatore verso l'alto/lontano[cite: 1]
                player.SetForce(new Vector2(transform.localScale.x > 0 ? -pushPlayer.x : pushPlayer.x, pushPlayer.y));

                // Il boss subisce danni (il colpo viene contato come "proiettile" o salto)[cite: 1]
                TakeDamage(10f, Vector2.zero, gameObject);
                return;
            }

            // Danno da contatto normale (il boss danneggia il player)[cite: 1]
            float side = Mathf.Sign(player.transform.position.x - transform.position.x);
            player.SetForce(new Vector2(Mathf.Abs(player.velocity.x) * side, 10f));
            player.TakeDamage(givePlayerDamage, Vector2.zero, gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (centerPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPoint.position, attackZone);
        Gizmos.DrawWireSphere(transform.position, distanceDetectPlayer);
    }
}