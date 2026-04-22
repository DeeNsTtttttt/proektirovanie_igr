using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [SerializeField] private string promptText = "Нажмите E";
    [SerializeField] private bool showPrompt = true;
    [SerializeField] private UnityEvent onInteract;

    private bool playerInRange;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (ReadInteractPressed())
        {
            onInteract?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void OnGUI()
    {
        if (!showPrompt || !playerInRange || string.IsNullOrWhiteSpace(promptText))
        {
            return;
        }

        const float width = 220f;
        const float height = 28f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height - 80f;
        GUI.Label(new Rect(x, y, width, height), promptText);
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
}
