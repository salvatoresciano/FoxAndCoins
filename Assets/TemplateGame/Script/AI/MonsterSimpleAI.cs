using UnityEngine;
using System.Collections;

public class MonsterSimpleAI : EnemyAI {
	[Header("Owner Setup")]
	public Animator animator;
	public string hitEventName="n/a";
	public string deadEventName="n/a";
	public string attackEventName = "n/a";
	public string respawnEventName = "n/a";

	protected override void HitEvent ()
	{
        base.HitEvent(); // Questo avvia PushBack che imposta isSocking = true

        if (isDead)
            Dead();

        if (animator != null && hitEventName.CompareTo("n/a") != 0)
            animator.SetTrigger(hitEventName);
	}

    protected override void AttackEvent()
    {
        base.AttackEvent();
        if (animator != null && attackEventName.CompareTo("n/a") != 0)
            animator.SetTrigger(attackEventName);
    }

    public override void Update ()
	{
		base.Update ();

	}

	protected override void Dead ()
	{
		base.Dead ();
		if (animator != null && deadEventName.CompareTo ("n/a") != 0)
			animator.SetTrigger (deadEventName);
		
		SetForce (0, 5);
		controller.HandlePhysic = false;
	}

	protected override void OnRespawn ()
	{
		base.OnRespawn ();
		controller.HandlePhysic = true;
		if (animator != null && respawnEventName.CompareTo("n/a") != 0)
			animator.Play(respawnEventName);
    }
}
