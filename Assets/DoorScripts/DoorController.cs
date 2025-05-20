using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("Should this door play a sound when opening?")]
    public bool playSoundOnOpen = true;

    [Tooltip("Door open sound effect")]
    public AudioClip openSound;

    [Tooltip("Volume for door sounds")]
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;

    [Tooltip("Should the door be destroyed when opened (instead of just being deactivated)?")]
    public bool destroyOnOpen = false;

    [Header("Events")]
    [Tooltip("Event triggered when the door opens")]
    public UnityEvent onDoorOpen;

    // Reference to audio source
    private AudioSource audioSource;

    private void Start()
    {
        // Add an audio source if sound is enabled and none exists
        if (playSoundOnOpen && openSound != null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1.0f; // 3D sound
                audioSource.volume = soundVolume;
            }
        }
    }

    public virtual bool CanOpen()
    {
        // Regular doors can always be opened
        return true;
    }

    public virtual void OpenDoor()
    {
        // Only open if we can
        if (!CanOpen()) return;

        // Play sound if enabled
        if (playSoundOnOpen && openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound, soundVolume);
        }

        // Invoke events
        onDoorOpen?.Invoke();

        // Handle the door visualization (either destroy or deactivate)
        if (destroyOnOpen)
        {
            // Wait for sound to play before destroying
            if (playSoundOnOpen && openSound != null)
            {
                Destroy(gameObject, openSound.length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}