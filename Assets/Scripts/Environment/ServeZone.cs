using UnityEngine;

public class ServeZone : MonoBehaviour
{
    [Header("Scoring Settings")]
    [SerializeField] private int baseScore = 100;
    [SerializeField] private int penaltyBurnt = -50;
    [SerializeField] private int penaltyWrongToppings = -25;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("pizza")) return;

        PizzaController pizza = other.GetComponent<PizzaController>();
        if (pizza == null)
        {
            Debug.LogWarning("[ServeZone] Object entered but no PizzaController found!");
            return;
        }

        // Mark as served
        pizza.OnServed();

        // Validate against the active order
        bool success = OrderManager.Instance.ValidatePizzaAndCompleteOrder(pizza);

        // Optional: handle scoring
        HandleScoring(pizza, success);

        // Cleanup
        AssemblyManager.Instance.CompletePizza(pizza);
    }

    private void HandleScoring(PizzaController pizza, bool success)
    {
        int score = baseScore;

        if (!success)
        {
            if (pizza.isBurnt)
                score += penaltyBurnt;
            else
                score += penaltyWrongToppings;
        }

        Debug.Log($"[ServeZone] Pizza '{pizza.pizzaName}' served. " +
                  $"Cooked: {pizza.isCooked}, Burnt: {pizza.isBurnt}, Valid: {success}. " +
                  $"Score: {score}");

        // TODO: send score to ScoreManager if you have one:
        // ScoreManager.Instance.AddScore(score);
    }
}
