using System;
using TMPro;
using UnityEngine;

public class KneadZone : MonoBehaviour
{
    [Header("Target Dough Controller")]
    public DoughController doughController;

    public TextMeshPro tmp;


    private void OnTriggerEnter(Collider other)
    {
        tmp.text = $"{other.name} touched knead zone of dough";
        Debug.Log($"{other.name} touched knead zone of dough");
        if (other.gameObject.CompareTag("hand"))
        {
            doughController.SetHandInKneadZone(true);
            Debug.Log("[KneadZone] Hand entered knead zone");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("hand"))
        {
            doughController.SetHandInKneadZone(false);
            Debug.Log("[KneadZone] Hand exited knead zone");
        }
    }
}
