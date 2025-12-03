using UnityEngine;
using TMPro;

public class FloatingScorePopup : MonoBehaviour
{
    public float lifetime = 1f;
    public float floatSpeed = 1f;
    public float fadeSpeed = 2f;

    private TextMeshProUGUI text;
    private Color startColor;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        startColor = text.color;
    }

    public void Setup(int amount)
    {
        text.text = "+" + amount;
    }

    void Update()
    {
        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        Color c = text.color;
        c.a -= fadeSpeed * Time.deltaTime;
        text.color = c;

        if (c.a <= 0f)
            Destroy(gameObject);
    }
}
