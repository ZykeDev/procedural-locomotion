using UnityEngine;

public class ConstraintController : MonoBehaviour
{
    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.position;
    }

    void Update()
    {
        // Keep the limb tip anchored to its original position
        transform.position = originalPos;
    }
}
