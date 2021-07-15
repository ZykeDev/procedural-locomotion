using UnityEngine;

public class GroundAnchor : MonoBehaviour
{
    private Entity ParentEntity;
    private Transform tip;                  // Tip of the limb. Should be passed down by the Constraint Controller
    private int layerMask;
    private bool forceInitPos = true;       // Forces the target position to start exactly where the tip is

    private Vector3 verticalOffset = new Vector3(0, 1f, 0);
    private Vector3 verticalGap = new Vector3(0, 0.05f, 0);

    // TODO Parametrize into upwards: global v3.down / local -t.up
    private bool useGeometricalUpwards = true;
    private float maxRange;

    void Awake()
    {
        layerMask = LayerMask.GetMask("Ground");
        ParentEntity = GetComponentInParent<Entity>();
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
            transform.position = tip.position;
            forceInitPos = false;
        }

        if (!ParentEntity.IsRotating)
        {
            // TODO remove overrides
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
        /*  Deprecated (would need to use the distance from Root to newPos)
        
        float distance = Vector3.Distance(transform.position, newPos);

        // Clamp the distance at maxRange
        if (distance > maxRange)
        {
            // Find the applied vector from the current position towards the target
            Vector3 posToTarget = newPos - transform.position;

            // (((b-a) / |b-a|) * r) + a
            newPos = (posToTarget.normalized * maxRange) + transform.position;
        }
        */


        // Update the values
        transform.position = newPos;
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

    }
}
