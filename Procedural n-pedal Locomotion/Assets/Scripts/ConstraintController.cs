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
    private float proximityThreshold = 0.1f;
    private float speed = 5f;
    private bool isMoving = false;


    void Start()
    {
        originalPos = transform.position;
        tip = TwoBoneIKConstraint.data.tip;

    }

    void Update()
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
    }
}
