using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public int maxHealth = 100;

    [Tooltip("Current health points")]
    public int currentHealth;

    [Header("Heart UI Elements")]
    [Tooltip("Array of heart SpriteRenderer GameObjects (should be 5 hearts for 100 HP)")]
    public SpriteRenderer[] heartRenderers;

    [Tooltip("Sprite for full heart (20 HP)")]
    public Sprite fullHeart;

    [Tooltip("Sprite for half heart (10 HP)")]
    public Sprite halfHeart;

    [Tooltip("Sprite for empty heart (0 HP)")]
    public Sprite emptyHeart;

    [Header("Effects")]
    [Tooltip("Duration of invincibility after taking damage")]
    public float invincibilityTime = 1f;

    [Tooltip("How fast the player flickers during invincibility")]
    public float flickerSpeed = 0.1f;

    [Header("Audio")]
    [Tooltip("Sound effect when taking damage")]
    public AudioClip damageSound;

    [Tooltip("Sound effect when healing")]
    public AudioClip healSound;

    [Tooltip("Sound effect when player dies")]
    public AudioClip deathSound;

    // Private variables
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Player_Controller playerController;

    // Events for other scripts to listen to
    public System.Action<int> OnHealthChanged;
    public System.Action OnPlayerDied;

    private void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<Player_Controller>();

        // Add AudioSource if not present
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Update UI
        UpdateHealthUI();

        // Validate UI setup
        ValidateUISetup();
    }

    private void ValidateUISetup()
    {
        if (heartRenderers == null || heartRenderers.Length == 0)
        {
            Debug.LogError("No heart renderers assigned! Please assign heart SpriteRenderer components in the inspector.");
            return;
        }

        int expectedHearts = Mathf.CeilToInt(maxHealth / 20f);
        if (heartRenderers.Length != expectedHearts)
        {
            Debug.LogWarning($"Heart UI mismatch! Expected {expectedHearts} hearts for {maxHealth} HP, but found {heartRenderers.Length} hearts.");
        }

        if (fullHeart == null || halfHeart == null || emptyHeart == null)
        {
            Debug.LogError("Heart sprites not assigned! Please assign full, half, and empty heart sprites.");
        }

        // Check if all heart renderers are properly assigned
        for (int i = 0; i < heartRenderers.Length; i++)
        {
            if (heartRenderers[i] == null)
            {
                Debug.LogError($"Heart renderer at index {i} is null! Please assign all heart SpriteRenderer components.");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Ignore damage if invincible or already dead
        if (isInvincible || currentHealth <= 0)
            return;

        // Apply damage
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Play damage sound
        PlaySound(damageSound);

        // Update UI
        UpdateHealthUI();

        // Trigger event
        OnHealthChanged?.Invoke(currentHealth);

        // Check if player died
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility period
            StartCoroutine(InvincibilityCoroutine());
        }

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }

    public void RestoreHealth(int amount)
    {
        if (currentHealth >= maxHealth)
            return;

        int oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Only play sound and update if health actually changed
        if (currentHealth > oldHealth)
        {
            PlaySound(healSound);
            UpdateHealthUI();
            OnHealthChanged?.Invoke(currentHealth);

            Debug.Log($"Player healed {currentHealth - oldHealth} HP. Health: {currentHealth}/{maxHealth}");
        }
    }

    public void UpdateHealthUI()
    {
        if (heartRenderers == null) return;

        // Each heart represents 20 HP
        for (int i = 0; i < heartRenderers.Length; i++)
        {
            if (heartRenderers[i] == null) continue;

            // Calculate health for this heart position
            int heartMinHealth = i * 20;
            int heartMaxHealth = heartMinHealth + 20;
            int healthInThisHeart = Mathf.Clamp(currentHealth - heartMinHealth, 0, 20);

            // Set appropriate heart sprite
            if (healthInThisHeart >= 20)
            {
                // Full heart (20 HP)
                heartRenderers[i].sprite = fullHeart;
            }
            else if (healthInThisHeart >= 10)
            {
                // Half heart (10-19 HP)
                heartRenderers[i].sprite = halfHeart;
            }
            else if (healthInThisHeart > 0)
            {
                // Quarter heart or less (1-9 HP) - show as half heart
                heartRenderers[i].sprite = halfHeart;
            }
            else
            {
                // Empty heart (0 HP)
                heartRenderers[i].sprite = emptyHeart;
            }

            // Ensure heart is visible and white
            heartRenderers[i].color = Color.white;
            heartRenderers[i].enabled = true;
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // Visual feedback during invincibility
        if (spriteRenderer != null)
        {
            float elapsed = 0f;

            while (elapsed < invincibilityTime)
            {
                // Flicker effect
                spriteRenderer.enabled = !spriteRenderer.enabled;

                yield return new WaitForSeconds(flickerSpeed);
                elapsed += flickerSpeed;
            }

            // Ensure sprite is visible when invincibility ends
            spriteRenderer.enabled = true;
        }
        else
        {
            // Fallback if no sprite renderer
            yield return new WaitForSeconds(invincibilityTime);
        }

        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player has died!");

        // Play death sound
        PlaySound(deathSound);

        // Disable player movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Disable colliders to prevent further damage
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // Trigger death event
        OnPlayerDied?.Invoke();

        // You can add more death logic here:
        // - Show game over screen
        // - Restart level
        // - Respawn player
        // - etc.
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public getters for other scripts
    public bool IsInvincible => isInvincible;
    public bool IsAlive => currentHealth > 0;
    public float HealthPercentage => (float)currentHealth / maxHealth;

    // Method to fully heal player
    public void FullHeal()
    {
        RestoreHealth(maxHealth);
    }

    // Method to set health directly (for debugging/testing)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void SetHealthForTesting(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
    }
}