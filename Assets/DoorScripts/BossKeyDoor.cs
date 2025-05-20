using UnityEngine;

public class BossKeyDoor : KeyDoor
{
    [Header("Boss Key Settings")]
    [Tooltip("Special effects when the boss door opens")]
    public ParticleSystem openingEffect;

    [Tooltip("Special sound when attempting to open without a boss key")]
    public AudioClip bossLockedSound;

    [Tooltip("Message to display when trying to open without a boss key")]
    public string bossLockedMessage = "Du benötigst den Boss-Schlüssel";

    private void Start()
    {
        // Initialize the base class
        base.Start();

        // Override the required key name
        requiredKeyName = "BossKey";

        // Set a different locked message
        lockedMessage = bossLockedMessage;
    }

    // This method is called by the player's interaction system
    new public void Interact()
    {
        // Use the base class interaction logic
        base.Interact();
    }

    // Check if the player has the boss key
    protected override bool HasKey()
    {
        // Get the player's inventory
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();

        // If inventory exists, check for the boss key
        if (inventory != null)
        {
            return inventory.HasItem("BossKey");
        }

        // For testing without inventory system:
        // return PlayerPrefs.GetInt("BossKey", 0) > 0;

        return false;
    }

    // Override to play special effects when the boss door opens
    protected override void RemoveKeyFromInventory()
    {
        base.RemoveKeyFromInventory();

        // Play special effect when the boss door opens
        if (openingEffect != null)
        {
            openingEffect.Play();
        }
    }

    // Show a message that the boss door is locked
    protected override void ShowLockedMessage()
    {
        // Play the boss locked sound if available, otherwise use the base sound
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && bossLockedSound != null)
        {
            audioSource.PlayOneShot(bossLockedSound);
        }

        // Use the base implementation to show the message
        base.ShowLockedMessage();
    }

    // Optional: Visualize in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }
}