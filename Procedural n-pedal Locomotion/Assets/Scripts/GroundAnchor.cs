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
    private Vector3 temp;

    // TODO Parametrize into upwards: geometrical / gravitational
    private bool useGeometricalUpwards = true;
    private float maxRange;

    void Awake()
    {
        layerMask = LayerMask.GetMask("Ground");
        ParentEntity = GetComponentInParent<Entity>();
    }

    void Start()
    {
        temp = transform.position;

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

        if (!ParentEntity.IsRotating)
        {
            Vector3 direction = useGeometricalUpwards ? Vector3.down : transform.up;
            direction = transform.TransformDirection(Vector3.down);
            direction = -transform.up;

            RaycastHit hit;

            if (Physics.Raycast(transform.position + verticalOffset, direction, out hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawRay(transform.position + verticalOffset, direction * hit.distance, Color.yellow);
                
                if (transform.position != hit.point)
                {
                    UpdatePosition(hit.point);
                }
            }
        }
    }

    private void UpdatePosition(Vector3 newPos)
    {
        float distance = Vector3.Distance(transform.position, newPos);

        if (distance > maxRange)
        {
            // Find the applied vector from the current position towards the target
            Vector3 posToTarget = newPos - transform.position;

            // Clamp the target pos to never exceed the limbs range
            //(((b-a)/|b-a|)*r)+a
            newPos = (posToTarget.normalized * maxRange) + transform.position;
        }

        // Update the values
        transform.position = newPos;
        temp = newPos;
    }


    public void SetData(Transform tip, float maxRange)
    {
        this.tip = tip;
        this.maxRange = maxRange;
    }


    /// <summary>
    /// Rescales a given vector to have a magnitude equal to "scale".
    /// </summary>
    /// <param name="v"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Vector3 Rescale(Vector3 v, float scale)
    {
        Vector3 scaledVector = v;

        scaledVector *= (1 - scale / v.magnitude);

        return scaledVector;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, maxRange/4);
    }
}
