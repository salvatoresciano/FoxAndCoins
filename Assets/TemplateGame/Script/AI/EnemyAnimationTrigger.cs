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

    public void StartMove()
    {
        enemyAI.StartMoving();
    }

    public void StopMove()
    {
        enemyAI.StopMoving();
    }

    public void StopSnocking()
    {
        enemyAI.StopSnocking();
    }

    public void IncreaseSpeed()
    {
        enemyAI.IncreaseSpeed();
    }
    public void ResetSpeed()
    {
        enemyAI.ResetSpeed();
    }
}
