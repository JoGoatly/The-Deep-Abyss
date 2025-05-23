using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform interactionAreaPosition; // Position where the player interacts with the door
    public float interactionDistance = 1.5f;
    public KeyCode interactionKey = KeyCode.E;
    public GameObject interactionPrompt;

    [Header("Destruction Settings")]
    public GameObject destroyEffect; // Optional particle effect when door is destroyed
    public AudioClip destroySound; // Optional sound effect when door is destroyed

    private Transform playerTransform;
    private bool playerInRange = false;
    private AudioSource audioSource;

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

        // Get audio source for sound effects
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && destroySound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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

        Debug.Log("Door initialized and ready for destruction!");
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
                    Debug.Log("E key pressed - destroying door!");
                    DestroyDoor();
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
    }

    private void DestroyDoor()
    {
        // Play destruction sound if available
        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound);
        }

        // Spawn destruction effect if available
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        // Hide interaction prompt if it exists
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        Debug.Log("Door destroyed!");

        // If we have a sound to play, wait for it to finish before destroying
        if (destroySound != null && audioSource != null)
        {
            // Detach audio source so it can finish playing
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = transform.position;
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = destroySound;
            tempSource.Play();

            // Destroy the temp audio object after the clip finishes
            Destroy(tempAudio, destroySound.length);
        }

        // Destroy the door immediately
        Destroy(gameObject);
    }

    // Draw gizmos to visualize interaction area
    void OnDrawGizmosSelected()
    {
        if (interactionAreaPosition != null)
        {
            Gizmos.color = Color.red; // Changed to red to indicate destruction
            Gizmos.DrawWireSphere(interactionAreaPosition.position, interactionDistance);

            // Draw line from door to interaction area
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, interactionAreaPosition.position);
        }
    }
}