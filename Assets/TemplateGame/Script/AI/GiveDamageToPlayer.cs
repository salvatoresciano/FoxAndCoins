using UnityEngine;
using System.Collections;

public class GiveDamageToPlayer : MonoBehaviour {
	[Header("Option")]
	[Tooltip("destroy this object when hit player?")]
	public bool isDestroyWhenHitPlayer = false;
	public GameObject DestroyFx;

	[Header("Make Damage")]
	public int DamageToPlayer;
	[Tooltip("delay a moment before give next damage to Player")]
	public float rateDamage = 0.2f;
	public Vector2 pushPlayer = new Vector2 (0, 10);
	float nextDamage;

	[Tooltip("Give damage to this object when Player jump on his head")]
	public bool canBeKillOnHead = false;
	public float damageOnHead;

    [Header("Raycast Check")]
    [Tooltip("Altezza rispetto all'origine da usare per il confronto con il player")]
    public float raycastHeightCheck = 0f;
    public Color rayColor = Color.red;


    void OnTriggerStay2D(Collider2D other){
		var Player = other.GetComponent<Player> ();
		if (Player == null)
			return;

		if (!Player.isPlaying)
			return;

		if (Time.time < nextDamage + rateDamage)
			return;

		nextDamage = Time.time;

        /* old code
		 * 
		 * if (canBeKillOnHead && Player.transform.position.y > transform.position.y) {

			Player.SetForce(pushPlayer);
			var canTakeDamage = (ICanTakeDamage) GetComponent (typeof(ICanTakeDamage));
			if (canTakeDamage != null)
				canTakeDamage.TakeDamage (damageOnHead, Vector2.zero, gameObject);
			
			return;
		}*/

        if (canBeKillOnHead && Player.transform.position.y > (transform.position.y + raycastHeightCheck))
        {

            Player.SetForce(pushPlayer);
            var canTakeDamage = (ICanTakeDamage)GetComponent(typeof(ICanTakeDamage));
            if (canTakeDamage != null)
                canTakeDamage.TakeDamage(damageOnHead, Vector2.zero, gameObject);

            return;
        }

        //Push player back
        //		var facingDirectionX = Mathf.Sign (Player.transform.localScale.x);
        //		var facingDirectionY = Mathf.Sign (Player.velocity.y);
        if (DamageToPlayer == 0)
			return;

		var facingDirectionX = Mathf.Sign (Player.transform.position.x - transform.position.x);
		var facingDirectionY = Mathf.Sign (Player.velocity.y);

		Player.SetForce(new Vector2 (Mathf.Clamp (Mathf.Abs(Player.velocity.x), 10, 15) * facingDirectionX,
			Mathf.Clamp (Mathf.Abs(Player.velocity.y), 5, 15) * facingDirectionY * -1));

		Player.TakeDamage (DamageToPlayer, Vector2.zero, gameObject);

		if (isDestroyWhenHitPlayer) {
			if (DestroyFx != null)
				Instantiate (DestroyFx, transform.position, Quaternion.identity);

			Destroy (gameObject);
		}
	}

    void OnDrawGizmos()
    {
        Gizmos.color = rayColor;
        Vector3 origin = transform.position;
        Vector3 target = new Vector3(transform.position.x, transform.position.y + raycastHeightCheck, transform.position.z);
        Gizmos.DrawLine(origin, target);
        Gizmos.DrawSphere(target, 0.1f);
    }
}
