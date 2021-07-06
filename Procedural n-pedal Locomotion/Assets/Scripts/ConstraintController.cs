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

    private TwoBoneIKConstraint TwoBoneIKConstraint { get { return GetComponent<TwoBoneIKConstraint>(); } }

    [SerializeField, Min(0.1f), Tooltip("Distance after which to take a step.")]
    private float distanceThreshold = 1.5f;

    private float proximityThreshold = 0.01f;

    [SerializeField, Range(0.1f, 10f), Tooltip("Movement Speed.")]
    private float speed = 2f;

    [SerializeField, Min(0.1f), Tooltip("Maximum height reached by the limb during its limb's arching animation.")]
    private float limbMovementHeight = 0.5f;

    public bool IsMoving { get; private set; }
    public Transform TipTransform { get; private set; }

    private Entity ParentEntity;

    void Awake()
    {
        ParentEntity = GetComponentInParent<Entity>();
        tip = TwoBoneIKConstraint.data.tip;
        TipTransform = tip.transform;
        IsMoving = false;

        // Pass the tip down to the Anchor for initial positioning
        target.gameObject.GetComponent<GroundAnchor>().SetTip(tip);
    }


    void Update()
    {
        Move();

        if (tip.name == "Bone.011_end") { 
        print(tip.transform.position);
        }
    }

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
    /// Moves the target forward to simulate a quadrupedal locomotion pattern
    /// </summary>
    /// <param name="difference"></param>
    public void ForwardTarget(float difference)
    {
        // TODO use differnece
        difference = distanceThreshold / 4;
        target.parent.position = new Vector3(target.parent.position.x, target.parent.position.y, target.parent.position.z + difference);
    }





    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
    }
}
