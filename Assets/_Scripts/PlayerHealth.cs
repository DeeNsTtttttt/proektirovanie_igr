using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TMP_Text healthText;

    private int currentHealth;
    private bool isDead;

    private void Awake()
    {
        // Common setup mistake: PlayerHealth accidentally added to UI text object.
        if (!CompareTag("Player") && GetComponent<RectTransform>() != null)
        {
            Debug.LogWarning(
                $"PlayerHealth is attached to UI object '{name}'. Disable this component and keep PlayerHealth only on Player.",
                this);
            enabled = false;
            return;
        }

        if (healthText == null)
        {
            TryAutoAssignHealthText();
        }

        currentHealth = maxHealth;
    }

    private void Start()
    {
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateHealthUI();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void UpdateHealthUI()
    {
        if (healthText == null)
        {
            TryAutoAssignHealthText();
        }

        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}";
        }
    }

    private void TryAutoAssignHealthText()
    {
        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        for (int i = 0; i < allTexts.Length; i++)
        {
            TMP_Text txt = allTexts[i];
            if (txt == null)
            {
                continue;
            }

            if (txt.name == "PlayerHealth" || txt.name.Contains("Health"))
            {
                healthText = txt;
                return;
            }
        }
    }
}
