using UnityEngine;
using UnityEngine.UI; // Required for the Image component

public class HealthBarEnemy : MonoBehaviour
{
    [Header("UI Reference")]
    public Image forceGroundSprite; // Changed from Transform to Image

    [Header("Settings")]
    public float maxHealth = 1f;
    public float currentHealth = 1f;

    // Update is called once per frame
    void Update()
    {
        // Calculate the percentage (value between 0.0 and 1.0)
        float healthPercent = currentHealth / maxHealth;

        // Apply to fillAmount instead of localScale
        if (forceGroundSprite != null)
        {
            forceGroundSprite.fillAmount = healthPercent;
        }

        // Optional: Color lerping example
        // forceGroundSprite.color = Color.Lerp(Color.red, Color.green, healthPercent);
    }
}