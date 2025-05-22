using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 60;
    public int currentHealth;

    [Header("Drop Settings")]
    [Tooltip("Prefab of the coin to spawn when enemy dies")]
    public GameObject coinPrefab;

    [Tooltip("Should a coin always drop when enemy dies?")]
    public bool alwaysDropCoin = true;

    [Tooltip("Chance to drop coin (0-1) if alwaysDropCoin is false")]
    [Range(0f, 1f)]
    public float coinDropChance = 0.8f;

    [Tooltip("Offset from enemy position where coin spawns")]
    public Vector2 coinSpawnOffset = Vector2.zero;

    [Header("Optional Effects")]
    public float hitFlashDuration = 0.15f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Try to find coin prefab automatically if not assigned
        if (coinPrefab == null)
        {
            GameObject foundCoin = Resources.Load<GameObject>("Coin");
            if (foundCoin != null)
            {
                coinPrefab = foundCoin;
            }
            else
            {
                Debug.LogWarning("No coin prefab assigned and none found in Resources folder!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Visual hit feedback
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        // Spawn coin before destroying enemy
        SpawnCoin();

        // Destroy the enemy
        Destroy(gameObject);
    }

    void SpawnCoin()
    {
        // Check if we should spawn a coin
        bool shouldSpawnCoin = alwaysDropCoin || Random.Range(0f, 1f) <= coinDropChance;

        if (shouldSpawnCoin && coinPrefab != null)
        {
            // Calculate spawn position
            Vector3 spawnPosition = transform.position + new Vector3(coinSpawnOffset.x, coinSpawnOffset.y, 0);

            // Spawn the coin
            GameObject spawnedCoin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

            // Optional: Add some randomness to coin position
            if (spawnedCoin != null)
            {
                // Add slight random offset to make it look more natural
                Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
                spawnedCoin.transform.position += new Vector3(randomOffset.x, randomOffset.y, 0);
            }
        }
        else if (coinPrefab == null)
        {
            Debug.LogWarning("Cannot spawn coin - no coin prefab assigned!");
        }
    }
}