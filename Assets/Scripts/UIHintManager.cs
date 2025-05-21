using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHintManager : MonoBehaviour
{
    [Header("Movement Hint")]
    [Tooltip("Reference to the movement hint text")]
    public TMP_Text movementHintText;

    [Tooltip("Duration the movement hint stays visible (seconds)")]
    public float movementHintDuration = 10f;

    [Header("Interaction Hint")]
    [Tooltip("Reference to the interaction hint text")]
    public TMP_Text interactionHintText;

    [Tooltip("Detection radius for interactable objects")]
    public float interactionRadius = 2f;

    [Tooltip("Layer mask for interactable objects")]
    public LayerMask interactableLayer;

    // Private variables
    private bool hasShownMovementHint = false;
    private float movementHintTimer = 0f;
    private bool isMovementHintActive = false;

    void Start()
    {
        // Hide both hints at the start
        if (movementHintText != null)
        {
            movementHintText.gameObject.SetActive(false);
        }

        if (interactionHintText != null)
        {
            interactionHintText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Check for WASD input if the hint hasn't been shown yet
        if (!hasShownMovementHint)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                ShowMovementHint();
            }
        }

        // Count down the timer if the movement hint is active
        if (isMovementHintActive)
        {
            movementHintTimer -= Time.deltaTime;

            if (movementHintTimer <= 0f)
            {
                HideMovementHint();
            }
        }

        // Check for nearby interactable objects
        CheckForInteractables();
    }

    void ShowMovementHint()
    {
        if (movementHintText != null)
        {
            movementHintText.gameObject.SetActive(true);
            movementHintText.text = "Bewege dich mit WASD";
            hasShownMovementHint = true;
            isMovementHintActive = true;
            movementHintTimer = movementHintDuration;
        }
    }

    void HideMovementHint()
    {
        if (movementHintText != null)
        {
            movementHintText.gameObject.SetActive(false);
            isMovementHintActive = false;
        }
    }

    void CheckForInteractables()
    {
        // Find all colliders in the interaction radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);

        if (colliders.Length > 0)
        {
            // At least one interactable object is nearby, show the hint
            ShowInteractionHint();
        }
        else
        {
            // No interactable objects nearby, hide the hint
            HideInteractionHint();
        }
    }

    void ShowInteractionHint()
    {
        if (interactionHintText != null && !interactionHintText.gameObject.activeSelf)
        {
            interactionHintText.gameObject.SetActive(true);
            interactionHintText.text = "Interagiere mit E";
        }
    }

    void HideInteractionHint()
    {
        if (interactionHintText != null && interactionHintText.gameObject.activeSelf)
        {
            interactionHintText.gameObject.SetActive(false);
        }
    }

    // Optional: Visualize the interaction radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}