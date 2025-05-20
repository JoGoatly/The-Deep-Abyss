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

    [Header("Sichtradius-Einstellungen")]
    [Tooltip("Radius in dem der Gegner den Spieler erkennen kann")]
    public float sightRadius = 7.0f;

    [Tooltip("Aktiviert die Visualisierung des Sichtradius im Editor")]
    public bool showSightRadius = true;

    [Tooltip("Wenn aktiviert, kehrt der Gegner zur Startposition zurück, wenn der Spieler nicht mehr in Sicht ist")]
    public bool returnToStartPosition = true;

    [Header("Kollisions-Einstellungen")]
    [Tooltip("Layer, die als Hindernis gelten (Wände etc.)")]
    public LayerMask obstacleLayer;

    [Tooltip("Layer des Spielers für Sichtprüfung")]
    public LayerMask playerLayer;

    [Tooltip("Suchradius für Kollisionen")]
    public float collisionRadius = 0.5f;

    [Tooltip("Nutze Rigidbody2D für Bewegung (empfohlen für Kollisionen)")]
    public bool useRigidbody = true;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool hasAnimator = false;
    private bool hasSpriteRenderer = false;
    private Vector2 moveDirection;
    private Vector3 startPosition;
    private bool isPlayerInSight = false;
    private bool isReturningToStart = false;

    void Start()
    {
        // Speichere die Startposition für die Rückkehrfunktion
        startPosition = transform.position;

        // Hole den Rigidbody2D (wird für die physikbasierte Bewegung benötigt)
        rb = GetComponent<Rigidbody2D>();

        // Falls kein Rigidbody vorhanden ist und wir einen verwenden möchten, erstelle einen
        if (rb == null && useRigidbody)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Deaktiviere Schwerkraft für Topdown-Spiel
            rb.freezeRotation = true; // Verhindere Rotation
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Bessere Kollisionserkennung
        }

        // Prüfe, ob ein Animator vorhanden ist
        animator = GetComponent<Animator>();
        hasAnimator = animator != null;

        // Hole den SpriteRenderer für Flip-Funktion
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

        // Prüfe, ob der Spieler im Sichtradius ist
        CheckPlayerInSight();

        // Wenn der Spieler in Sicht ist, verfolge ihn
        if (isPlayerInSight)
        {
            isReturningToStart = false;
            ChasePlayer();
        }
        // Ansonsten zur Startposition zurückkehren, falls aktiviert
        else if (returnToStartPosition && !isNearStartPosition())
        {
            isReturningToStart = true;
            ReturnToStart();
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

    // Prüfe, ob der Spieler im Sichtradius ist
    void CheckPlayerInSight()
    {
        if (playerTransform == null)
            return;

        // Berechne Distanz zum Spieler
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Prüfe, ob der Spieler innerhalb des Sichtradius ist
        if (distanceToPlayer <= sightRadius)
        {
            // Optional: Sichtlinienprüfung mit Raycast
            RaycastHit2D hit = Physics2D.Linecast(
                transform.position,
                playerTransform.position,
                obstacleLayer
            );

            // Wenn es keine Hindernisse zwischen Gegner und Spieler gibt
            if (hit.collider == null)
            {
                isPlayerInSight = true;
                return;
            }
        }

        isPlayerInSight = false;
    }

    // Verfolgungslogik für den Spieler
    void ChasePlayer()
    {
        if (playerTransform == null)
            return;

        // Berechne die Richtung zum Spieler (ignoriere Z-Achse für 2D)
        Vector2 directionToPlayer = (Vector2)(playerTransform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;

        // Normalisiere die Richtung
        directionToPlayer.Normalize();

        // Sprite in Bewegungsrichtung drehen (falls erwünscht)
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

                // Optional: Setze Richtungsparameter für Animationen
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

    // Zur Startposition zurückkehren
    void ReturnToStart()
    {
        // Berechne Richtung zur Startposition
        Vector2 directionToStart = (Vector2)(startPosition - transform.position);
        float distanceToStart = directionToStart.magnitude;

        // Normalisiere die Richtung
        directionToStart.Normalize();

        // Sprite in Bewegungsrichtung drehen (falls erwünscht)
        if (hasSpriteRenderer && flipSpriteToDirection && directionToStart.x != 0)
        {
            spriteRenderer.flipX = directionToStart.x < 0;
        }

        // Setze die Bewegungsrichtung
        moveDirection = directionToStart;

        // Animation kontrollieren, falls vorhanden
        if (hasAnimator)
        {
            animator.SetBool("isMoving", true);
        }

        // Wenn kein Rigidbody verwendet wird, bewege direkt mit Kollisionserkennung
        if (!useRigidbody)
        {
            MoveWithCollisionCheck(directionToStart);
        }
    }

    // Prüfe, ob der Gegner nahe der Startposition ist
    bool isNearStartPosition()
    {
        return Vector2.Distance(transform.position, startPosition) < 0.1f;
    }

    // Physikbasierte Bewegung mit Rigidbody2D
    void FixedUpdate()
    {
        if (useRigidbody && rb != null && moveDirection != Vector2.zero)
        {
            // Prüfe zuerst, ob der nächste Schritt eine Kollision verursachen würde
            Vector2 nextPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

            // Raycasting für Kollisionen
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

                // Teste, ob Sliding in diese Richtung möglich ist
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

    // Bewegung ohne Rigidbody mit manueller Kollisionsprüfung
    private void MoveWithCollisionCheck(Vector2 direction)
    {
        // Prüfe, ob der Weg frei ist
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

            // Teste, ob Sliding in diese Richtung möglich ist
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

    // Visualisiere den Sichtradius im Unity-Editor
    private void OnDrawGizmos()
    {
        if (showSightRadius)
        {
            // Zeige den Sichtradius in Gelb an (oder in Rot, wenn der Spieler gesehen wird)
            Gizmos.color = isPlayerInSight ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRadius);

            // Zeige die Startposition an
            if (returnToStartPosition && Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(startPosition, 0.3f);
            }
        }
    }
}