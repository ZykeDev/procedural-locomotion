using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(TwoBoneIKConstraint))]
public class ConstraintController : MonoBehaviour
{
    [SerializeField, Tooltip("Reference to the target transform the joint should move towards.")] 
    private Transform target;

    [SerializeField, Tooltip("Reference to the joint opposite to this one with respect to the walking axis.")]
    private ConstraintController opposite;

    private Vector3 originalPos;
    private Transform tip;
    private float maxRange;                 // Maximum range of the limb chain
    private int layerMask;

    private TwoBoneIKConstraint TwoBoneIKConstraint => GetComponent<TwoBoneIKConstraint>();
    private Entity ParentEntity => GetComponentInParent<Entity>();

    [SerializeField, Min(0.1f), Tooltip("Distance after which to take a step.")]
    private float distanceThreshold = 1.5f;

    private float proximityThreshold = 0.01f;

    [SerializeField, Range(0.1f, 10f), Tooltip("Movement Speed.")]
    private float speed = 2f;

    [SerializeField, Min(0.1f), Tooltip("Maximum height reached by the limb during its limb's arching animation.")]
    private float limbMovementHeight = 0.5f;

    public bool IsMoving { get; private set; }
    public Transform TipTransform { get; private set; }


    void Awake()
    {
        tip = TwoBoneIKConstraint.data.tip;
        TipTransform = tip.transform;
        originalPos = transform.position;
        IsMoving = false;
        layerMask = LayerMask.GetMask("Ground");

        maxRange = GetChainLength();

        // Pass the tip down to the Anchor for initial positioning
        target.gameObject.GetComponent<GroundAnchor>().SetData(tip, maxRange);
    }

    void Start()
    {
        AnchorOriginalPosition();
    }


    void Update()
    {
        Move();
    }

    /// <summary>
    /// Move the limb towards the target Ground Anchor
    /// </summary>
    private void Move()
    {
        if (!IsMoving)
        {
            // Check if the distance to the target point is too great
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // Check if the opposite limb is already moving
            bool isOppositeMoving = opposite != null && opposite.IsMoving;

            // TODO also check if the distance is too great (edges)
            if (distanceToTarget > distanceThreshold && !isOppositeMoving)
            {
                int axisIndex = 1; // Make a parabola along the Y axis only
                // Start moving the limb
                Coro.Perp(transform, target.position, axisIndex, 1 / speed, OnMovementEnd);
                IsMoving = true;
            }
            else
            {
                // Keep anchored
                transform.position = originalPos;
            }
        }
    }

    private void OnMovementEnd()
    {
        IsMoving = false;
        transform.position = target.position;
        originalPos = transform.position;
    }


    /// <summary>
    /// Anchors the starting tip position to the ground below
    /// </summary>
    private void AnchorOriginalPosition()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.yellow);
            originalPos = hit.transform.position;
        }
    }


    /// <summary>
    /// Moves the target forward to simulate a quadrupedal locomotion pattern
    /// </summary>
    /// <param name="difference"></param>
    public void ForwardTarget(float difference)
    {
        // TODO use differnece
        difference = distanceThreshold / 4;
        target.parent.position = new Vector3(target.parent.position.x, target.parent.position.y, target.parent.position.z + difference);
    }


    // TODO generalize by changing bones with joints
    /// <summary>
    /// Returns the length of the entire limb by adding up the distance between bones
    /// </summary>
    /// <returns></returns>
    public float GetChainLength()
    {
        Vector3 root = TwoBoneIKConstraint.data.root.transform.position;
        Vector3 mid = TwoBoneIKConstraint.data.mid.transform.position;
        Vector3 tip = TwoBoneIKConstraint.data.tip.transform.position;

        float tipToMid = Vector3.Distance(tip, mid);
        float midToRoot = Vector3.Distance(mid, root);

        return tipToMid + midToRoot;
    }




    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);

        Vector3 root = TwoBoneIKConstraint.data.root.transform.position;
        Vector3 mid = TwoBoneIKConstraint.data.mid.transform.position;
        Vector3 tip = TwoBoneIKConstraint.data.tip.transform.position;

        //Gizmos.DrawLine(tip, mid);
        //Gizmos.DrawLine(mid, root);
    }
}
