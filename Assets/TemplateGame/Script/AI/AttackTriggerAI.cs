using UnityEngine;
using UnityEngine.UIElements;

public class AttackTriggerAI : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI; // Riferimento allo script EnemyAI
    [SerializeField] private float rayDistance = 5f; // Distanza del raycast
    [SerializeField] private LayerMask playerLayer; // Layer di collisione per rilevare il giocatore
    [SerializeField] private float attackCooldown = 2f; // Tempo di attesa tra un attacco e l'altro (in secondi)
    [SerializeField] private float yOffset = 0f; // Offset sull'asse Y per il raycast

    private float nextAttackTime = 0f; // Tempo al quale è possibile eseguire il prossimo attacco

    private void Update()
    {
        // Direzione del raycast
        Vector2 rayDirection = transform.right * (transform.localScale.x >= 0 ? -1 : 1);

        // Posizione iniziale con offset Y
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * yOffset;

        // Esegui il raycast
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, playerLayer);

        if (hit.collider != null)
        {
            var player = hit.collider.GetComponent<Player>();
            if (player != null && Time.time >= nextAttackTime)
            {
                enemyAI.Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        // Debug ray con offset Y
        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.yellow);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 rayDirection = transform.right * (transform.localScale.x >= 0 ? -1 : 1);
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * yOffset;

        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.yellow);
    }
}
