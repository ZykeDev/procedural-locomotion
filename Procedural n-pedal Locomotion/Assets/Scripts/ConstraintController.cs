/* 
 * This file is part of the Procedural-Locomotion repo on github.com/ZykeDev 
 * Marco Vincenzi - 2021
 */

using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// Controls the movement of a single limb through a TwoBone IK Constraint.

[RequireComponent(typeof(TwoBoneIKConstraint))]
public class ConstraintController : MonoBehaviour
{
    [HideInInspector]
    public int id;  // Index of this CC and its corresponding limb in the character's list

    [Tooltip("Reference to the target transform the joint should move towards.")] 
    public Transform target;

    [Tooltip("Reference to the joint opposite to this one with respect to the walking axis.")]
    public Transform opposite;

    [Tooltip("Reference to the joint ahead along the walking axis.")]
    public Transform ahead;

    [Tooltip("Reference to the joint behind along the walking axis.")]
    public Transform behind;

    private ConstraintController oppositeCC, aheadCC, behindCC;

    [HideInInspector] public Transform tip, mid, root;

    private float maxRange;                 // Maximum range of the limb chain
    private Vector3 originalPos;            // Initial position of the constraint

    public TwoBoneIKConstraint TwoBoneIKConstraint => GetComponent<TwoBoneIKConstraint>();
    private LocomotionSystem Character => GetComponentInParent<LocomotionSystem>();

    [Space]
    [SerializeField, Min(0.1f), Tooltip("Distance after which to take a step. If this value is set to anything other than 0, it overrides the Step Size defined in the Locomotion System Component.")]
    private float stepSize;

    [SerializeField, Min(0.1f), Tooltip("Maximum height reached by the limb during its limb's arching animation.")]
    private float stepHeight = 0.5f;

    [SerializeField, Range(0.1f, 50f), Tooltip("Movement Speed.")]
    private float speed = 4f;
    private float chainWeight;

    public bool IsMoving { get; private set; }

    void Awake()
    {
        root = TwoBoneIKConstraint.data.root;
        mid = TwoBoneIKConstraint.data.mid;
        tip = TwoBoneIKConstraint.data.tip;
        
        originalPos = transform.position;
        IsMoving = false;
        
        maxRange = GetChainLength();
        chainWeight = GetAvgChainWeight();  
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
        Vector3 jointPos = root.transform.position;
        float distanceFromBody = Vector3.Distance(jointPos, target.position);

        //if (distanceFromBody > maxRange) Debug.DrawLine(transform.position, jointPos, Color.red);
        //else Debug.DrawLine(transform.position, jointPos, Color.green);

        if (!IsMoving)
        {
            // Check if the step respects the terrain constriants
            bool isTraversable = IsTraversable(target.position);
            if (!isTraversable)
            {
                Character.LimitMovement(target.position, id);
            } 
            else
            {
                Character.MovementController?.ResetArcLimit(id);
            }

            // Check if the distance to the target point is too great
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            //Debug.DrawLine(transform.position + new Vector3(0, 0.001f, 0f), target.position + new Vector3(0, 0.001f, 0f));

            // Check if the opposite, ahead, or behind limbs are already moving
            bool isOppositeMoving = oppositeCC != null && oppositeCC.IsMoving;      
            bool isAheadMoving = aheadCC != null && aheadCC.IsMoving;
            bool isBehindMoving = behindCC != null && behindCC.IsMoving;

            // Check if the step is legal
            bool isStable = !isOppositeMoving && !isAheadMoving && !isBehindMoving;
            bool canMove = distanceToTarget > stepSize || (distanceFromBody > maxRange && distanceToTarget > stepSize);

            bool isSprintEnabled = Character.MovementController.enableSprint;

            // If all checks are successful, move the limb
            if (canMove && isTraversable && isStable)
            {
                float stepSpeed = speed;

                bool isSprinting = Input.GetKey(Settings.Sprint_Key);
                if (isSprintEnabled && isSprinting)
                {
                    stepSpeed *= Character.MovementController.sprintMultiplier;
                    stepSpeed = Mathf.Pow(stepSpeed, 1 / 1.4f);
                }

                // Start a coroutine to move the limb
                Coro.Perp(transform, target.position, (int)Character.limbUpwardsAxis, stepHeight, chainWeight / stepSpeed, OnMovementEnd);
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
    public void DisplaceTarget(int index, int disparity, int numberOfLimbs, Vector3 forwardDirection)
    {
        // Find a forward distance relative to the step size and the number of limbs
        float forwardDistance = stepSize / (numberOfLimbs * 2) + (stepSize / (numberOfLimbs * 4) * index);

        // Converts the disparity into a number sign (-1 or +1)
        int sign = Convert.ToInt32(disparity % 2 != 0) * 2 - 1;
        
        // Shift the target position by direction and distance
        Vector3 targetPos = target.position + (forwardDirection * forwardDistance * sign);
        
        // Make sure the initial target distance is shorter than the max range of the limb
        Vector3 rootPos = root.transform.position;
        float distFromRoot = Vector3.Distance(targetPos, rootPos);
        int iterations = 10;

        while (distFromRoot > maxRange && iterations > 0)
        {
            targetPos -= (forwardDirection * 0.005f * sign);
            iterations--;
        }
        
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
        if (range > 0)
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
        float tipToMid = Vector3.Distance(tip.position, mid.position);
        float midToRoot = Vector3.Distance(mid.position, root.position);

        return tipToMid + midToRoot;    
    }


    /// <summary>
    /// Returns the average weight of all components that form the associated limb
    /// </summary>
    /// <returns></returns>
    private float GetAvgChainWeight()
    {
        (float rootW, float midW, float tipW) = GetChainWeights();

        return (rootW + midW + tipW) / 3;
    }

    public (float root, float mid, float tip) GetChainWeights()
    {
        float rootW = root.GetComponent<Weight>() ? root.GetComponent<Weight>().weight : 1;
        float midW = mid.GetComponent<Weight>() ? mid.GetComponent<Weight>().weight : 1;
        float tipW = tip.GetComponent<Weight>() ? tip.GetComponent<Weight>().weight : 1;

        return (rootW, midW, tipW);
    }


    /// <summary>
    /// Returns true if the point is not inside an Untraversable terrain collider
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool IsTraversable(Vector3 point)
    {
        Vector3 com = Character.CenterOfMass;
        Vector3 rayDirection = (point - com).Rescale(0.98f);        // Only use 98% of the ray, since we don't want to hit the ground
        RaycastHit[] hits = Physics.RaycastAll(com, rayDirection, rayDirection.magnitude);

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.gameObject.CompareTag(Settings.Tag_Untraversable))
                {
                    //Debug.DrawLine(com, hits[i].point, Color.red, 1f);
                    return false;
                }
            }
        }

        return true;
    }

}
