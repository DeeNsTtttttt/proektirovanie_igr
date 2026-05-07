using System;
using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Resources")]
    [SerializeField, Min(0)] private int startCoins = 0;
    [SerializeField] private bool useUIManagerScore = true;

    [Header("UI")]
    [SerializeField] private TMP_Text coinText;

    [Header("Optional Compatibility")]
    [SerializeField] private UIManager uiManager;

    private int coins;

    public int Coins => useUIManagerScore && uiManager != null ? uiManager.Score : coins;
    public event Action<int> CoinsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        coins = startCoins;
    }

    private void OnEnable()
    {
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        if (uiManager != null)
        {
            uiManager.ScoreChanged += HandleScoreChanged;
        }
    }

    private void OnDisable()
    {
        if (uiManager != null)
        {
            uiManager.ScoreChanged -= HandleScoreChanged;
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (useUIManagerScore && uiManager != null)
        {
            uiManager.AddScore(amount);
        }
        else
        {
            coins += amount;
            HandleScoreChanged(coins);
        }
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Coins < amount)
        {
            RefreshUI();
            return false;
        }

        if (useUIManagerScore && uiManager != null)
        {
            if (!uiManager.TrySpendScore(amount))
            {
                RefreshUI();
                return false;
            }
        }
        else
        {
            coins -= amount;
            HandleScoreChanged(coins);
        }

        return true;
    }

    public void SetCoinText(TMP_Text text)
    {
        coinText = text;
        RefreshUI();
    }

    public void SetCoins(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (useUIManagerScore && uiManager != null)
        {
            uiManager.SetScore(safeAmount);
        }
        else
        {
            coins = safeAmount;
            HandleScoreChanged(coins);
        }
    }

    private void HandleScoreChanged(int _)
    {
        RefreshUI();
        CoinsChanged?.Invoke(Coins);
    }

    private void RefreshUI()
    {
        if (coinText != null)
        {
            coinText.text = $"Coins: {Coins}";
        }
    }
}
