using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(TwoBoneIKConstraint))]
public class ConstraintController : MonoBehaviour
{
    [SerializeField] Transform target;
    
    private Vector3 originalPos;
    private Transform tip;

    private TwoBoneIKConstraint TwoBoneIKConstraint { get { return GetComponent<TwoBoneIKConstraint>(); } }

    private float distanceThreshold = 1.5f;
    private float proximityThreshold = 0.01f;
    private float speed = 10f;
    private bool isMoving = false;


    void Awake()
    {
        originalPos = transform.position;
        tip = TwoBoneIKConstraint.data.tip;
    }


    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (!isMoving)
        {
            // Check if the distance to the target point is too great
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget > distanceThreshold)
            {
                // Start moving the limb
                isMoving = true;
            }
            else
            {
                // Keep anchored
                transform.position = originalPos;
            }
        }
        else
        {
            print("moving");
            // Move the leg to target
            transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);

            // Keep moving until very close to the target
            isMoving = Vector3.Distance(transform.position, target.position) > proximityThreshold;

            // If the movement is done, cleanely reset the positions
            if (!isMoving)
            {
                transform.position = target.position;
                originalPos = transform.position;
            }
        }
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
