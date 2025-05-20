using UnityEngine;

[RequireComponent(typeof(DoorController))]
public class InteractableDoor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Text to display when player is near the door")]
    public string interactionText = "Interagiere mit E";

    [Tooltip("The radius within which the player can interact with this door")]
    public float interactionRadius = 2f;

    // Reference to the door controller
    private DoorController doorController;

    private void Start()
    {
        // Get the door controller
        doorController = GetComponent<DoorController>();

        // Set layer to Interactable (create this layer in Unity)
        // This is used for the interaction detection
        if (gameObject.layer != LayerMask.NameToLayer("Interactable"))
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
    }

    // This method is called by the player's interaction system
    public void Interact()
    {
        if (doorController != null && doorController.CanOpen())
        {
            doorController.OpenDoor();
        }
    }

    // Optional: Visualize the interaction radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}