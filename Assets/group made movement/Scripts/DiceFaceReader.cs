using UnityEngine;

public class DiceFaceReader : MonoBehaviour
{
    [System.Serializable]
    public struct DiceFace
    {
        public int value;
        public Transform faceTransform;
    }

    public DiceFace[] faces;
    public int FinalValue { get; private set; }
    public bool HasStopped { get; private set; }

    private Rigidbody rb;
    private float stillTime = 0f;
    private const float settleDuration = 0.1f;   // must remain still for this long
    private const float velocityThreshold = 0.09f; // small tolerance for resting dice

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (HasStopped) return;

        // If dice movement is slow enough
        if (rb.linearVelocity.magnitude < velocityThreshold && rb.angularVelocity.magnitude < velocityThreshold)
        {
            stillTime += Time.deltaTime;

            // Consider dice stopped after being still for a bit
            if (stillTime >= settleDuration)
            {
                HasStopped = true;
                DetermineTopFace();
            }
        }
        else
        {
            stillTime = 0f; // Reset timer if dice move again
        }
    }

    private void DetermineTopFace()
    {
        float highestDot = -1f;
        int topValue = 0;

        foreach (var face in faces)
        {
            float dot = Vector3.Dot(face.faceTransform.up, Vector3.up);
            if (dot > highestDot)
            {
                highestDot = dot;
                topValue = face.value;
            }
        }

        FinalValue = topValue;
        Debug.Log($"{gameObject.name} rolled a {FinalValue}");
    }
}
