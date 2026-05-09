using UnityEngine;
using System.Collections;

public class SimpleProjectile : Projectile, ICanTakeDamage
{
	public int Damage;
	public GameObject DestroyEffect;
	public int pointToGivePlayer;
	public float timeToLive;

	public AudioClip soundHitEnemy;
	[Range(0,1)]
	public float soundHitEnemyVolume = 0.5f;
	public AudioClip soundHitNothing;
	[Range(0,1)]
	public float soundHitNothingVolume = 0.5f;


	public bool isDrop;

	
	// Update is called once per frame
	void Update ()
	{
		if ((timeToLive -= Time.deltaTime) <= 0) {
			DestroyProjectile ();
			return;
		}

		if (!isDrop)
		{
            transform.Translate((Direction + new Vector2(InitialVelocity.x, 0)) * Speed * Time.deltaTime, Space.World);
        }
		else
		{
            transform.Translate((new Vector2(0, -1f) + new Vector2(InitialVelocity.x, 0)) * Speed * Time.deltaTime, Space.World);
        }
			
	}

	void DestroyProjectile(){
		if (DestroyEffect != null)
			Instantiate (DestroyEffect, transform.position, Quaternion.identity);
		
		Destroy (gameObject);
	}


	public void TakeDamage (float damage, Vector2 force, GameObject instigator)
	{
		if (pointToGivePlayer != 0) {
			var projectile = instigator.GetComponent<Projectile> ();
			if (projectile != null && projectile.Owner.GetComponent<Player> () != null) {
				GameManager.Instance.AddPoint (pointToGivePlayer);
				GameManager.Instance.ShowFloatingText ("+" + pointToGivePlayer, transform.position,Color.yellow);
			}
		}

		SoundManager.PlaySfx (soundHitNothing, soundHitNothingVolume);
		DestroyProjectile ();
	}

	protected override void OnCollideOther (Collider2D other)
	{
		SoundManager.PlaySfx (soundHitNothing, soundHitNothingVolume);
		DestroyProjectile ();
	}

	protected override void OnCollideTakeDamage (Collider2D other, ICanTakeDamage takedamage)
	{
		takedamage.TakeDamage (Damage, Vector2.zero, gameObject);
		SoundManager.PlaySfx (soundHitEnemy, soundHitEnemyVolume);
		DestroyProjectile ();
	}
}

