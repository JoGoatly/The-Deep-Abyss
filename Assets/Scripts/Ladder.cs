using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LadderInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform interactionAreaPosition; // Position where the player interacts with the door
    public float interactionDistance = 1.5f;
    public KeyCode interactionKey = KeyCode.E;
    public GameObject interactionPrompt;
    public string SceneToLoad;

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
                    if (interactionPrompt != null)
                    {
                        interactionPrompt.SetActive(true);
                    }
                }

                // Check for interaction key press while in range
                if (Input.GetKeyDown(interactionKey))
                {
                    LoadScene();
                }
            }
            else if (playerInRange)
            {
                // Player left range
                playerInRange = false;
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(SceneToLoad);
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