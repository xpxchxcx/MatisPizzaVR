using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PizzaLabelUI : MonoBehaviour
{
    public TextMeshProUGUI orderNumberText;
    public TextMeshProUGUI pizzaTypeText;
    public Slider assemblyProgressSlider;

    private Transform targetPizza;
    private Transform xrCamera;

    private PizzaController parentPizzaController;

    public void Destruct()
    {
        Destroy(this);
    }

    void OnDisable()
    {
        parentPizzaController.PizzaDead -= Destruct;
    }


    public void Initialize(GameObject go, Transform pizzaTransform, string orderNumber, string pizzaType, float maxProgress, Transform xrCam)
    {
        parentPizzaController = go.GetComponent<PizzaController>();
        parentPizzaController.PizzaDead += Destruct;
        targetPizza = pizzaTransform;
        xrCamera = xrCam;
        orderNumberText.text = $"Order: {orderNumber}";
        pizzaTypeText.text = pizzaType;
        if (assemblyProgressSlider != null)
        {
            assemblyProgressSlider.maxValue = maxProgress;
            assemblyProgressSlider.value = 0;
        }
    }
    public void SetUIElements(TextMeshProUGUI orderText, TextMeshProUGUI typeText, Slider slider)
    {
        orderNumberText = orderText;
        pizzaTypeText = typeText;
        assemblyProgressSlider = slider;
    }
    public void SetTarget(Transform newTarget)
    {
        targetPizza = newTarget;
    }

    public void UpdateProgress(float progress)
    {
        if (assemblyProgressSlider != null)
            assemblyProgressSlider.value = progress;
        if (progress == 6) Destroy(this);
    }

    void LateUpdate()
    {
        if (targetPizza == null || xrCamera == null) return;

        transform.position = targetPizza.position + Vector3.up * 0.5f;

        Vector3 lookDir = transform.position - xrCamera.position; // note the swap
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
