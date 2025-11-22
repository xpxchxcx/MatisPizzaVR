using UnityEngine;

public class ParentFollower : MonoBehaviour
{
    private Transform targetChild;  // The object to follow

    void LateUpdate()
    {
        if (targetChild == null) return;

        // Snap parent position exactly to target child
        transform.position = targetChild.position;
        transform.rotation = targetChild.rotation;
    }

    /// <summary>
    /// Sets a new child for the parent to follow.
    /// </summary>
    public void Follow(Transform newTarget)
    {
        targetChild = newTarget;
    }

    /// <summary>
    /// Stops following the current target.
    /// </summary>
    public void StopFollowing()
    {
        targetChild = null;
    }
}
