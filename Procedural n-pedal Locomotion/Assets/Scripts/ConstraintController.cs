using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(TwoBoneIKConstraint))]
public class ConstraintController : MonoBehaviour
{
    [Tooltip("Reference to the target transform the joint should move towards.")] 
    public Transform target;

    [Tooltip("Reference to the joint opposite to this one with respect to the walking axis.")]
    public Transform opposite;

    [Tooltip("Reference to the joint ahead along the walking axis.")]
    public Transform ahead;

    [Tooltip("Reference to the joint behind along the walking axis.")]
    public Transform behind;

    private ConstraintController oppositeCC, aheadCC, behindCC;

    private Vector3 originalPos;
    private Transform tip;
    private float maxRange;                 // Maximum range of the limb chain
    private int layerMask;

    public TwoBoneIKConstraint TwoBoneIKConstraint => GetComponent<TwoBoneIKConstraint>();
    private Entity ParentEntity => GetComponentInParent<Entity>();

    [SerializeField, Min(0.1f), Tooltip("Distance after which to take a step. If this value is set to anything other than 0, it overrides the Step Size defined in the Entity Component.")]
    private float stepSize;

    [SerializeField, Range(0.1f, 50f), Tooltip("Movement Speed.")]
    private float speed = 4f;
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
    }

    void Start()
    {
        if (opposite != null) oppositeCC = opposite?.gameObject.GetComponent<ConstraintController>();
        if (ahead != null) aheadCC = ahead.gameObject.GetComponent<ConstraintController>();
        if (behind != null) behindCC = behind.gameObject.GetComponent<ConstraintController>();

        target.GetComponent<GroundAnchor>().SetTip(tip);
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

        if (distanceFromBody > maxRange) Debug.DrawLine(transform.position, jointPos, Color.red);
        else Debug.DrawLine(transform.position, jointPos, Color.green);

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

            // Check if the opposite, ahead, or behind limbs are already moving
            bool isOppositeMoving = oppositeCC != null && oppositeCC.IsMoving;      
            bool isAheadMoving = aheadCC != null && aheadCC.IsMoving;
            bool isBehindMoving = behindCC != null && behindCC.IsMoving;

            bool isStable = !isOppositeMoving && !isAheadMoving && !isBehindMoving;

            // Check if the step respects the limb and terrain constriants
            bool canMove = distanceToTarget > stepSize || (distanceFromBody > maxRange && distanceToTarget > stepSize);
            bool isTraversable = IsTraversable(target.position);

            if (canMove && isTraversable && isStable)
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
    /// Moves the target forward by a random amount to simulate a quadrupedal locomotion patterns
    /// </summary>
    public void DisplaceTarget(int index, int disparity)
    {
        int numberOfLimbs = 8;  // TODO automatically detect this
        float forwardDistance = stepSize / (numberOfLimbs * 2) + (stepSize / (numberOfLimbs * 4) * index);

        int sign = Convert.ToInt32(disparity % 2 != 0) * 2 - 1;     // Converts the disparity into a number sign (-1 or +1)

        Vector3 rootPos = TwoBoneIKConstraint.data.root.transform.position;
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, target.position.z + (forwardDistance * sign));
        float distFromRoot = Vector3.Distance(targetPos, rootPos);

        // Make sure the initial target distance is shorter than the max range of the limb.
        while (distFromRoot > maxRange)
        {
            targetPos.z += 0.002f * sign;
        }
                       
        // TODO use forward rather than always Z
        target.position = targetPos;
    }

    /// <summary>
    /// Sets the Step Size. The new value is ignored if it has been overwritten in this CC component.
    /// </summary>
    /// <param name="size"></param>
    public void SetStepSize(float size)
    {
        if (stepSize == default || stepSize <= 0)
        {
            stepSize = size;
        }
    }

    /// <summary>
    /// Sets the Max stepping Range.
    /// </summary>
    /// <param name="range"></param>
    public void SetMaxRange(float range)
    {
        if (range != default && range > 0)
        {
            maxRange = range;
        }
    }


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
