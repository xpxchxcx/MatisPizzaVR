using UnityEngine;
using System.Collections.Generic;

public class ToppingHandler : MonoBehaviour
{
    [SerializeField] private PizzaController pizzaController;

    [Header("Recipe Selection")]
    public enum PizzaType { Margherita, Pepperoni }
    [SerializeField] private PizzaType selectedPizza = PizzaType.Margherita;

    // Recipe requirements
    private Dictionary<PizzaType, Dictionary<string, int>> recipes = new Dictionary<PizzaType, Dictionary<string, int>>();

    // Current toppings on pizza
    private Dictionary<string, int> currentToppings = new Dictionary<string, int>();

    void Start()
    {
        // Define recipes
        recipes[PizzaType.Margherita] = new Dictionary<string, int>
        {
            { "cheese", 5 }
        };

        recipes[PizzaType.Pepperoni] = new Dictionary<string, int>
        {
            { "cheese", 5 },
            { "pepperoni", 5 }
        };

        Debug.Log($"Selected pizza: {selectedPizza}");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is a topping (using tags)
        if (other.CompareTag("cheese") || other.CompareTag("pepperoni"))
        {
            string toppingType = other.tag;
            
            // Add topping to current count
            if (!currentToppings.ContainsKey(toppingType))
            {
                currentToppings[toppingType] = 0;
            }
            currentToppings[toppingType]++;

            Debug.Log($"Topping added: {toppingType} (Total: {currentToppings[toppingType]})");

            // Check if recipe is complete
            CheckRecipeCompletion();
        }
    }

    private void CheckRecipeCompletion()
    {
        Dictionary<string, int> requiredToppings = recipes[selectedPizza];
        
        // Check if all required toppings are met
        foreach (var requirement in requiredToppings)
        {
            string toppingType = requirement.Key;
            int requiredAmount = requirement.Value;

            if (!currentToppings.ContainsKey(toppingType) || currentToppings[toppingType] < requiredAmount)
            {
                // Recipe not complete yet
                return;
            }
        }

        // All requirements met!
        OnRecipeComplete();
    }

    private void OnRecipeComplete()
    {
        Debug.Log($"<color=green>Recipe complete for {selectedPizza} pizza!</color>");
        
        if (pizzaController != null)
        {
            pizzaController.CompleteToppingStage();
        }
    }
}
