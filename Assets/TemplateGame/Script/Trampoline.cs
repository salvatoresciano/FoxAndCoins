using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public float bounceForce = 20f; // forza di rimbalzo
    public AudioClip bounceSound;   // suono opzionale

    public Animator Anim;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            PlayAnimation();

            // Applica la forza verso l'alto
            Vector3 velocity = player.velocity;
            velocity.y = bounceForce;
            player.velocity = velocity;

            // Effetto sonoro o visuale
            if (bounceSound != null)
            {
                SoundManager.PlaySfx(bounceSound);
            }

            // Se vuoi, istanzia anche un effetto visivo
            if (player.JumpEffect != null)
                Instantiate(player.JumpEffect, player.transform.position, Quaternion.identity);
        }
    }

    private void PlayAnimation()
    {
        Anim.SetTrigger("Jump");
    }
}
