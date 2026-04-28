using System;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text positionText;

    [Header("References")]
    [SerializeField] private Transform player;

    private int score;

    public int Score => score;
    public event Action<int> ScoreChanged;

    private void Start()
    {
        RefreshScore();
    }

    private void Update()
    {
        if (player == null || positionText == null)
        {
            return;
        }

        Vector3 p = player.position;
        positionText.text = $"Pos X:{p.x:F1} Y:{p.y:F1} Z:{p.z:F1}";
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        score += amount;
        RefreshScore();
        ScoreChanged?.Invoke(score);
    }

    private void RefreshScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Coins: {score}";
        }
    }
}
