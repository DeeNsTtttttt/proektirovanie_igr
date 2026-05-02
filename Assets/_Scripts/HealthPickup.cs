using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HealthPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int healAmount = 25;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 0.8f;
    [SerializeField] private GameObject pickupVfxPrefab;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
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

        playerHealth.Heal(healAmount);
        PlayPickupSound();
        SpawnPickupVfx();
        Destroy(gameObject);
    }

    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        }
    }

    private void SpawnPickupVfx()
    {
        if (pickupVfxPrefab == null)
        {
            return;
        }

        GameObject vfx = Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);
        Destroy(vfx, 2f);
    }
}
