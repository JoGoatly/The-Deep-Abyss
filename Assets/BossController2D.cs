using UnityEngine;
using System.Reflection;

// Interface definition for damage-receiving objects
public interface IDamageable
{
    void TakeDamage(float damage);
}

public class BossController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Boss movement speed")]
    public float moveSpeed = 1.5f;

    [Tooltip("Minimum distance to player")]
    public float minDistanceToPlayer = 1.0f;

    [Tooltip("Optional: Reference to player (if not specified, will be found automatically)")]
    public Transform playerTransform;

    [Tooltip("Should the sprite flip in the direction of movement?")]
    public bool flipSpriteToDirection = true;

    [Header("Sight Radius Settings")]
    [Tooltip("Radius in which the boss can detect the player")]
    public float sightRadius = 7.0f;

    [Tooltip("Enables visualization of sight radius in the editor")]
    public bool showSightRadius = true;

    [Tooltip("If enabled, the boss returns to start position when player is no longer in sight")]
    public bool returnToStartPosition = true;

    [Header("Collision Settings")]
    [Tooltip("Layers considered as obstacles (walls etc.)")]
    public LayerMask obstacleLayer;

    [Tooltip("Player layer for sight check")]
    public LayerMask playerLayer;

    [Tooltip("Search radius for collisions")]
    public float collisionRadius = 0.5f;

    [Tooltip("Use Rigidbody2D for movement (recommended for collisions)")]
    public bool useRigidbody = true;

    [Header("Combat Settings")]
    [Tooltip("Boss health points")]
    public float health = 100f;

    [Tooltip("Smash attack cooldown in seconds")]
    public float smashAttackCooldown = 4.0f;

    [Tooltip("Thrust attack cooldown in seconds")]
    public float thrustAttackCooldown = 3.0f;

    [Tooltip("Attack range for smash attack")]
    public float smashAttackRange = 2.0f;

    [Tooltip("Attack range for thrust attack")]
    public float thrustAttackRange = 3.0f;

    [Tooltip("Damage dealt by smash attack")]
    public float smashAttackDamage = 20f;

    [Tooltip("Damage dealt by thrust attack")]
    public float thrustAttackDamage = 15f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool hasAnimator = false;
    private bool hasSpriteRenderer = false;
    private Vector2 moveDirection;
    private Vector3 startPosition;
    private bool isPlayerInSight = false;
    private bool isReturningToStart = false;

    // Animation states
    private bool isAttacking = false;
    private float smashAttackTimer = 0f;
    private float thrustAttackTimer = 0f;
    private bool isDead = false;

    // Animation hash IDs (for better performance)
    private int isMovingHash;
    private int smashTriggerHash;
    private int thrustTriggerHash;
    private int deathTriggerHash;

    void Start()
    {
        // Store start position for return function
        startPosition = transform.position;

        // Get the Rigidbody2D (required for physics-based movement)
        rb = GetComponent<Rigidbody2D>();

        // If no Rigidbody exists and we want to use one, create it
        if (rb == null && useRigidbody)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Disable gravity for top-down game
            rb.freezeRotation = true; // Prevent rotation
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        }

        // Check if Animator exists
        animator = GetComponent<Animator>();
        hasAnimator = animator != null;

        // Cache animator parameter hashes for better performance
        if (hasAnimator)
        {
            isMovingHash = Animator.StringToHash("isMoving");
            smashTriggerHash = Animator.StringToHash("smash");
            thrustTriggerHash = Animator.StringToHash("thrust");
            deathTriggerHash = Animator.StringToHash("death");
        }

        // Get SpriteRenderer for flip function
        spriteRenderer = GetComponent<SpriteRenderer>();
        hasSpriteRenderer = spriteRenderer != null;

        // If no player reference is provided, try to find one
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("No player with tag 'Player' found! Please assign the player manually.");
            }
        }
    }

    void Update()
    {
        // Don't do anything if dead
        if (isDead)
            return;

        // Update attack cooldown timers
        if (smashAttackTimer > 0)
            smashAttackTimer -= Time.deltaTime;

        if (thrustAttackTimer > 0)
            thrustAttackTimer -= Time.deltaTime;

        if (playerTransform == null)
            return;

        // Check if player is in sight radius
        CheckPlayerInSight();

        // Don't move if currently performing an attack
        if (isAttacking)
            return;

        // If player is in sight, chase or attack them
        if (isPlayerInSight)
        {
            isReturningToStart = false;

            // Get distance to player
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Try to perform attacks if in range
            if (distanceToPlayer <= smashAttackRange && smashAttackTimer <= 0)
            {
                PerformSmashAttack();
            }
            else if (distanceToPlayer <= thrustAttackRange && thrustAttackTimer <= 0)
            {
                PerformThrustAttack();
            }
            else
            {
                // Chase player if not attacking
                ChasePlayer();
            }
        }
        // Otherwise return to start position if enabled
        else if (returnToStartPosition && !isNearStartPosition())
        {
            isReturningToStart = true;
            ReturnToStart();
        }
        else
        {
            // Stop movement
            moveDirection = Vector2.zero;

            // Set idle animation
            if (hasAnimator)
            {
                animator.SetBool(isMovingHash, false);
            }
        }
    }

    // Check if player is in sight radius
    void CheckPlayerInSight()
    {
        if (playerTransform == null)
            return;

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Check if player is within sight radius
        if (distanceToPlayer <= sightRadius)
        {
            // Optional: Line of sight check with raycast
            RaycastHit2D hit = Physics2D.Linecast(
                transform.position,
                playerTransform.position,
                obstacleLayer
            );

            // If there are no obstacles between boss and player
            if (hit.collider == null)
            {
                isPlayerInSight = true;
                return;
            }
        }

        isPlayerInSight = false;
    }

    // Player chase logic
    void ChasePlayer()
    {
        if (playerTransform == null)
            return;

        // Calculate direction to player (ignore Z-axis for 2D)
        Vector2 directionToPlayer = (Vector2)(playerTransform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        // Normalize direction
        directionToPlayer.Normalize();

        // Flip sprite in direction of movement (if desired)
        if (hasSpriteRenderer && flipSpriteToDirection && directionToPlayer.x != 0)
        {
            spriteRenderer.flipX = directionToPlayer.x < 0;
        }

        // Move the boss only if further than minimum distance
        if (distanceToPlayer > minDistanceToPlayer)
        {
            // Set movement direction
            moveDirection = directionToPlayer;

            // Control animation
            if (hasAnimator)
            {
                animator.SetBool(isMovingHash, true);
            }

            // If not using Rigidbody, move directly with collision detection
            if (!useRigidbody)
            {
                MoveWithCollisionCheck(directionToPlayer);
            }
        }
        else
        {
            // Stop movement
            moveDirection = Vector2.zero;

            // Set idle animation
            if (hasAnimator)
            {
                animator.SetBool(isMovingHash, false);
            }
        }
    }

    // Return to start position
    void ReturnToStart()
    {
        // Calculate direction to start position
        Vector2 directionToStart = (Vector2)(startPosition - transform.position);
        float distanceToStart = directionToStart.magnitude;

        // Normalize direction
        directionToStart.Normalize();

        // Flip sprite in direction of movement (if desired)
        if (hasSpriteRenderer && flipSpriteToDirection && directionToStart.x != 0)
        {
            spriteRenderer.flipX = directionToStart.x < 0;
        }

        // Set movement direction
        moveDirection = directionToStart;

        // Control animation
        if (hasAnimator)
        {
            animator.SetBool(isMovingHash, true);
        }

        // If not using Rigidbody, move directly with collision detection
        if (!useRigidbody)
        {
            MoveWithCollisionCheck(directionToStart);
        }
    }

    // Check if boss is near start position
    bool isNearStartPosition()
    {
        return Vector2.Distance(transform.position, startPosition) < 0.1f;
    }

    // Physics-based movement with Rigidbody2D
    void FixedUpdate()
    {
        if (isDead || isAttacking)
            return;

        if (useRigidbody && rb != null && moveDirection != Vector2.zero)
        {
            // Check if next step would cause a collision
            Vector2 nextPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

            // Raycasting for collisions
            RaycastHit2D hit = Physics2D.CircleCast(
                rb.position,
                collisionRadius,
                moveDirection,
                moveSpeed * Time.fixedDeltaTime,
                obstacleLayer
            );

            if (hit.collider == null)
            {
                // No obstacle - move normally
                rb.MovePosition(nextPosition);
            }
            else
            {
                // Obstacle detected - try to slide along it
                Vector2 slidingDirection = Vector2.Perpendicular(hit.normal).normalized;
                if (Vector2.Dot(slidingDirection, moveDirection) < 0)
                {
                    slidingDirection = -slidingDirection;
                }

                // Test if sliding in this direction is possible
                RaycastHit2D slideHit = Physics2D.CircleCast(
                    rb.position,
                    collisionRadius,
                    slidingDirection,
                    moveSpeed * Time.fixedDeltaTime * 0.5f,
                    obstacleLayer
                );

                if (slideHit.collider == null)
                {
                    // Slide along the wall
                    rb.MovePosition(rb.position + slidingDirection * moveSpeed * Time.fixedDeltaTime * 0.75f);
                }
            }
        }
    }

    // Movement without Rigidbody with manual collision check
    private void MoveWithCollisionCheck(Vector2 direction)
    {
        // Check if path is clear
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            collisionRadius,
            direction,
            moveSpeed * Time.deltaTime,
            obstacleLayer
        );

        if (hit.collider == null)
        {
            // No obstacle - move normally
            transform.position += new Vector3(direction.x, direction.y, 0) * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Obstacle detected - try to slide along it
            Vector2 slidingDirection = Vector2.Perpendicular(hit.normal).normalized;
            if (Vector2.Dot(slidingDirection, direction) < 0)
            {
                slidingDirection = -slidingDirection;
            }

            // Test if sliding in this direction is possible
            RaycastHit2D slideHit = Physics2D.CircleCast(
                transform.position,
                collisionRadius,
                slidingDirection,
                moveSpeed * Time.deltaTime * 0.5f,
                obstacleLayer
            );

            if (slideHit.collider == null)
            {
                // Slide along the wall
                transform.position += new Vector3(slidingDirection.x, slidingDirection.y, 0) * moveSpeed * Time.deltaTime * 0.75f;
            }
        }
    }

    // Perform smash attack
    void PerformSmashAttack()
    {
        if (!hasAnimator || isAttacking)
            return;

        // Stop movement during attack
        moveDirection = Vector2.zero;
        isAttacking = true;

        // Trigger smash animation
        animator.SetTrigger(smashTriggerHash);

        // Reset cooldown
        smashAttackTimer = smashAttackCooldown;

        // Look for player targets (you need to implement damage logic)
        Invoke("ApplySmashDamage", 0.5f); // Apply damage halfway through animation

        // Resume movement after animation completes (assuming animation length)
        Invoke("EndAttack", 1.0f); // Adjust time based on animation length
    }

    // Apply damage from smash attack
    void ApplySmashDamage()
    {
        // Example: Find all colliders within smash attack range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, smashAttackRange, playerLayer);

        // Apply damage to player if hit
        foreach (Collider2D collider in hitColliders)
        {
            // Try to find a player health component - adjust this to match your health system
            // Option 1: Check if player has IDamageable interface
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(smashAttackDamage);
            }
            // Option 2: Check for a PlayerHealth component (use your actual component name)
            var playerHealth = collider.GetComponent<MonoBehaviour>();
            if (playerHealth != null && playerHealth.GetType().GetMethod("TakeDamage") != null)
            {
                playerHealth.SendMessage("TakeDamage", smashAttackDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    // Perform thrust attack
    void PerformThrustAttack()
    {
        if (!hasAnimator || isAttacking)
            return;

        // Stop movement during attack
        moveDirection = Vector2.zero;
        isAttacking = true;

        // Trigger thrust animation
        animator.SetTrigger(thrustTriggerHash);

        // Reset cooldown
        thrustAttackTimer = thrustAttackCooldown;

        // Apply damage (you need to implement damage logic)
        Invoke("ApplyThrustDamage", 0.4f); // Apply damage halfway through animation

        // Resume movement after animation completes (assuming animation length)
        Invoke("EndAttack", 0.8f); // Adjust time based on animation length
    }

    // Apply damage from thrust attack
    void ApplyThrustDamage()
    {
        // Calculate attack direction toward player
        if (playerTransform == null)
            return;

        Vector2 attackDirection = (playerTransform.position - transform.position).normalized;

        // Cast a ray in the direction of the player to detect hits
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            attackDirection,
            thrustAttackRange,
            playerLayer
        );

        if (hit.collider != null)
        {
            // Try to find a player health component - adjust this to match your health system
            // Option 1: Check if player has IDamageable interface
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(thrustAttackDamage);
            }
            // Option 2: Check for any component with TakeDamage method
            var playerHealth = hit.collider.GetComponent<MonoBehaviour>();
            if (playerHealth != null)
            {
                playerHealth.SendMessage("TakeDamage", thrustAttackDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    // End attack state
    void EndAttack()
    {
        isAttacking = false;
    }

    // Take damage
    public void TakeDamage(float damage)
    {
        // Reduce health
        health -= damage;

        // Check for death
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    // Die
    void Die()
    {
        isDead = true;

        // Stop all movement
        moveDirection = Vector2.zero;

        // Play death animation
        if (hasAnimator)
        {
            animator.SetTrigger(deathTriggerHash);
        }

        // Disable components but keep renderer
        if (useRigidbody && rb != null)
        {
            rb.simulated = false;
        }

        // Optional: Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        // Optional: Destroy after animation (adjust time based on animation length)
        Invoke("DestroyBoss", 2.0f);
    }

    // Destroy boss object
    void DestroyBoss()
    {
        Destroy(gameObject);
    }

    // Visualize sight radius in Unity Editor
    private void OnDrawGizmos()
    {
        if (showSightRadius)
        {
            // Show sight radius in yellow (or red if player is seen)
            Gizmos.color = isPlayerInSight ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRadius);

            // Show start position
            if (returnToStartPosition && Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(startPosition, 0.3f);
            }

            // Show attack ranges
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, smashAttackRange);

            Gizmos.color = new Color(1, 0.5f, 0);
            Gizmos.DrawWireSphere(transform.position, thrustAttackRange);
        }
    }
}