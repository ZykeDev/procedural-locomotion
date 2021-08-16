using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(TwoBoneIKConstraint))]
public class ConstraintController : MonoBehaviour
{
    [Tooltip("Reference to the target transform the joint should move towards.")] 
    public Transform target;

    [Tooltip("Reference to the joint opposite to this one with respect to the walking axis.")]
    public Transform opposite;

    private Vector3 originalPos;
    private Transform tip;
    private float maxRange;                 // Maximum range of the limb chain
    private int layerMask;
    

    public TwoBoneIKConstraint TwoBoneIKConstraint => GetComponent<TwoBoneIKConstraint>();
    private Entity ParentEntity => GetComponentInParent<Entity>();

    [SerializeField, Min(0.1f), Tooltip("Distance after which to take a step.")]
    private float minRange = 1.5f;

    private float proximityThreshold = 0.01f;

    [SerializeField, Range(0.1f, 10f), Tooltip("Movement Speed.")]
    private float speed = 2f;
    private float weight;

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
        weight = GetChainWeight(); 

        // Pass the tip down to the Anchor for initial positioning
        target.gameObject.GetComponent<GroundAnchor>().SetData(tip, maxRange);
    }

    void Start()
    {
        AnchorOriginalPosition();
    }

    void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// Moves the limb towards the target Ground Anchor
    /// </summary>
    private void Move()
    {
        Vector3 jointPos = TwoBoneIKConstraint.data.root.transform.position;
        float distanceFromBody = Vector3.Distance(jointPos, target.position);

        if (distanceFromBody > maxRange) Debug.DrawLine(jointPos, target.position, Color.red);
        else Debug.DrawLine(jointPos, target.position, Color.green);

        if (!IsMoving)
        {
            // Check if the distance to the target point is too great
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            // If the next step would be too far, don't allow the entity to move in that direction.
            if (distanceFromBody > maxRange)
            {
                ParentEntity.LimitMovement(target.position);
            } 
            else
            {
                ParentEntity.MovementController.ResetArcLimit();
            }

            ConstraintController oppositeCC = opposite.gameObject.GetComponent<ConstraintController>();

            bool isOppositeMoving = oppositeCC != null && oppositeCC.IsMoving;      // Check if the opposite limb is already moving           
            bool isWithinRange = distanceToTarget > minRange;
            bool isTargetWithinRange = distanceFromBody < maxRange;
            bool isTraversable = IsTraversable(target.position);                    // Check if the destination is traversable


            if (isWithinRange && !isOppositeMoving && isTraversable)
            {
                // Start a coroutine to move the limb
                Coro.Perp(transform, target.position, (int)ParentEntity.limbUpwardsAxis, weight / speed, OnMovementEnd);
                IsMoving = true;
            }
            else
            {
                Anchor();
            }
        }
    }

    private void OnMovementEnd()
    {
        IsMoving = false;
        transform.position = target.position;
        originalPos = transform.position;
    }


    private void Anchor()
    {
        if (transform.position != originalPos)
        {
            transform.position = originalPos;
        }
    }

    /// <summary>
    /// Anchors the starting tip position to the ground below
    /// </summary>
    private void AnchorOriginalPosition()
    {
        Vector3 direction = transform.TransformDirection(Vector3.down);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            //Debug.DrawRay(transform.position, direction * hit.distance, Color.cyan);
            originalPos = hit.point;
        }
    }


    /// <summary>
    /// Moves the target forward to simulate a quadrupedal locomotion pattern
    /// </summary>
    /// <param name="difference"></param>
    public void ForwardTarget(float difference)
    {
        // TODO use differnece
        difference = minRange / 4;
        target.parent.position = new Vector3(target.parent.position.x, target.parent.position.y, target.parent.position.z + difference);
    }


    // TODO generalize by changing bones with joints?
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


    /// <summary>
    /// Returns the average weight of all components that form the associated limb
    /// </summary>
    /// <returns></returns>
    private float GetChainWeight()
    {
        Weight root = TwoBoneIKConstraint.data.root.GetComponent<Weight>();
        Weight mid = TwoBoneIKConstraint.data.mid.GetComponent<Weight>();
        Weight tip = TwoBoneIKConstraint.data.tip.GetComponent<Weight>();

        float rootW = root ? root.weight : 1;
        float midW = mid ? mid.weight : 1;
        float tipW = tip ? tip.weight : 1;

        return (rootW + midW + tipW) / 3;
    }


    /// <summary>
    /// Returns true if the point is not inside an Untraversable terrain collider
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool IsTraversable(Vector3 point)
    {
        Vector3 com = ParentEntity.CenterOfMass;
        Vector3 rayDirection = (point - com).Rescale(0.98f);        // Only use 98% of the ray, since we don't want to hit the ground
        RaycastHit[] hits = Physics.RaycastAll(com, rayDirection, rayDirection.magnitude);

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                Debug.DrawLine(hits[i].point, hits[i].point + Vector3.up);
                if (hits[i].transform.gameObject.CompareTag(Settings.Tag_untraversable))
                {
                    return false;
                }
            }
        }

        return true;
    }

}
