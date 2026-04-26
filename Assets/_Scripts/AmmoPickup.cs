using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
public class AmmoPickup : MonoBehaviour
{
    private enum InteractionMode
    {
        AutoPickup = 0,
        StationByE = 1
    }

    [Header("Common")]
    [SerializeField] private InteractionMode mode = InteractionMode.AutoPickup;
    [SerializeField, Min(1)] private int ammoAmount = 12;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 0.9f;

    [Header("Station Settings")]
    [SerializeField, Min(0f)] private float cooldown = 4f;
    [SerializeField] private bool showStatusText = true;
    [SerializeField] private string promptText = "Press E to refill ammo";
    [SerializeField] private string cooldownText = "Ammo station recharging...";

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
        if (mode != InteractionMode.StationByE || !playerInRange)
        {
            return;
        }

        if (ReadInteractPressed())
        {
            TryUseStation();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        ShooterWeapon weapon = FindWeapon(other.transform);
        if (weapon == null)
        {
            return;
        }

        if (mode == InteractionMode.AutoPickup)
        {
            weapon.AddReserveAmmo(ammoAmount);
            PlayPickupSound();
            Destroy(gameObject);
            return;
        }

        playerInRange = true;
        playerWeapon = weapon;
    }

    private void OnTriggerExit(Collider other)
    {
        if (mode != InteractionMode.StationByE || !other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
        playerWeapon = null;
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

    private void TryUseStation()
    {
        if (playerWeapon == null || Time.time < nextReadyTime)
        {
            return;
        }

        playerWeapon.AddReserveAmmo(ammoAmount);
        nextReadyTime = Time.time + cooldown;
        PlayPickupSound();
    }

    private void PlayPickupSound()
    {
        AudioClip activeClip = pickupSound != null ? pickupSound : SyntheticSfx.GetPickupClip();
        AudioSource.PlayClipAtPoint(activeClip, transform.position, pickupVolume);
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
        if (mode != InteractionMode.StationByE || !showStatusText || !playerInRange)
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
