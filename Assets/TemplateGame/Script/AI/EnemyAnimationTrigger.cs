using UnityEngine;

public class EnemyAnimationTrigger : MonoBehaviour
{

    EnemyAI enemyAI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    public void Shoot()
    {
        enemyAI.FireProjectile();
    }
}
