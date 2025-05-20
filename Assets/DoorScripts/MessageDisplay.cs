using System.Collections;
using UnityEngine;
using TMPro;

public class MessageDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Reference to the text component for displaying messages")]
    public TMP_Text messageText;

    [Header("Animation Settings")]
    [Tooltip("Should the message fade in/out")]
    public bool useFadeAnimation = true;

    [Tooltip("Duration of fade in/out (seconds)")]
    public float fadeDuration = 0.5f;

    // Currently active coroutine
    private Coroutine activeMessageCoroutine;

    private void Start()
    {
        // Make sure the message text is hidden at start
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MessageDisplay: No text component assigned!");
        }
    }

    // Show a message for the specified duration
    public void ShowMessage(string message, float duration = 2f)
    {
        if (messageText == null)
            return;

        // Stop any active message display
        if (activeMessageCoroutine != null)
        {
            StopCoroutine(activeMessageCoroutine);
        }

        // Start new message display
        activeMessageCoroutine = StartCoroutine(DisplayMessageCoroutine(message, duration));
    }

    // Coroutine for displaying messages with fade effects
    private IEnumerator DisplayMessageCoroutine(string message, float duration)
    {
        // Set the message text
        messageText.text = message;

        // Show the message
        messageText.gameObject.SetActive(true);

        // Fade in
        if (useFadeAnimation)
        {
            yield return FadeTextAlpha(0f, 1f, fadeDuration);
        }

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Fade out
        if (useFadeAnimation)
        {
            yield return FadeTextAlpha(1f, 0f, fadeDuration);
        }

        // Hide the message
        messageText.gameObject.SetActive(false);

        activeMessageCoroutine = null;
    }

    // Fade the text alpha from startAlpha to endAlpha over duration seconds
    private IEnumerator FadeTextAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = messageText.color;
        startColor.a = startAlpha;

        Color endColor = startColor;
        endColor.a = endAlpha;

        messageText.color = startColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            messageText.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        messageText.color = endColor;
    }
}