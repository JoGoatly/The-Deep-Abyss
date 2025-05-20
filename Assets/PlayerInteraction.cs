using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaktions-Einstellungen")]
    [Tooltip("Reichweite für Interaktionen")]
    public float interactionRange = 2.5f;

    [Tooltip("Layer für interagierbare Objekte")]
    public LayerMask interactableLayer;

    [Tooltip("Taste für Interaktion")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI-Einstellungen")]
    [Tooltip("Referenz zum UI-Element für Interaktionshinweise")]
    public GameObject interactionPromptUI;

    private Camera playerCamera;
    private Interactable currentInteractable;

    private void Start()
    {
        // Kamera des Spielers finden
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Interaktions-UI am Anfang ausblenden
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Prüfen, ob ein interagierbares Objekt im Fokus ist
        CheckForInteractable();

        // Bei Tastendruck interagieren
        if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void CheckForInteractable()
    {
        RaycastHit hit;
        Ray ray = playerCamera != null
            ? new Ray(playerCamera.transform.position, playerCamera.transform.forward)
            : new Ray(transform.position, transform.forward);

        // Raycast durchführen
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            // Prüfen, ob das getroffene Objekt interagierbar ist
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            // Wenn wir ein neues interagierbares Objekt gefunden haben
            if (interactable != null && interactable != currentInteractable)
            {
                // Altes Objekt deaktivieren
                if (currentInteractable != null)
                {
                    currentInteractable.OnEndFocus();
                }

                // Neues Objekt aktivieren
                currentInteractable = interactable;
                currentInteractable.OnStartFocus();

                // UI anzeigen
                if (interactionPromptUI != null)
                {
                    interactionPromptUI.SetActive(true);
                }
            }
        }
        else if (currentInteractable != null)
        {
            // Keine Interaktion mehr möglich
            currentInteractable.OnEndFocus();
            currentInteractable = null;

            // UI ausblenden
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }
    }

    // Gizmos für bessere Visualisierung im Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // Startposition des Raycasts ermitteln
        Vector3 rayStart;
        Vector3 rayDirection;

        if (playerCamera != null)
        {
            rayStart = playerCamera.transform.position;
            rayDirection = playerCamera.transform.forward;
        }
        else
        {
            rayStart = transform.position;
            rayDirection = transform.forward;
        }

        // Raycast visualisieren
        Gizmos.DrawLine(rayStart, rayStart + rayDirection * interactionRange);
    }
}

// Abstrakte Basisklasse für alle interagierbaren Objekte
public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Interaktionstext, der angezeigt wird")]
    public string interactionPrompt = "Drücke [E] zum Interagieren";

    // Diese Methode wird aufgerufen, wenn der Spieler mit dem Objekt interagiert
    public abstract void Interact();

    // Diese Methode wird aufgerufen, wenn der Spieler das Objekt fokussiert
    public virtual void OnStartFocus() { }

    // Diese Methode wird aufgerufen, wenn der Spieler den Fokus verliert
    public virtual void OnEndFocus() { }
}