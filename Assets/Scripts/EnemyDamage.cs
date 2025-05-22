using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Damage dealt to player on contact")]
    public int damageAmount = 20;

    [Tooltip("Time between damage ticks (prevents spam damage)")]
    public float damageInterval = 1.0f;

    [Tooltip("Knockback force applied to player")]
    public float knockbackForce = 5f;

    [Header("Visual Effects")]
    [Tooltip("Screen shake intensity when damaging player")]
    public float screenShakeIntensity = 0.2f;

    [Tooltip("Duration of screen shake")]
    public float screenShakeDuration = 0.2f;

    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool enableDebug = true;

    private float lastDamageTime;
    private PlayerHealth playerHealth;
    private Rigidbody2D playerRb;
    private Animator _animator;
    private Collider2D damageCollider;

    private void Start()
    {
        // Get components
        damageCollider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();

        // Debug logging
        if (enableDebug)
        {
            Debug.Log($"[EnemyDamage] {gameObject.name} - Starting setup:");
            Debug.Log($"  - Collider2D: {(damageCollider != null ? "Found" : "Missing")}");
            Debug.Log($"  - Is Trigger: {(damageCollider != null ? damageCollider.isTrigger.ToString() : "N/A")}");
            Debug.Log($"  - Animator: {(_animator != null ? "Found" : "Missing")}");
            Debug.Log($"  - Layer: {LayerMask.LayerToName(gameObject.layer)}");
        }

        // Validation
        if (damageCollider == null)
        {
            Debug.LogError($"[EnemyDamage] {gameObject.name} requires a Collider2D component!");
            return;
        }

        if (!damageCollider.isTrigger)
        {
            Debug.LogWarning($"[EnemyDamage] {gameObject.name} collider should be set as Trigger for damage detection!");
        }

        if (_animator == null)
        {
            Debug.LogWarning($"[EnemyDamage] {gameObject.name} has no Animator component - attack animations won't play");
        }
        else
        {
            // Check if Attack trigger exists
            bool hasAttackTrigger = false;
            foreach (var param in _animator.parameters)
            {
                if (param.name == "Attack" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasAttackTrigger = true;
                    break;
                }
            }

            if (!hasAttackTrigger && enableDebug)
            {
                Debug.LogWarning($"[EnemyDamage] {gameObject.name} animator doesn't have 'Attack' trigger parameter!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enableDebug)
        {
            Debug.Log($"[EnemyDamage] {gameObject.name} - Trigger Enter with: {other.name} (Tag: {other.tag})");
        }

        if (other.CompareTag("Player"))
        {
            if (enableDebug)
            {
                Debug.Log($"[EnemyDamage] {gameObject.name} - Player detected! Dealing damage...");
            }
            DealDamageToPlayer(other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if enough time has passed since last damage
            if (Time.time >= lastDamageTime + damageInterval)
            {
                if (enableDebug)
                {
                    Debug.Log($"[EnemyDamage] {gameObject.name} - Damage interval passed, dealing damage again...");
                }
                DealDamageToPlayer(other);
            }
            else if (enableDebug)
            {
                float timeLeft = (lastDamageTime + damageInterval) - Time.time;
                Debug.Log($"[EnemyDamage] {gameObject.name} - Still in damage cooldown: {timeLeft:F1}s remaining");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && enableDebug)
        {
            Debug.Log($"[EnemyDamage] {gameObject.name} - Player left trigger area");
        }
    }

    private void DealDamageToPlayer(Collider2D playerCollider)
    {
        // Get player health component
        if (playerHealth == null)
        {
            playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError($"[EnemyDamage] Player doesn't have PlayerHealth component!");
                return;
            }
        }

        // Check if player can take damage
        if (playerHealth.IsInvincible)
        {
            if (enableDebug)
            {
                Debug.Log($"[EnemyDamage] {gameObject.name} - Player is invincible, damage blocked");
            }
            return;
        }

        // Trigger attack animation
        if (_animator != null)
        {
            if (enableDebug)
            {
                Debug.Log($"[EnemyDamage] {gameObject.name} - Triggering attack animation");
            }
            _animator.SetTrigger("Attack");
        }

        // Deal damage to player
        playerHealth.TakeDamage(damageAmount);
        lastDamageTime = Time.time;

        // Apply knockback
        ApplyKnockback(playerCollider);

        // Add screen shake effect
        ApplyScreenShake();

        if (enableDebug)
        {
            Debug.Log($"[EnemyDamage] {gameObject.name} dealt {damageAmount} damage to player! Player health: {playerHealth.currentHealth}/{playerHealth.maxHealth}");
        }
    }

    private void ApplyKnockback(Collider2D playerCollider)
    {
        if (knockbackForce <= 0) return;

        // Get player rigidbody
        if (playerRb == null)
        {
            playerRb = playerCollider.GetComponent<Rigidbody2D>();
            if (playerRb == null)
            {
                if (enableDebug)
                {
                    Debug.LogWarning($"[EnemyDamage] Player has no Rigidbody2D - knockback disabled");
                }
                return;
            }
        }

        // Calculate knockback direction
        Vector2 knockbackDirection = (playerCollider.transform.position - transform.position).normalized;

        // Apply knockback force
        playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if (enableDebug)
        {
            Debug.Log($"[EnemyDamage] {gameObject.name} - Applied knockback force: {knockbackForce} in direction: {knockbackDirection}");
        }
    }

    private void ApplyScreenShake()
    {
        // Simple screen shake implementation
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            StartCoroutine(ShakeCamera(mainCamera));
        }
        else if (enableDebug)
        {
            Debug.LogWarning("[EnemyDamage] No main camera found for screen shake");
        }
    }

    private System.Collections.IEnumerator ShakeCamera(Camera camera)
    {
        Vector3 originalPosition = camera.transform.position;
        float elapsed = 0f;

        while (elapsed < screenShakeDuration)
        {
            float x = Random.Range(-screenShakeIntensity, screenShakeIntensity);
            float y = Random.Range(-screenShakeIntensity, screenShakeIntensity);

            camera.transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camera.transform.position = originalPosition;
    }

    // Manual testing method (call from inspector or other scripts)
    [ContextMenu("Test Damage Player")]
    public void TestDamagePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                Debug.Log("[EnemyDamage] Manual test - dealing damage to player");
                health.TakeDamage(damageAmount);
            }
        }
        else
        {
            Debug.LogError("[EnemyDamage] No player found for manual test!");
        }
    }

    // Optional: Visualize damage range in editor
    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            if (col is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, col.bounds.size);
            }
        }
    }

    // Debug info in inspector
    private void OnValidate()
    {
        if (damageAmount <= 0)
        {
            Debug.LogWarning($"[EnemyDamage] {gameObject.name} - Damage amount should be greater than 0!");
        }

        if (damageInterval <= 0)
        {
            Debug.LogWarning($"[EnemyDamage] {gameObject.name} - Damage interval should be greater than 0!");
        }
    }
}