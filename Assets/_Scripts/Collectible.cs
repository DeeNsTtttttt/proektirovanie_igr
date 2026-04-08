using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1;

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

        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
        {
            ui.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}
