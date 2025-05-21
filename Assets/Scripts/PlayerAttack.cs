using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.2f;
    public int attackDamage = 20;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;

    [Header("Movement During Attack")]
    public float attackMoveSpeedMultiplier = 0.3f;

    [Header("Attack Direction")]
    public float attackOffset = 0.8f;

    private float nextAttackTime = 0f;
    private Player_Controller playerController;
    private float attackStartTime = -1f;

    // Get attack duration from animator instead of hardcoding
    private float currentAttackDuration = 0f;

    void Start()
    {
        playerController = GetComponent<Player_Controller>();
    }

    void Update()
    {
        HandleAttackInput();
    }

    void HandleAttackInput()
    {
        // Left mouse button = attack right
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime && !IsCurrentlyAttacking())
        {
            PerformAttack(Vector2.left);
        }
        // Right mouse button = attack left
        else if (Input.GetMouseButtonDown(1) && Time.time >= nextAttackTime && !IsCurrentlyAttacking())
        {
            PerformAttack(Vector2.right);
        }
    }

    void PerformAttack(Vector2 attackDirection)
    {
        // Start attack
        attackStartTime = Time.time;
        nextAttackTime = Time.time + attackCooldown;

        // Flip player sprite to face attack direction
        FlipPlayerTowardsAttack(attackDirection);

        // Tell the PlayerController to play attack animation
        if (playerController != null)
        {
            playerController.TriggerAttackAnimation(attackDirection);
        }

        // Get the actual attack animation duration from the animator
        UpdateAttackDuration();

        // Perform the actual attack (damage detection)
        StartCoroutine(AttackSequence(attackDirection));
    }

    private void UpdateAttackDuration()
    {
        // Get the animator from PlayerController
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // Get current state info for the attack layer (assuming base layer is 0)
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // If we're in an attack state, get its length
            if (stateInfo.IsName("Attack") || stateInfo.IsTag("Attack"))
            {
                currentAttackDuration = stateInfo.length;
            }
            else
            {
                // Fallback duration if we can't get it from animator
                currentAttackDuration = 0.5f;
            }
        }
        else
        {
            currentAttackDuration = 0.5f;
        }
    }

    IEnumerator AttackSequence(Vector2 attackDirection)
    {
        // Small delay to sync with animation start
        yield return new WaitForSeconds(0.1f);

        // Calculate attack position
        Vector2 attackPosition = (Vector2)transform.position + (attackDirection * attackOffset);

        // Detect enemies in attack area
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);

        // Apply damage to enemies
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);

                // Optional: Add knockback
                ApplyKnockback(enemy, attackDirection);
            }
        }

        // Wait for remaining attack duration
        float remainingDuration = currentAttackDuration - 0.1f;
        if (remainingDuration > 0)
        {
            yield return new WaitForSeconds(remainingDuration);
        }

        // Reset attack time
        attackStartTime = -1f;
    }

    void ApplyKnockback(Collider2D enemy, Vector2 direction)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            float knockbackForce = 3f;
            enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }

    void FlipPlayerTowardsAttack(Vector2 attackDirection)
    {
        // Get the sprite renderer from the player
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null && Mathf.Abs(attackDirection.x) > 0.1f)
        {
            // Flip sprite based on attack direction
            // If attacking to the left (negative x), flip sprite
            spriteRenderer.flipX = attackDirection.x < 0;
        }
    }

    // Public methods for PlayerController to check attack state
    public bool IsCurrentlyAttacking()
    {
        return attackStartTime > 0 && (Time.time - attackStartTime) < currentAttackDuration;
    }

    public float GetMovementMultiplier()
    {
        return IsCurrentlyAttacking() ? attackMoveSpeedMultiplier : 1f;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Show both possible attack directions
        Vector2 rightAttackPos = (Vector2)transform.position + (Vector2.right * attackOffset);
        Vector2 leftAttackPos = (Vector2)transform.position + (Vector2.left * attackOffset);

        Gizmos.color = IsCurrentlyAttacking() ? Color.red : Color.yellow;

        // Draw right attack range (left mouse button)
        Gizmos.DrawWireSphere(rightAttackPos, attackRange);

        // Draw left attack range (right mouse button)  
        Gizmos.DrawWireSphere(leftAttackPos, attackRange);

        // Draw attack directions
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, rightAttackPos);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, leftAttackPos);
    }
}