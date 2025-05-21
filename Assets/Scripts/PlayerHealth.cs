using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Elements")]
    public Image[] heartImages;
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    [Header("Effects")]
    public float invincibilityTime = 1f;
    public bool isInvincible = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        currentHealth -= damage;

        // Clamp health to not go below 0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    public void RestoreHealth(int amount)
    {
        currentHealth += amount;

        // Make sure health doesn't exceed max health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        // Each heart represents 20 HP (5 hearts = 100 HP)
        for (int i = 0; i < heartImages.Length; i++)
        {
            // Calculate which heart position we're updating
            int heartPosition = i * 20;

            // Calculate how much health is in this heart position (0-20)
            int healthInThisHeart = currentHealth - heartPosition;

            if (healthInThisHeart >= 20)
            {
                // Full heart
                heartImages[i].sprite = fullHeart;
                heartImages[i].color = Color.white;
            }
            else if (healthInThisHeart >= 10)
            {
                // Half heart (10-19 HP)
                heartImages[i].sprite = halfHeart;
                heartImages[i].color = Color.white;
            }
            else if (healthInThisHeart > 0)
            {
                // Less than half heart but not empty (1-9 HP)
                // You could add more heart states here, or just use half heart
                heartImages[i].sprite = halfHeart;
                heartImages[i].color = Color.white;
            }
            else
            {
                // Empty heart (0 HP)
                heartImages[i].sprite = emptyHeart;
                heartImages[i].color = Color.white;
            }
        }
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;

        // Visual feedback for taking damage (optional)
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer)
        {
            float flickerTime = 0.1f;
            for (float i = 0; i < invincibilityTime; i += flickerTime * 2)
            {
                renderer.enabled = false;
                yield return new WaitForSeconds(flickerTime);
                renderer.enabled = true;
                yield return new WaitForSeconds(flickerTime);
            }
        }
        else
        {
            yield return new WaitForSeconds(invincibilityTime);
        }

        isInvincible = false;
    }

    private void Die()
    {
        // Player death logic
        Debug.Log("Player has died!");

        // Disable player control
        GetComponent<Collider2D>().enabled = false;

        // You could add game over logic, respawn, etc.
    }
}