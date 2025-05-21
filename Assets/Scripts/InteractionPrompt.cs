using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [Header("UI-Einstellungen")]
    [Tooltip("Der Text, der angezeigt werden soll")]
    public string promptText = "Drücke [E] zum Öffnen";

    [Tooltip("Das Text-Element, das den Hinweis anzeigt")]
    public TextMeshProUGUI promptTextUI;

    [Tooltip("Canvas, das den Interaktionshinweis enthält")]
    public Canvas promptCanvas;

    [Tooltip("Soll das Prompt immer zum Spieler ausgerichtet sein?")]
    public bool faceCamera = true;

    [Header("Animation")]
    [Tooltip("Soll das Prompt animiert werden?")]
    public bool animatePrompt = true;

    [Tooltip("Animations-Geschwindigkeit")]
    public float animationSpeed = 1.5f;

    [Tooltip("Bewegungshöhe der Animation")]
    public float animationHeight = 0.2f;

    private Camera mainCamera;
    private Vector3 initialPosition;
    private float animationTime;

    private void Start()
    {
        // Kamera finden
        mainCamera = Camera.main;

        // Startposition speichern
        initialPosition = transform.position;

        // Standardmäßig ausblenden
        if (promptCanvas != null)
        {
            promptCanvas.enabled = false;
        }

        // Text setzen, falls vorhanden
        if (promptTextUI != null)
        {
            promptTextUI.text = promptText;
        }
    }

    private void Update()
    {
        // Zum Spieler/Kamera ausrichten
        if (faceCamera && mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }

        // Animation
        if (animatePrompt && promptCanvas != null && promptCanvas.enabled)
        {
            animationTime += Time.deltaTime * animationSpeed;
            float yOffset = Mathf.Sin(animationTime) * animationHeight;
            transform.position = initialPosition + new Vector3(0, yOffset, 0);
        }
    }

    // Methode zum Ein-/Ausblenden des Prompts
    public void ShowPrompt(bool show)
    {
        if (promptCanvas != null)
        {
            promptCanvas.enabled = show;
        }
    }

    // Methode zum Ändern des Prompt-Textes
    public void SetPromptText(string newText)
    {
        promptText = newText;
        if (promptTextUI != null)
        {
            promptTextUI.text = newText;
        }
    }
}