using UnityEngine;
using System;
using TMPro;

public class ScoreManager : Singleton<ScoreManager>
{

    [Header("Scoring Settings")]
    [SerializeField] private int baseScore = 100;
    [SerializeField] private int penaltyBurnt = -50;
    //[SerializeField] private int penaltyWrongToppings = -25;

    [SerializeField] private TextMeshProUGUI scoreTMP;

    private int currentScore = 0;
    public event Action<int> OnScoreChanged;

    private void OnEnable()
    {
        ServeZone.OnPizzaServed += HandlePizzaServed;
    }
    private void OnDisable()
    {
        ServeZone.OnPizzaServed -= HandlePizzaServed;
    }
    // Calculate score based on success and pizza state
    // For now, just add the score or penalty to the current score
    // TODO: Add more complex scoring logic later (Timing, bonuses, etc.)\

    void Start()
    {
        ServeZone.OnPizzaServed += HandlePizzaServed;
        scoreTMP.text = $"Score: {currentScore}";
    }
    private void HandlePizzaServed(PizzaController pizza, bool success)
    {
        if (success)
        {
            AddScore(baseScore);
            return;
        }
        if (pizza.isBurnt)
        {
            AddScore(penaltyBurnt);
        }
        //else
        //{
        //   AddScore(penaltyWrongToppings);
        //}
        //return;
    }

    // For UI updates
    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"Current Score: {currentScore}");
        scoreTMP.text = $"Score: {currentScore}";
        OnScoreChanged?.Invoke(currentScore);
    }
    public int GetScore()
    {
        return currentScore;
    }

    // For round resets
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
}
