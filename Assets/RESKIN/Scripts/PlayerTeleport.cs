using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PlayerTeleport : MonoBehaviour
{
    [Header("References")]
    public GameObject player;           // The Player object
    public Controller2D playerControl; // Drag your movement script here (e.g. PlayerController)
    public GameObject fadeAnimator;       // The UI Animator for the black screen
    public GameObject interactButton;

    [Header("Settings")]
    public Transform targetLocation;    // Where the player will go
    public float waitTime = 2.0f;       // How long to stay in the dark[]

    [Header("Events")]
    public UnityEvent OnTriggerEnter;
    public UnityEvent OnTriggerExit;

    // Call this method to start the teleport (e.g., from a trigger or button)
    public void StartTeleport()
    {
        StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        // 1. Disable Player Controls
        if (playerControl != null) playerControl.enabled = false;

        // 2. Start Fade to Black
        // "FadeOut" should be the name of the Trigger or State in your Animator
        fadeAnimator.SetActive(true);

        // 3. Wait for the transition and the specified delay
        yield return new WaitForSeconds(waitTime);

        // 4. Move Player to Target Position
        // If using CharacterController, disable it temporarily to "warp" the position
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = targetLocation.position;

        if (cc != null) cc.enabled = true;

        // 5. Start Fade In (Remove black)
        //fadeAnimator.SetTrigger("FadeIn");

        // 6. Re-enable Controls
        if (playerControl != null) playerControl.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag != "Player")
        {
            return;
        }

        Debug.Log("PLAYER IN TELEPORT");
        //StartTeleport();

        interactButton.SetActive(true);

        OnTriggerEnter?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag != "Player")
        {
            return;
        }

        interactButton.SetActive(false);

        OnTriggerExit?.Invoke();
    }
}