using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerStatsPanel : MonoBehaviour
{
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private string toggleHint = "C - характеристики";
    [SerializeField] private bool showHint = true;

    private bool isOpen;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.StatsChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.StatsChanged -= Refresh;
        }
    }

    private void Update()
    {
        if (ReadTogglePressed())
        {
            Toggle();
        }

        if (isOpen)
        {
            Refresh();
        }
    }

    public void Toggle()
    {
        isOpen = !isOpen;

        if (statsPanel != null)
        {
            statsPanel.SetActive(isOpen);
        }

        Refresh();
    }

    public void Close()
    {
        isOpen = false;

        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }
    }

    private void Refresh()
    {
        if (statsText == null)
        {
            return;
        }

        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        int currentHealth = playerHealth != null ? playerHealth.CurrentHealth : 0;
        int maxHealth = playerHealth != null ? playerHealth.MaxHealth : 0;
        int damage = playerStats != null ? playerStats.TotalDamage : 0;
        int damageBonus = playerStats != null ? playerStats.DamageBonus : 0;
        float moveSpeed = playerStats != null ? playerStats.TotalMoveSpeed : 0f;
        float runSpeed = playerStats != null ? playerStats.TotalRunSpeed : 0f;
        float speedBonus = playerStats != null ? playerStats.SpeedBonus : 0f;

        statsText.text =
            $"Характеристики\n" +
            $"HP: {currentHealth}/{maxHealth}\n" +
            $"Damage: {damage} (+{damageBonus})\n" +
            $"Move Speed: {moveSpeed:0.#}\n" +
            $"Run Speed: {runSpeed:0.#}\n" +
            $"Speed Bonus: +{speedBonus:0.#}";
    }

    private void OnGUI()
    {
        if (!showHint || string.IsNullOrWhiteSpace(toggleHint))
        {
            return;
        }

        GUI.Label(new Rect(12f, Screen.height - 38f, 220f, 24f), toggleHint);
    }

    private bool ReadTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.cKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.C);
#endif
    }
}
