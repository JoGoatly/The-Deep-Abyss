using UnityEngine;

[RequireComponent(typeof(DoorController))]
public class KeyDoor : MonoBehaviour
{
    [Header("Key Settings")]
    [Tooltip("The name of the key item needed to open this door")]
    public string requiredKeyName = "Key";

    [Tooltip("Message to display when trying to open without a key")]
    public string lockedMessage = "Du benötigst einen Schlüssel";

    [Tooltip("Sound to play when the door is locked")]
    public AudioClip lockedSound;

    [Tooltip("The duration to display the locked message")]
    public float messageDisplayTime = 2f;

    // Reference to components
    private DoorController doorController;
    private AudioSource audioSource;

    public void Start()
    {
        // Get the door controller
        doorController = GetComponent<DoorController>();

        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && lockedSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
        }

        // Set layer to Interactable
        if (gameObject.layer != LayerMask.NameToLayer("Interactable"))
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
    }

    // This method is called by the player's interaction system
    public void Interact()
    {
        // Check if the player has the required key
        if (HasKey())
        {
            // Player has the key, open the door
            doorController.OpenDoor();

            // Optionally, remove the key from inventory
            RemoveKeyFromInventory();
        }
        else
        {
            // Player doesn't have the key, show locked message
            ShowLockedMessage();

            // Play locked sound
            if (audioSource != null && lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
        }
    }

    // Check if the player has the required key
    protected virtual bool HasKey()
    {
        // Get the player's inventory
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();

        // If inventory exists, check for the key
        if (inventory != null)
        {
            return inventory.HasItem(requiredKeyName);
        }

        // For testing without inventory system, you can use this alternative:
        // return PlayerPrefs.GetInt(requiredKeyName, 0) > 0;

        return false;
    }

    // Remove the key from the player's inventory
    protected virtual void RemoveKeyFromInventory()
    {
        // Get the player's inventory
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();

        // If inventory exists, remove the key
        if (inventory != null)
        {
            inventory.RemoveItem(requiredKeyName);
        }

        // For testing without inventory system:
        // PlayerPrefs.SetInt(requiredKeyName, 0);
    }

    // Show a message that the door is locked
    protected virtual void ShowLockedMessage()
    {
        // Find message display system
        MessageDisplay messageDisplay = FindObjectOfType<MessageDisplay>();

        if (messageDisplay != null)
        {
            messageDisplay.ShowMessage(lockedMessage, messageDisplayTime);
        }
        else
        {
            // Fallback: print to console
            Debug.Log(lockedMessage);
        }
    }

    // Optional: Visualize in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }
}