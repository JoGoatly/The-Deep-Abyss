using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    [Header("Coin Settings")]
    public int coins = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI coinText;
    // If you're using legacy UI Text component instead of TextMeshPro:
    // public Text coinText;

    [Header("Effects")]
    public bool animateTextOnCollect = true;
    public float animationDuration = 0.5f;

    private void Start()
    {
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;

        if (animateTextOnCollect)
        {
            StartCoroutine(AnimateCoinText());
        }
        else
        {
            UpdateUI();
        }
    }

    public bool SpendCoins(int amount)
    {
        // Check if player has enough coins
        if (coins >= amount)
        {
            coins -= amount;
            UpdateUI();
            return true;
        }

        return false;
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }

    private IEnumerator AnimateCoinText()
    {
        if (coinText != null)
        {
            // Store original scale and color
            Vector3 originalScale = coinText.transform.localScale;
            Color originalColor = coinText.color;

            // Update the displayed value
            coinText.text = coins.ToString();

            // Animate scale up
            float elapsed = 0f;
            while (elapsed < animationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration / 2);

                // Scale up and change color slightly
                coinText.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, t);
                coinText.color = Color.Lerp(originalColor, Color.yellow, t);

                yield return null;
            }

            // Animate scale back to normal
            elapsed = 0f;
            while (elapsed < animationDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (animationDuration / 2);

                // Scale back down and restore color
                coinText.transform.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, t);
                coinText.color = Color.Lerp(Color.yellow, originalColor, t);

                yield return null;
            }

            // Ensure we end at the original scale and color
            coinText.transform.localScale = originalScale;
            coinText.color = originalColor;
        }
    }
}