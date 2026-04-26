using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
public class AmmoStation : MonoBehaviour
{
    [SerializeField, Min(1)] private int ammoPerUse = 20;
    [SerializeField, Min(0f)] private float cooldown = 5f;
    [SerializeField] private bool showStatusText = true;
    [SerializeField] private string promptText = "Press E to refill ammo";
    [SerializeField] private string cooldownText = "Ammo station recharging...";
    [SerializeField] private AudioClip useClip;
    [SerializeField, Range(0f, 1f)] private float useVolume = 0.85f;

    private ShooterWeapon playerWeapon;
    private bool playerInRange;
    private float nextReadyTime;

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
            TryUse();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
        playerWeapon = FindWeapon(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
        playerWeapon = null;
    }

    public void TryUse()
    {
        TryUse(playerWeapon);
    }

    public void TryUse(ShooterWeapon weapon)
    {
        if (weapon == null)
        {
            return;
        }

        if (Time.time < nextReadyTime)
        {
            return;
        }

        weapon.AddReserveAmmo(ammoPerUse);
        nextReadyTime = Time.time + cooldown;

        AudioClip activeClip = useClip != null ? useClip : SyntheticSfx.GetPickupClip();
        AudioSource.PlayClipAtPoint(activeClip, transform.position, useVolume);
    }

    private ShooterWeapon FindWeapon(Transform playerRoot)
    {
        ShooterWeapon weapon = playerRoot.GetComponentInChildren<ShooterWeapon>();
        if (weapon == null)
        {
            weapon = FindFirstObjectByType<ShooterWeapon>();
        }

        return weapon;
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

        string text = Time.time < nextReadyTime ? cooldownText : promptText;
        const float width = 280f;
        const float height = 30f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height - 110f;
        GUI.Label(new Rect(x, y, width, height), text);
    }
}
