using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private bool showStatusText = true;
    [SerializeField] private string promptText = "Press E to activate checkpoint";
    [SerializeField] private string activatedText = "Checkpoint activated";
    [SerializeField] private AudioClip activateSound;
    [SerializeField, Range(0f, 1f)] private float activateVolume = 0.8f;

    private bool activated;
    private bool playerInRange;
    private PlayerHealth playerHealth;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        respawnPoint = transform;
    }

    private void Awake()
    {
        if (respawnPoint == null)
        {
            respawnPoint = transform;
        }

        RefreshVisual();
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (ReadInteractPressed())
        {
            ActivateCheckpoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInChildren<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            return;
        }

        playerInRange = true;
        this.playerHealth = playerHealth;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
        playerHealth = null;
    }

    private void ActivateCheckpoint()
    {
        if (playerHealth == null)
        {
            return;
        }

        activated = true;
        playerHealth.SetRespawnPoint(respawnPoint);
        PlayActivateSound();
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (activeVisual != null)
        {
            activeVisual.SetActive(activated);
        }
    }

    private void PlayActivateSound()
    {
        if (activateSound != null)
        {
            AudioSource.PlayClipAtPoint(activateSound, transform.position, activateVolume);
        }
    }

    private bool ReadInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    private void OnGUI()
    {
        if (!showStatusText || !playerInRange)
        {
            return;
        }

        string text = activated ? activatedText : promptText;
        const float width = 280f;
        const float height = 30f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height - 140f;
        GUI.Label(new Rect(x, y, width, height), text);
    }
}
