using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Values For UI")]
    [SerializeField, Min(1)] private int baseDamage = 20;
    [SerializeField, Min(0f)] private float baseMoveSpeed = 6f;
    [SerializeField, Min(0f)] private float baseRunSpeed = 8f;

    [Header("Upgrade Bonuses")]
    [SerializeField, Min(0)] private int damageBonus = 0;
    [SerializeField, Min(0f)] private float speedBonus = 0f;
    [SerializeField, Min(0)] private int maxHealthBonus = 0;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    public int BaseDamage => baseDamage;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float BaseRunSpeed => baseRunSpeed;
    public int DamageBonus => damageBonus;
    public float SpeedBonus => speedBonus;
    public int MaxHealthBonus => maxHealthBonus;
    public int TotalDamage => baseDamage + damageBonus;
    public float TotalMoveSpeed => baseMoveSpeed + speedBonus;
    public float TotalRunSpeed => baseRunSpeed + speedBonus;

    public event Action StatsChanged;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    public void AddDamageBonus(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        damageBonus += amount;
        StatsChanged?.Invoke();
    }

    public void AddSpeedBonus(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        speedBonus += amount;
        StatsChanged?.Invoke();
    }

    public void AddMaxHealthBonus(int amount, bool healAddedHealth = true)
    {
        if (amount <= 0)
        {
            return;
        }

        maxHealthBonus += amount;

        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.IncreaseMaxHealth(amount, healAddedHealth);
        }

        StatsChanged?.Invoke();
    }
}
