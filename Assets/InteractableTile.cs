using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionInteractable : MonoBehaviour
{
    [Header("UI Prompt")]
    [SerializeField] private GameObject interactPrompt;  // private but still assignable in Inspector

    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;

    private bool playerInRange = false;

    void Awake()
    {
        if (interactPrompt == null)
            Debug.LogError($"[SceneTransitionInteractable] No InteractPrompt assigned on '{gameObject.name}'");
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            LoadNewScene();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            interactPrompt?.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            interactPrompt?.SetActive(false);
        }
    }

    private void LoadNewScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[SceneTransitionInteractable] sceneToLoad is empty!");
            return;
        }
        SceneManager.LoadScene(sceneToLoad);
    }
}
