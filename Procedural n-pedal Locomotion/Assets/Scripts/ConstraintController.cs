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

    private float distanceThreshold = 1f;
    private float proximityThreshold = 0.01f;
    private float speed = 10f;
    public bool IsMoving { get; private set; }


    void Awake()
    {
        originalPos = transform.position;
        tip = TwoBoneIKConstraint.data.tip;
        IsMoving = false;
    }


    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (!IsMoving)
        {
            // Check if the distance to the target point is too great
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // Check if the opposite limb is already moving
            bool isOppositeMoving = opposite != null && opposite.IsMoving;
            
            if (distanceToTarget > distanceThreshold && !isOppositeMoving)
            {
                // Start moving the limb
                Coro.Perp(transform, target.position, 0.2f, OnMovementEnd);
                print("From: " + transform.position + " > " + target.position);
                IsMoving = true;
            }
            else
            {
                // Keep anchored
                transform.position = originalPos;
            }
        }
        else
        {
            /*
            // Move the leg to target
            //transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
            transform.position = MathParabolic.Parabola(transform.position, target.position, speed * Time.deltaTime);
            print(speed * Time.deltaTime);

            // Keep moving until very close to the target
            IsMoving = Vector3.Distance(transform.position, target.position) > proximityThreshold;

            // If the movement is done, cleanely reset the positions
            if (!IsMoving)
            {
                print("---");
                transform.position = target.position;
                originalPos = transform.position;
            }
            */
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
        difference = distanceThreshold / 2;
        target.parent.position = new Vector3(target.parent.position.x, target.parent.position.y, target.parent.position.z + difference);
    }





    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
    }
}
