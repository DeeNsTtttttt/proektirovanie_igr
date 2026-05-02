using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Trap : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 25;
    [SerializeField, Min(0f)] private float cooldown = 1f;
    [SerializeField] private bool damageOnStay = true;
    [SerializeField] private AudioClip damageSound;
    [SerializeField, Range(0f, 1f)] private float damageVolume = 0.8f;

    private float lastDamageTime = -Mathf.Infinity;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (damageOnStay)
        {
            TryDamage(other);
        }
    }

    private void TryDamage(Collider other)
    {
        if (!other.CompareTag("Player") || Time.time < lastDamageTime + cooldown)
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

        playerHealth.TakeDamage(damage);
        lastDamageTime = Time.time;

        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(damageSound, transform.position, damageVolume);
        }
    }
}
