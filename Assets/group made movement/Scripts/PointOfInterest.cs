using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    [Header("Tracking Settings")]
    public Transform target;         // target
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);
    public float smoothSpeed = 10f;
    public bool faceTarget = false; // incase of events?

    void LateUpdate()
    {
        if (target == null) return;

        // move towards object of interest
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Optionally rotate towards target
        if (faceTarget)
        {
            Vector3 lookDirection = target.position - transform.position;
            if (lookDirection.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDirection),
                    smoothSpeed * Time.deltaTime
                );
        }
        else
        {
            // Keep upright if not rotating toward target
            transform.rotation = Quaternion.identity;
        }
    }

    // Allows other scripts to set what to follow
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Optional: stop following anything
    public void ClearTarget()
    {
        target = null;
    }
}
