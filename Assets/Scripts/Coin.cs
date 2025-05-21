using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    public int value = 1;
    public float magnetRadius = 3f;
    public float moveSpeed = 10f;
    public float accelerationRate = 1.5f;

    [Header("References")]
    private GameObject player;
    private PlayerWallet playerWallet;
    private Transform coinTransform;
    private CircleCollider2D magnetCollider;
    private CircleCollider2D coinCollider;

    private bool isAttracting = false;
    private float currentSpeed;

    private void Awake()
    {
        coinTransform = transform;

        // Create the coin collider for collection
        coinCollider = gameObject.AddComponent<CircleCollider2D>();
        coinCollider.radius = 0.3f; // Adjust based on your coin size
        coinCollider.isTrigger = true;

        // Create the magnet trigger collider
        GameObject magnetTrigger = new GameObject("MagnetTrigger");
        magnetTrigger.transform.parent = transform;
        magnetTrigger.transform.localPosition = Vector3.zero;

        // Check if "Triggers" layer exists, if not use default layer
        int triggerLayer = LayerMask.NameToLayer("Triggers");
        if (triggerLayer != -1)
        {
            magnetTrigger.layer = triggerLayer;
        }

        magnetCollider = magnetTrigger.AddComponent<CircleCollider2D>();
        magnetCollider.radius = magnetRadius;
        magnetCollider.isTrigger = true;

        // Add a trigger script to the magnet collider
        MagnetTrigger trigger = magnetTrigger.AddComponent<MagnetTrigger>();
        trigger.parentCoin = this;

        // Find player reference
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerWallet = player.GetComponent<PlayerWallet>();
            if (playerWallet == null)
            {
                Debug.LogError("PlayerWallet component not found on player!");
            }
        }
        else
        {
            Debug.LogError("Player not found! Make sure your player has the 'Player' tag.");
        }

        currentSpeed = moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the player touches the coin itself
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    // Called by the MagnetTrigger component when player enters the magnetic field
    public void StartAttracting()
    {
        isAttracting = true;
    }

    private void Update()
    {
        if (isAttracting && player != null)
        {
            // Move coin towards player
            Vector3 direction = (player.transform.position - coinTransform.position).normalized;

            // Accelerate over time
            currentSpeed += accelerationRate * Time.deltaTime;

            coinTransform.position += direction * currentSpeed * Time.deltaTime;

            // Check if coin is close enough to be collected
            float distanceToPlayer = Vector3.Distance(coinTransform.position, player.transform.position);
            if (distanceToPlayer < 0.5f) // Adjust collection distance threshold
            {
                CollectCoin();
            }
        }
    }

    private void CollectCoin()
    {
        if (playerWallet != null)
        {
            // Add coin value to player's wallet
            playerWallet.AddCoins(value);

            // Optional: play sound effect, particles, etc.

            // Destroy the coin object
            Destroy(gameObject);
        }
    }
}

// Helper class to detect when player enters magnetic field
public class MagnetTrigger : MonoBehaviour
{
    [HideInInspector] public Coin parentCoin;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && parentCoin != null)
        {
            parentCoin.StartAttracting();
        }
    }
}