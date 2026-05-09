using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class EnemyAI : MonoBehaviour, ICanTakeDamage, IPlayerRespawnListener {
	[Header("Behavior")]
	public float gravity = 35f;
	[Tooltip("allow push the enemy back when hit by player")]
	public bool pushEnemyBack = true;
	Vector2 pushForce;
	public GameObject spawnItemWhenDead;

	[Header("Moving")]
	public float moveSpeed = 3;
	private float defaultSpeed;
	public bool ignoreCheckGroundAhead = false;
	public GameObject DestroyEffect;
    public bool canBeStoppedAtHit;

    public enum HealthType{HitToKill, HealthAmount}
	[Header("Health")]

	public HealthType healthType;
	public int maxHitToKill = 1;
	[HideInInspector]
	public int currentHitLeft;

    public float health;
	float currentHealth;
	public int pointToGivePlayer;
	public GameObject HurtEffect;
	
	[Header("Sound")]
	public AudioClip hurtSound;
	[Range(0,1)]
	public float hurtSoundVolume = 0.5f;
	public AudioClip deadSound;
	[Range(0,1)]
	public float deadSoundVolume = 0.5f;
    public AudioClip attackSound;
    [Range(0, 1)]
    public float attackSoundVolume = 0.5f;
    public AudioClip playerHurtSound;
    [Range(0, 1)]
    public float playerHurtVolume = 0.5f;

    [Header("Projectile")]
	public bool isUseProjectile;
	public LayerMask shootableLayer;
	public Transform PointSpawn;
	public Projectile projectile;
	public float fireRate = 1f;
	public float detectDistance = 10f;
	float _fireIn;

	public bool isPlaying{ get; set; }
	public bool isSocking{ get; set; }

	public bool isAttacking { get; set; }
	public bool isDead{ get; set; }

	private Vector3 velocity;
	private Vector2 _direction;
	private Vector2 _startPosition;	//set this enemy back to the first position when Player spawn to check point
	private Vector2 _startScale;	//set this enemy back to the first position when Player spawn to check point
	[HideInInspector]
	public Controller2D controller;

	// Use this for initialization
	public virtual void Start () {
		controller = GetComponent<Controller2D> ();
		_direction = Vector2.left;
		_startPosition = transform.position;
		_startScale = transform.localScale;
		_fireIn = fireRate;
		currentHealth = health;
		currentHitLeft = maxHitToKill;

		defaultSpeed = moveSpeed;

		isPlaying = true;
		isSocking = false;
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if (GameManager.Instance.State == GameManager.GameState.Finish)
			enabled = false;
		
		if (!isPlaying || isSocking)
			return;

		_fireIn -= Time.deltaTime;

		if ((_direction.x > 0 && controller.collisions.right) || (_direction.x < 0 && controller.collisions.left)
			|| (!ignoreCheckGroundAhead && !controller.collisions.isGrounedAhead && controller.collisions.below)) {

			_direction = -_direction;
			transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		}



		if (isUseProjectile) {
			var position = PointSpawn != null ? PointSpawn.position : transform.position;
			var hit = Physics2D.Raycast (position, _direction, detectDistance, shootableLayer);
			if (hit) {
				if (hit.collider.gameObject.GetComponent<Player> () != null)
					FireProjectile ();
			}
		}
	}

	public virtual void LateUpdate(){

        // Il nemico si muove solo se sta giocando, NON è stordito (isSocking) e NON sta attaccando
        if (isPlaying && !isSocking && !isAttacking)
        {
            velocity.x = _direction.x * moveSpeed;
        }
        else
        {
            // Se è colpito o attacca, la velocità X deve azzerarsi (a meno che non ci sia il pushback)
            if (!isSocking) velocity.x = 0;
        }

        velocity.y += -gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime, false);

        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0;
    }

	public void SetForce(float x, float y){
		velocity = new Vector3 (x, y, 0);
	}

	public void FireProjectile(){
		if (_fireIn > 0)
			return;

		_fireIn = fireRate;
		var _projectile = (Projectile) Instantiate (projectile, PointSpawn.position, Quaternion.identity);
		_projectile.Initialize (gameObject, _direction, Vector2.zero);
	}


	/// <summary>
	/// Takes the damage.
	/// </summary>
	/// <param name="damage">Damage.</param>
	/// <param name="instigator">Instigator.</param>
	public void TakeDamage (float damage, Vector2 force, GameObject instigator)
	{
		if (isDead)
			return;
		
		pushForce = force;

		if (HurtEffect != null)
			Instantiate (HurtEffect, instigator.transform.position, Quaternion.identity);

		if (healthType == HealthType.HitToKill) {
			currentHitLeft--;
			if (currentHitLeft <= 0) {
				isDead = true;
			}
		} else if (healthType == HealthType.HealthAmount) {
			currentHealth -= damage;
			if (currentHealth <= 0) {
				isDead = true;
			}
		}

		if (instigator.GetComponent<Block> () != null)
			isDead = true;

		/*if (!isAttacking)
		{
            HitEvent();
        }*/

        HitEvent();

    }

	protected virtual void HitEvent(){
		
		SoundManager.PlaySfx (hurtSound, hurtSoundVolume);
		if (HurtEffect != null)
			Instantiate (HurtEffect, transform.position, transform.rotation);

		StopAllCoroutines ();
		StartCoroutine(PushBack (0.35f));

		if (canBeStoppedAtHit)
		{
            isSocking = true;

            // Fermiamo il movimento immediatamente
            velocity.x = 0;
        }

	

    }

    protected virtual void AttackEvent()
    {

        SoundManager.PlaySfx(attackSound, attackSoundVolume);
        SoundManager.PlaySfx(playerHurtSound, playerHurtVolume);

    }

	public void Attack()
	{
		AttackEvent();
	}


    protected virtual void Dead(){
		
		isPlaying = false;

//		transform.localScale = new Vector3 (1, -1, 1);	//fall

		StopAllCoroutines ();
		SoundManager.PlaySfx (deadSound, deadSoundVolume);
		if (pointToGivePlayer != 0) {
			GameManager.Instance.AddPoint (pointToGivePlayer);
			GameManager.Instance.ShowFloatingText ("+" + pointToGivePlayer, transform.position, Color.yellow);
		}

		if (DestroyEffect != null)
			Instantiate (DestroyEffect, transform.position, transform.rotation);

		if(spawnItemWhenDead!=null)
			Instantiate (spawnItemWhenDead, PointSpawn.position, PointSpawn.rotation);

		//turn off all colliders if the enemy have
		var boxCo = GetComponents<BoxCollider2D> ();
		foreach (var box in boxCo) {
			box.enabled = false;
		}
		var CirCo = GetComponents<CircleCollider2D> ();
		foreach (var cir in CirCo) {
			cir.enabled = false;
		}

	}

	protected virtual void OnRespawn(){

       
    }

	public void OnPlayerRespawnInThisCheckPoint (CheckPoint checkpoint, Player player)
	{
		
		currentHealth = health;
		currentHitLeft = maxHitToKill;
		transform.position = _startPosition;
		transform.localScale = _startScale;
		_direction = Vector2.left;
		velocity = Vector3.zero;
		isPlaying = true;
		isDead = false;
		isSocking = false;
		gameObject.SetActive (true);

		//turn on all colliders if the enemy have
		var boxCo = GetComponents<BoxCollider2D> ();
		foreach (var box in boxCo) {
			box.enabled = true;
		}
		var CirCo = GetComponents<CircleCollider2D> ();
		foreach (var cir in CirCo) {
			cir.enabled = true;
		}


		OnRespawn ();
	}

	public IEnumerator PushBack(float delay){
		
		isPlaying = false;
		SetForce (GameManager.Instance.Player.transform.localScale.x * pushForce.x, pushForce.y);

		yield return new WaitForSeconds (delay);
		SetForce (0, 0);

		if (isDead)
			Dead ();
		else
			isPlaying = true;
	}

    public void StopMoving()
    {
        isAttacking = true;
        velocity.x = 0;
    }

    public void StartMoving()
    {
        isAttacking = false;
    }

	public void StopSnocking()
	{
		isSocking = false;
	}

	public void IncreaseSpeed()
	{
		moveSpeed += 2;
	}

	public void ResetSpeed()
	{
		moveSpeed = defaultSpeed;
	}

    public void OnDrawGizmosSelected(){
		if (isUseProjectile) {
			Gizmos.color = Color.blue;
			if (_direction.magnitude != 0)
				Gizmos.DrawRay (PointSpawn.position, _direction * detectDistance);
			else
				Gizmos.DrawRay (PointSpawn.position, Vector2.left * detectDistance);
		}
	}
}
