using UnityEngine;

public class GroundAnchor : MonoBehaviour
{
    private Transform origin;
    private int layerMask;

    private Vector3 verticalOffset = new Vector3(0, 0.75f, 0);
    private Vector3 verticalGap = new Vector3(0, 0.2f, 0);


    void Awake()
    {
        layerMask = LayerMask.GetMask("Ground");
    }

    void Start()
    {
        origin = transform.parent;
        if (origin == null)
        {
            Debug.LogError("No origin parent");
        }
    }

    void Update()
    {
        Anchor();
    }

    /// <summary>
    /// Keeps the object anchored to the ground
    /// </summary>
    private void Anchor()
    {
        RaycastHit hit;

        // First, check if the ground is between the object and the target
        if (Physics.Linecast(origin.position + verticalOffset, transform.position, out hit, layerMask))
        {
            transform.position = hit.point + verticalGap;
        }
        // If there is nothing in between, anchor the object to the ground
                                                                   // -transform.up
        else if (Physics.Raycast(transform.position + verticalOffset, -Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point + verticalGap;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin.position, transform.position);
    }
}
