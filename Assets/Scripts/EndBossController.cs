using UnityEngine;
using System.Reflection;

// Create a new script without the conflicting interface
public class EndBossController : MonoBehaviour
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

    [Tooltip("Attack cooldown in seconds")]
    public float attackCooldown = 3.0f;

    [Tooltip("Attack range")]
    public float attackRange = 2.5f;

    [Tooltip("Damage dealt by attack")]
    public float attackDamage = 20f;

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
    private float attackTimer = 0f;
    private bool isDead = false;

    // Animation parameter names - the ones you specified
    private readonly string idleAnimationName = "IdleS";
    private readonly string attackAnimationName = "AttackS";
    private readonly string deathAnimationName = "Death";

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

        // Set initial animation
        PlayAnimation(idleAnimationName);
    }

    void Update()
    {
        // Don't do anything if dead
        if (isDead)
            return;

        // Update attack cooldown timer
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

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

            // Try to perform attack if in range
            if (distanceToPlayer <= attackRange && attackTimer <= 0)
            {
                PerformAttack();
            }
            else
            {
                // Chase player if not attacking
                ChasePlayer();

                // Always play idle animation (since you want IdleS for both standing and moving)
                PlayAnimation(idleAnimationName);
            }
        }
        // Otherwise return to start position if enabled
        else if (returnToStartPosition && !isNearStartPosition())
        {
            isReturningToStart = true;
            ReturnToStart();

            // Play idle animation while returning
            PlayAnimation(idleAnimationName);
        }
        else
        {
            // Stop movement
            moveDirection = Vector2.zero;

            // Play idle animation
            PlayAnimation(idleAnimationName);
        }
    }

    // Play animation by name
    void PlayAnimation(string animationName)
    {
        if (!hasAnimator)
            return;

        // Play the specified animation
        animator.Play(animationName);
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

    // Perform attack
    void PerformAttack()
    {
        if (!hasAnimator || isAttacking)
            return;

        // Stop movement during attack
        moveDirection = Vector2.zero;
        isAttacking = true;

        // Play attack animation
        PlayAnimation(attackAnimationName);

        // Reset cooldown
        attackTimer = attackCooldown;

        // Apply damage
        Invoke("ApplyAttackDamage", 0.5f); // Apply damage halfway through animation

        // Get animation length to ensure it plays fully
        if (hasAnimator)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            float animationLength = 1.0f; // Default fallback length

            foreach (AnimationClip clip in clips)
            {
                if (clip.name == attackAnimationName)
                {
                    animationLength = clip.length;
                    break;
                }
            }

            // Resume movement only after animation completes
            Invoke("EndAttack", animationLength);
        }
        else
        {
            // Fallback if no animator found
            Invoke("EndAttack", 1.0f);
        }
    }

    // Apply damage from attack
    void ApplyAttackDamage()
    {
        // Find all colliders within attack range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);

        // Apply damage to player if hit
        foreach (Collider2D collider in hitColliders)
        {
            // Use SendMessage to avoid interface conflicts
            collider.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    // End attack state
    void EndAttack()
    {
        isAttacking = false;

        // Return to idle animation
        PlayAnimation(idleAnimationName);
    }

    // Take damage - this is called through SendMessage
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
        PlayAnimation(deathAnimationName);

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

        // Get death animation length to ensure it plays fully before destroying
        if (hasAnimator)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            float animationLength = 2.0f; // Default fallback length

            foreach (AnimationClip clip in clips)
            {
                if (clip.name == deathAnimationName)
                {
                    animationLength = clip.length;
                    break;
                }
            }

            // Destroy after full animation plays
            Invoke("DestroyBoss", animationLength);
        }
        else
        {
            // Fallback if no animator found
            Invoke("DestroyBoss", 2.0f);
        }
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

            // Show attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}   