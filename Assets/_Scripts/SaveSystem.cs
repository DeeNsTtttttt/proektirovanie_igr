using TMPro;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private const string Prefix = "Lab8_";

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PauseMenu pauseMenu;

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField, Min(0f)] private float statusDuration = 2f;
    [SerializeField] private bool showFallbackStatus = true;

    private float statusTimer;
    private string lastStatusMessage;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        if (statusText == null || statusTimer <= 0f)
        {
            return;
        }

        statusTimer -= Time.unscaledDeltaTime;
        if (statusTimer <= 0f)
        {
            statusText.text = string.Empty;
        }
    }

    private void OnGUI()
    {
        if (!showFallbackStatus || statusText != null || statusTimer <= 0f || string.IsNullOrWhiteSpace(lastStatusMessage))
        {
            return;
        }

        const float width = 260f;
        const float height = 28f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height - 150f;
        GUI.Label(new Rect(x, y, width, height), lastStatusMessage);
    }

    public void SaveGame()    {
        ResolveReferences();

        if (player == null)
        {
            SetStatus("Save failed: Player not found");
            return;
        }

        Vector3 p = player.position;
        PlayerPrefs.SetFloat(Prefix + "PlayerX", p.x);
        PlayerPrefs.SetFloat(Prefix + "PlayerY", p.y);
        PlayerPrefs.SetFloat(Prefix + "PlayerZ", p.z);

        if (playerHealth != null)
        {
            PlayerPrefs.SetInt(Prefix + "CurrentHealth", playerHealth.CurrentHealth);
            PlayerPrefs.SetInt(Prefix + "MaxHealth", playerHealth.MaxHealth);
        }

        if (resourceManager != null)
        {
            PlayerPrefs.SetInt(Prefix + "Coins", resourceManager.Coins);
        }

        if (playerStats != null)
        {
            PlayerPrefs.SetInt(Prefix + "DamageBonus", playerStats.DamageBonus);
            PlayerPrefs.SetFloat(Prefix + "SpeedBonus", playerStats.SpeedBonus);
            PlayerPrefs.SetInt(Prefix + "MaxHealthBonus", playerStats.MaxHealthBonus);
        }

        PlayerPrefs.SetInt(Prefix + "HasSave", 1);
        PlayerPrefs.Save();
        SetStatus("Game saved");
    }

    public void LoadGame()
    {
        ResolveReferences();

        if (!HasSave())
        {
            SetStatus("No save found");
            return;
        }

        if (player != null)
        {
            Vector3 p = new Vector3(
                PlayerPrefs.GetFloat(Prefix + "PlayerX", player.position.x),
                PlayerPrefs.GetFloat(Prefix + "PlayerY", player.position.y),
                PlayerPrefs.GetFloat(Prefix + "PlayerZ", player.position.z));

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            player.position = p;
        }

        if (playerStats != null)
        {
            playerStats.RestoreBonuses(
                PlayerPrefs.GetInt(Prefix + "DamageBonus", 0),
                PlayerPrefs.GetFloat(Prefix + "SpeedBonus", 0f),
                PlayerPrefs.GetInt(Prefix + "MaxHealthBonus", 0));
        }

        if (playerHealth != null)
        {
            int maxHealth = PlayerPrefs.GetInt(Prefix + "MaxHealth", playerHealth.MaxHealth);
            int currentHealth = PlayerPrefs.GetInt(Prefix + "CurrentHealth", maxHealth);
            playerHealth.RestoreHealthState(currentHealth, maxHealth);
        }

        if (resourceManager != null)
        {
            resourceManager.SetCoins(PlayerPrefs.GetInt(Prefix + "Coins", resourceManager.Coins));
        }

        if (pauseMenu != null)
        {
            pauseMenu.Resume();
        }
        else
        {
            Time.timeScale = 1f;
        }

        SetStatus("Game loaded");
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(Prefix + "HasSave");
        PlayerPrefs.DeleteKey(Prefix + "PlayerX");
        PlayerPrefs.DeleteKey(Prefix + "PlayerY");
        PlayerPrefs.DeleteKey(Prefix + "PlayerZ");
        PlayerPrefs.DeleteKey(Prefix + "CurrentHealth");
        PlayerPrefs.DeleteKey(Prefix + "MaxHealth");
        PlayerPrefs.DeleteKey(Prefix + "Coins");
        PlayerPrefs.DeleteKey(Prefix + "DamageBonus");
        PlayerPrefs.DeleteKey(Prefix + "SpeedBonus");
        PlayerPrefs.DeleteKey(Prefix + "MaxHealthBonus");
        PlayerPrefs.Save();
        SetStatus("Save deleted");
    }

    public bool HasSave()
    {
        return PlayerPrefs.GetInt(Prefix + "HasSave", 0) == 1;
    }

    private void ResolveReferences()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerStats == null && player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        if (resourceManager == null)
        {
            resourceManager = ResourceManager.Instance != null
                ? ResourceManager.Instance
                : FindFirstObjectByType<ResourceManager>();
        }

        if (pauseMenu == null)
        {
            pauseMenu = FindFirstObjectByType<PauseMenu>();
        }
    }

    private void SetStatus(string message)
    {
        lastStatusMessage = message;
        statusTimer = statusDuration;

        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message, this);
    }
}