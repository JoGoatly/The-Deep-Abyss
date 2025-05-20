using UnityEngine;

public class CollectableKey : MonoBehaviour
{
    [Header("Key Settings")]
    [Tooltip("Type of key (standard key or boss key)")]
    public string keyType = "Key";  // Use "Key" or "BossKey"

    [Tooltip("Should the key be destroyed when collected?")]
    public bool destroyOnCollect = true;

    [Tooltip("Message to display when key is collected")]
    public string collectMessage = "Schlüssel gefunden!";

    [Tooltip("Sound to play when collected")]
    public AudioClip collectSound;

    [Tooltip("Visual effect when collected")]
    public GameObject collectEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision is with the player
        if (collision.CompareTag("Player"))
        {
            CollectKey(collision.gameObject);
        }
    }

    private void CollectKey(GameObject player)
    {
        // Try to find the player's inventory
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (inventory != null)
        {
            // Add the key to the inventory
            bool added = inventory.AddItem(keyType);

            if (added)
            {
                // Show collection message
                ShowCollectMessage();

                // Play collection sound
                PlayCollectSound();

                // Show collection effect
                ShowCollectEffect();

                // Destroy or deactivate the key object
                if (destroyOnCollect)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("Player does not have an inventory component!");

            // Alternative: Set key directly in PlayerPrefs for testing
            PlayerPrefs.SetInt(keyType, 1);
            PlayerPrefs.Save();

            // Show message, play sound, etc. even without inventory
            ShowCollectMessage();
            PlayCollectSound();
            ShowCollectEffect();

            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void ShowCollectMessage()
    {
        // Find message display system
        MessageDisplay messageDisplay = FindObjectOfType<MessageDisplay>();

        if (messageDisplay != null)
        {
            // Set custom messages for different key types
            string message = collectMessage;
            if (keyType == "BossKey")
            {
                message = "Boss-Schlüssel gefunden!";
            }

            messageDisplay.ShowMessage(message, 2f);
        }
    }

    private void PlayCollectSound()
    {
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, 1f);
        }
    }

    private void ShowCollectEffect()
    {
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
    }

    // Optional: Add floating or rotation animation
    void Update()
    {
        // Simple floating animation
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + Mathf.Sin(Time.time * 2) * 0.005f,
            transform.position.z
        );

        // Simple rotation
        transform.Rotate(0, 0, 30 * Time.deltaTime);
    }
}