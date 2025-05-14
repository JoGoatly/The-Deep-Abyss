using UnityEngine;

public class EnemyController2D : MonoBehaviour
{
    [Header("Einstellungen")]
    [Tooltip("Bewegungsgeschwindigkeit des Gegners")]
    public float moveSpeed = 1.5f;

    [Tooltip("Minimaler Abstand zum Spieler")]
    public float minDistanceToPlayer = 1.0f;

    [Tooltip("Optional: Referenz zum Spieler (falls nicht angegeben, wird automatisch gesucht)")]
    public Transform playerTransform;

    [Tooltip("Soll der Sprite in Bewegungsrichtung gedreht werden?")]
    public bool flipSpriteToDirection = true;

    [Header("Kollisions-Einstellungen")]
    [Tooltip("Layer, die als Hindernis gelten (W�nde etc.)")]
    public LayerMask obstacleLayer;

    [Tooltip("Suchradius f�r Kollisionen")]
    public float collisionRadius = 0.5f;

    [Tooltip("Nutze Rigidbody2D f�r Bewegung (empfohlen f�r Kollisionen)")]
    public bool useRigidbody = true;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool hasAnimator = false;
    private bool hasSpriteRenderer = false;
    private Vector2 moveDirection;

    void Start()
    {
        // Hole den Rigidbody2D (wird f�r die physikbasierte Bewegung ben�tigt)
        rb = GetComponent<Rigidbody2D>();

        // Falls kein Rigidbody vorhanden ist und wir einen verwenden m�chten, erstelle einen
        if (rb == null && useRigidbody)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Deaktiviere Schwerkraft f�r Topdown-Spiel
            rb.freezeRotation = true; // Verhindere Rotation
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Bessere Kollisionserkennung
        }

        // Pr�fe, ob ein Animator vorhanden ist
        animator = GetComponent<Animator>();
        hasAnimator = animator != null;

        // Hole den SpriteRenderer f�r Flip-Funktion
        spriteRenderer = GetComponent<SpriteRenderer>();
        hasSpriteRenderer = spriteRenderer != null;

        // Falls keine Referenz zum Spieler angegeben wurde, versuche ihn zu finden
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Kein Spieler mit Tag 'Player' gefunden! Bitte weise den Spieler manuell zu.");
            }
        }
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        // Berechne die Richtung zum Spieler (ignoriere Z-Achse f�r 2D)
        Vector2 directionToPlayer = (Vector2)(playerTransform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        // Normalisiere die Richtung
        directionToPlayer.Normalize();

        // Sprite in Bewegungsrichtung drehen (falls erw�nscht)
        if (hasSpriteRenderer && flipSpriteToDirection && directionToPlayer.x != 0)
        {
            spriteRenderer.flipX = directionToPlayer.x < 0;
        }

        // Bewege den Gegner nur, wenn er weiter als der Mindestabstand entfernt ist
        if (distanceToPlayer > minDistanceToPlayer)
        {
            // Setze die Bewegungsrichtung
            moveDirection = directionToPlayer;

            // Animation kontrollieren, falls vorhanden
            if (hasAnimator)
            {
                animator.SetBool("isMoving", true);

                // Optional: Setze Richtungsparameter f�r Animationen
                // animator.SetFloat("dirX", directionToPlayer.x);
                // animator.SetFloat("dirY", directionToPlayer.y);
            }

            // Wenn kein Rigidbody verwendet wird, bewege direkt mit Kollisionserkennung
            if (!useRigidbody)
            {
                MoveWithCollisionCheck(directionToPlayer);
            }
        }
        else
        {
            // Stoppe die Bewegung
            moveDirection = Vector2.zero;

            // Animation stoppen, falls vorhanden
            if (hasAnimator)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    // Physikbasierte Bewegung mit Rigidbody2D
    void FixedUpdate()
    {
        if (useRigidbody && rb != null && moveDirection != Vector2.zero)
        {
            // Pr�fe zuerst, ob der n�chste Schritt eine Kollision verursachen w�rde
            Vector2 nextPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

            // Raycasting f�r Kollisionen
            RaycastHit2D hit = Physics2D.CircleCast(
                rb.position,
                collisionRadius,
                moveDirection,
                moveSpeed * Time.fixedDeltaTime,
                obstacleLayer
            );

            if (hit.collider == null)
            {
                // Kein Hindernis - bewege normal
                rb.MovePosition(nextPosition);
            }
            else
            {
                // Hindernis erkannt - versuche entlang des Hindernisses zu gleiten
                Vector2 slidingDirection = Vector2.Perpendicular(hit.normal).normalized;
                if (Vector2.Dot(slidingDirection, moveDirection) < 0)
                {
                    slidingDirection = -slidingDirection;
                }

                // Teste, ob Sliding in diese Richtung m�glich ist
                RaycastHit2D slideHit = Physics2D.CircleCast(
                    rb.position,
                    collisionRadius,
                    slidingDirection,
                    moveSpeed * Time.fixedDeltaTime * 0.5f,
                    obstacleLayer
                );

                if (slideHit.collider == null)
                {
                    // Gleite entlang der Wand
                    rb.MovePosition(rb.position + slidingDirection * moveSpeed * Time.fixedDeltaTime * 0.75f);
                }
            }
        }
    }

    // Bewegung ohne Rigidbody mit manueller Kollisionspr�fung
    private void MoveWithCollisionCheck(Vector2 direction)
    {
        // Pr�fe, ob der Weg frei ist
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            collisionRadius,
            direction,
            moveSpeed * Time.deltaTime,
            obstacleLayer
        );

        if (hit.collider == null)
        {
            // Kein Hindernis - bewege normal
            transform.position += new Vector3(direction.x, direction.y, 0) * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Hindernis erkannt - versuche entlang des Hindernisses zu gleiten
            Vector2 slidingDirection = Vector2.Perpendicular(hit.normal).normalized;
            if (Vector2.Dot(slidingDirection, direction) < 0)
            {
                slidingDirection = -slidingDirection;
            }

            // Teste, ob Sliding in diese Richtung m�glich ist
            RaycastHit2D slideHit = Physics2D.CircleCast(
                transform.position,
                collisionRadius,
                slidingDirection,
                moveSpeed * Time.deltaTime * 0.5f,
                obstacleLayer
            );

            if (slideHit.collider == null)
            {
                // Gleite entlang der Wand
                transform.position += new Vector3(slidingDirection.x, slidingDirection.y, 0) * moveSpeed * Time.deltaTime * 0.75f;
            }
        }
    }

    // Hilfe zur Visualisierung des Kollisionsradius im Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}