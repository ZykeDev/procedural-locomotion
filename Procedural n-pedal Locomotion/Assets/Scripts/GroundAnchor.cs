using UnityEngine;

public class GroundAnchor : MonoBehaviour
{
    private Entity ParentEntity;
    private Transform tip;                  // Tip of the limb. Should be passed down by the Constraint Controller
    private Transform origin;
    private int layerMask;
    private bool forceInitPos = true;       // Forces the target position to start exactly where the tip is

    private Vector3 verticalOffset = new Vector3(0, 1f, 0);
    private Vector3 verticalGap = new Vector3(0, 0.05f, 0);
    Vector3 temp;
    // TODO Parametrize into upwards: geometrical / gravitational
    private bool useGeometricalUpwards = true;

    void Awake()
    {
        layerMask = LayerMask.GetMask("Ground");
        ParentEntity = GetComponentInParent<Entity>();
    }

    void Start()
    {
        temp = transform.position;
        /*
        origin = transform.parent;

        if (origin == null)
        {
#if UNITY_EDITOR
            Debug.LogError("No origin parent");
#endif
        }
        */
    }

    void Update()
    {
        Anchor();
    }

    /// <summary>
    /// Keeps the object anchored to the ground below
    /// </summary>
    private void Anchor()
    {
        if (tip && forceInitPos)
        {
            //origin.position = tip.position;
            transform.position = tip.position;
            forceInitPos = false;
        }


        Vector3 direction = useGeometricalUpwards ? Vector3.down : transform.up;

        RaycastHit hit;

        if (Physics.Raycast(transform.position + verticalOffset, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position + verticalOffset, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            if (transform.position != hit.point)
            {
                UpdatePosition(hit.point);
            }
        }
        /*
        // First, check if the ground is between the object and the target
        if (Physics.Linecast(origin.position + verticalOffset, transform.position, out hit, layerMask))
        {
            transform.position = hit.point + verticalGap;
        }*/

        // If there is nothing in between, anchor the object to the ground
        // -transform.up
/*
        if (Physics.Raycast(origin.position + verticalOffset, -Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            print(origin.position + " -> " + hit.point);
            origin.position = hit.point;
            //transform.position = hit.point; // + verticalGap;
        }*/
    }

    private void UpdatePosition(Vector3 pos)
    {
        transform.position = pos;
        temp = pos;
    }


    public void SetTip(Transform tip) => this.tip = tip;

}
