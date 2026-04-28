using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private Collectible coinDropPrefab;
    [SerializeField, Min(1)] private int coinDropCount = 1;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (coinDropPrefab != null)
        {
            for (int i = 0; i < coinDropCount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.35f, 0.35f), 0.2f, Random.Range(-0.35f, 0.35f));
                Instantiate(coinDropPrefab, transform.position + offset, Quaternion.identity);
            }
        }

        if (deathVfxPrefab != null)
        {
            GameObject vfx = Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        Destroy(gameObject);
    }
}
