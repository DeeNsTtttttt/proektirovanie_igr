using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField, Min(0f)] private float invincibilityDuration = 1.5f;
    [SerializeField] private bool reloadSceneOnDeath = false;
    [SerializeField] private Transform respawnPoint;

    private int currentHealth;
    private bool isDead;
    private bool isInvincible;
    private float invincibilityTimer;
    private Vector3 startPosition;

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

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        startPosition = transform.position;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        UpdateHealthUI();
    }

    private void Update()
    {
        if (!isInvincible)
        {
            return;
        }

        invincibilityTimer -= Time.deltaTime;
        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible || damage <= 0)
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
        StartInvincibility();
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }

    public void SetRespawnPoint(Transform point)
    {
        if (point != null)
        {
            respawnPoint = point;
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (reloadSceneOnDeath || gameOverPanel == null)
        {
            RestartLevel();
            return;
        }

        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel()
    {
        if (!reloadSceneOnDeath && respawnPoint != null)
        {
            RespawnAtCheckpoint();
            return;
        }

        FullRestartLevel();
    }

    public void FullRestartLevel()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void RespawnAtCheckpoint()
    {
        Transform target = respawnPoint;
        Vector3 position = target != null ? target.position : startPosition;

        Time.timeScale = 1f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = position;
        currentHealth = maxHealth;
        isDead = false;
        StartInvincibility();
        UpdateHealthUI();
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

    private void StartInvincibility()
    {
        if (invincibilityDuration <= 0f)
        {
            return;
        }

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }
}
