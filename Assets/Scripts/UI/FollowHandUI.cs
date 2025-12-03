using UnityEngine;

public class FollowHandUI : MonoBehaviour
{
    [Header("Hand Reference")]
    public Transform handTransform; // Assign your XR hand here

    [Header("Offset from hand")]
    public Vector3 localOffset = new Vector3(0f, 0.1f, 0.2f); // adjust as needed
    public Vector3 localEulerRotation = new Vector3(0f, 0f, 0f);

    [Header("Smooth Movement")]
    public float followSpeed = 10f;

    private void LateUpdate()
    {
        if (handTransform == null) return;

        // Target position relative to hand
        Vector3 targetPos = handTransform.position + handTransform.TransformVector(localOffset);

        // Smoothly move panel to target position
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        // Make the panel face the main camera
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(localEulerRotation); // apply any rotation offset
        }
    }
}
