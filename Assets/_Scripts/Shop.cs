using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
public class Shop : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private string promptText = "Нажмите E, чтобы открыть магазин";
    [SerializeField] private string closeText = "Esc - закрыть магазин";
    [SerializeField] private bool showPrompt = true;
    [SerializeField] private bool showCursorWhenOpen = true;

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text healthButtonText;
    [SerializeField] private TMP_Text damageButtonText;
    [SerializeField] private TMP_Text speedButtonText;

    [Header("Upgrade Costs")]
    [SerializeField, Min(0)] private int healthUpgradeCost = 50;
    [SerializeField, Min(0)] private int damageUpgradeCost = 40;
    [SerializeField, Min(0)] private int speedUpgradeCost = 30;
    [SerializeField, Min(0)] private int costIncreaseAfterBuy = 15;

    [Header("Upgrade Values")]
    [SerializeField, Min(1)] private int healthUpgradeAmount = 20;
    [SerializeField, Min(1)] private int damageUpgradeAmount = 5;
    [SerializeField, Min(0.1f)] private float speedUpgradeAmount = 1f;

    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private PlayerStats playerStats;

    private bool playerInRange;
    private bool isOpen;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
        }

        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    private void Start()
    {
        UpdateButtonTexts();
    }

    private void Update()
    {
        if (playerInRange && ReadInteractPressed())
        {
            ToggleShop();
        }

        if (isOpen && ReadClosePressed())
        {
            CloseShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;

        if (playerStats == null)
        {
            playerStats = other.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = other.GetComponentInParent<PlayerStats>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
        CloseShop();
    }

    public void ToggleShop()
    {
        if (isOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }

    public void OpenShop()
    {
        isOpen = true;

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        if (showCursorWhenOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        SetStatus("Выберите улучшение за монеты.");
        UpdateButtonTexts();
    }

    public void CloseShop()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void BuyHealthUpgrade()
    {
        if (!TrySpend(healthUpgradeCost))
        {
            return;
        }

        EnsurePlayerStats();
        if (playerStats != null)
        {
            playerStats.AddMaxHealthBonus(healthUpgradeAmount, true);
        }

        healthUpgradeCost += costIncreaseAfterBuy;
        SetStatus($"Максимальное здоровье увеличено на {healthUpgradeAmount}.");
        UpdateButtonTexts();
    }

    public void BuyDamageUpgrade()
    {
        if (!TrySpend(damageUpgradeCost))
        {
            return;
        }

        EnsurePlayerStats();
        if (playerStats != null)
        {
            playerStats.AddDamageBonus(damageUpgradeAmount);
        }

        damageUpgradeCost += costIncreaseAfterBuy;
        SetStatus($"Урон увеличен на {damageUpgradeAmount}.");
        UpdateButtonTexts();
    }

    public void BuySpeedUpgrade()
    {
        if (!TrySpend(speedUpgradeCost))
        {
            return;
        }

        EnsurePlayerStats();
        if (playerStats != null)
        {
            playerStats.AddSpeedBonus(speedUpgradeAmount);
        }

        speedUpgradeCost += costIncreaseAfterBuy;
        SetStatus($"Скорость увеличена на {speedUpgradeAmount:0.#}.");
        UpdateButtonTexts();
    }

    private bool TrySpend(int cost)
    {
        if (resourceManager == null)
        {
            resourceManager = ResourceManager.Instance != null
                ? ResourceManager.Instance
                : FindFirstObjectByType<ResourceManager>();
        }

        if (resourceManager == null)
        {
            SetStatus("ResourceManager не найден на сцене.");
            return false;
        }

        if (!resourceManager.SpendCoins(cost))
        {
            SetStatus($"Недостаточно монет. Нужно: {cost}.");
            return false;
        }

        return true;
    }

    private void EnsurePlayerStats()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }
    }

    private void UpdateButtonTexts()
    {
        if (healthButtonText != null)
        {
            healthButtonText.text = $"+{healthUpgradeAmount} Max HP - {healthUpgradeCost} coins";
        }

        if (damageButtonText != null)
        {
            damageButtonText.text = $"+{damageUpgradeAmount} Damage - {damageUpgradeCost} coins";
        }

        if (speedButtonText != null)
        {
            speedButtonText.text = $"+{speedUpgradeAmount:0.#} Speed - {speedUpgradeCost} coins";
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void OnGUI()
    {
        if (!showPrompt || !playerInRange)
        {
            return;
        }

        string text = isOpen ? closeText : promptText;
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        const float width = 360f;
        const float height = 28f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height - 110f;
        GUI.Label(new Rect(x, y, width, height), text);
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

    private bool ReadClosePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
