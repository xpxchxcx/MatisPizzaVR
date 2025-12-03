using UnityEngine;

public class ScorePopupManager : MonoBehaviour
{
    public FloatingScorePopup popupPrefab;



    private void OnDisable()
    {
        ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
    }

    void Start()
    {
        ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
    }


    public void OnScoreChanged(int amountAdded)
    {
        // Only spawn popup if positive
        if (amountAdded <= 0) return;

        // Spawn at spawner's position
        FloatingScorePopup popup = Instantiate(popupPrefab, transform.position, Quaternion.identity, transform);
        popup.Setup(amountAdded);
    }
}
