using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1;
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField, Range(0f, 1f)] private float pickUpVolume = 1f;
    [SerializeField] private GameObject pickUpVfxPrefab;

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

        ResourceManager resourceManager = ResourceManager.Instance;
        if (resourceManager != null)
        {
            resourceManager.AddCoins(scoreValue);
        }
        else
        {
            UIManager ui = FindFirstObjectByType<UIManager>();
            if (ui != null)
            {
                ui.AddScore(scoreValue);
            }
        }

        AudioClip activeClip = pickUpSound != null ? pickUpSound : SyntheticSfx.GetPickupClip();
        if (activeClip != null)
        {
            AudioSource.PlayClipAtPoint(activeClip, transform.position, pickUpVolume);
        }

        if (pickUpVfxPrefab != null)
        {
            GameObject vfx = Instantiate(pickUpVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        Destroy(gameObject);
    }
}

