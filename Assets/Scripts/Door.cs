using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen = false;
    public float openRotationY = 90f;
    public float closedRotationY = 0f;
    public float rotationSpeed = 2f;

    [Header("Collider Settings")]
    public BoxCollider2D doorCollider; // Reference to the collider that blocks the player

    [Header("Interaction Settings")]
    public Transform interactionAreaPosition; // Position where the player interacts with the door
    public float interactionDistance = 1.5f;
    public KeyCode interactionKey = KeyCode.E;
    public GameObject interactionPrompt;

    private Transform playerTransform;
    private bool playerInRange = false;
    private float targetRotation;

    void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player with tag 'Player' not found!");
        }

        // Make sure we have a door collider reference
        if (doorCollider == null)
        {
            doorCollider = GetComponent<BoxCollider2D>();
            if (doorCollider == null)
            {
                Debug.LogError("No BoxCollider2D found on door! Please add one.");
            }
        }

        // Create interaction area if not set
        if (interactionAreaPosition == null)
        {
            // Create a new GameObject for the interaction area
            GameObject interactionArea = new GameObject("DoorInteractionArea");
            interactionArea.transform.parent = transform;

            // Position it in front of the door
            interactionArea.transform.localPosition = new Vector3(0, -1f, 0); // Adjust this position as needed

            interactionAreaPosition = interactionArea.transform;

            // Add a trigger collider to the interaction area
            BoxCollider2D triggerCollider = interactionArea.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector2(2f, 1f); // Size of interaction area

            Debug.Log("Created interaction area at " + interactionArea.transform.position);
        }

        // Hide the interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Set initial door state
        UpdateDoorState(isOpen);

        Debug.Log("Door initialized. Current state: " + (isOpen ? "Open" : "Closed"));
    }

    void Update()
    {
        // Check if player is in range based on distance to interaction area
        if (playerTransform != null && interactionAreaPosition != null)
        {
            float distanceToPlayer = Vector2.Distance(interactionAreaPosition.position, playerTransform.position);

            // Show/hide prompt based on distance to interaction area
            if (distanceToPlayer <= interactionDistance)
            {
                // Player entered range
                if (!playerInRange)
                {
                    playerInRange = true;
                    Debug.Log("Player entered interaction range! Distance: " + distanceToPlayer);
                    if (interactionPrompt != null)
                    {
                        interactionPrompt.SetActive(true);
                    }
                }

                // Check for interaction key press while in range
                if (Input.GetKeyDown(interactionKey))
                {
                    Debug.Log("E key pressed while in range!");
                    ToggleDoor();
                }
            }
            else if (playerInRange)
            {
                // Player left range
                playerInRange = false;
                Debug.Log("Player left interaction range!");
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }

        // Animate door rotation
        if (transform.localEulerAngles.y != targetRotation)
        {
            float newRotation = Mathf.LerpAngle(transform.localEulerAngles.y, targetRotation, Time.deltaTime * rotationSpeed);
            transform.localEulerAngles = new Vector3(0, newRotation, 0);
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        UpdateDoorState(isOpen);

        Debug.Log("Door toggled: " + (isOpen ? "Open" : "Closed"));
    }

    private void UpdateDoorState(bool open)
    {
        // Update collider based on door state
        if (doorCollider != null)
        {
            if (open)
            {
                // When open, set collider to trigger (player can walk through)
                doorCollider.isTrigger = true;
            }
            else
            {
                // When closed, set collider to non-trigger (player can't walk through)
                doorCollider.isTrigger = false;
            }
        }

        // Set target rotation
        targetRotation = open ? openRotationY : closedRotationY;
    }

    // Draw gizmos to visualize interaction area
    void OnDrawGizmosSelected()
    {
        if (interactionAreaPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactionAreaPosition.position, interactionDistance);

            // Draw line from door to interaction area
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, interactionAreaPosition.position);
        }
    }
}