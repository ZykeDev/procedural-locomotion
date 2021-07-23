using UnityEngine;

public class GroundAnchor : MonoBehaviour
{
    private Entity ParentEntity;
    private Transform tip;                  // Tip of the limb. Should be passed down by the Constraint Controller
    private int layerMask;
    private bool forceInitPos = true;       // Forces the target position to start exactly where the tip is

    private Vector3 verticalOffset = new Vector3(0, 1f, 0);
    private Vector3 verticalGap = new Vector3(0, 0.001f, 0);    // Short vertical vector

    private Vector3 prevPos;

    // TODO Parametrize into upwards: global v3.down / local -t.up
    private bool useGeometricalUpwards = true;
    private float maxRange;

    void Awake()
    {
        layerMask = LayerMask.GetMask("Ground");
        ParentEntity = GetComponentInParent<Entity>();
    }

    void FixedUpdate()
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

            Vector3 target = prevPos;
            Vector3 cvo = transform.position + verticalOffset;  // Previous vertical offset position
            Vector3 pvo = prevPos + verticalOffset;             // Current vertical offset position


            bool isGroundHit = Physics.Raycast(cvo, direction, out RaycastHit groundHit, Mathf.Infinity, layerMask);

            if (isGroundHit)
            {
                Debug.DrawRay(cvo, direction * groundHit.distance, Color.yellow);
                target = groundHit.point;
            }

            if (transform.position != target)
            {
                prevPos = transform.position;
                transform.position = target;
            }            
        }
    }


    public void SetData(Transform tip, float maxRange)
    {
        this.tip = tip;
        this.maxRange = maxRange;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + verticalGap, 0.02f);
    }

}
