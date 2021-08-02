using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    private CharacterController Controller => GetComponent<CharacterController>();
    private Entity Entity => GetComponent<Entity>();

    // Pair of degrees between which to limit directional movement.
    // i.e. (0, 90) only allows movement in a direction if its forward vector
    // points towards a 90° to 360° range around the entity's center.
    private (float from, float to) arcLimit = (0, 0);

    private float turnVelocity;


    [SerializeField, Range(0.01f, 20f)] 
    private float speed = 10f;

    [SerializeField, Range(0.1f, 50f)] 
    private float turnSpeed = 5f;

    [SerializeField, Tooltip("Allows the entity to only move in a direction where limb targets are permitted.")] 
    private bool useDirectionLimiter = false;



    private void Start()
    {
        // Use the CoM as the geometrical center
        // TODO for now its only using the Y component
        Controller.center = new Vector3(0, Entity.CenterOfMass.y, 0);
    }

    void Update()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(hor, 0f, ver).normalized;

        bool canMove = CanMove(direction);

        if (canMove && direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, 1 / turnSpeed * Entity.BodyWeight);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Controller.Move(direction * (speed / Entity.BodyWeight) * Time.deltaTime);
        }
    }



    /// <summary>
    /// Returns true if the entity is allowed to move in the given direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool CanMove(Vector3 direction)
    {
        // Ignore if the feature is off
        if (!useDirectionLimiter) return true;

        // Ignore the case with no limits
        if (arcLimit == (0, 0)) return true;


        // We only need the X and Z coordinates
        Vector2 unitDirection = new Vector2(direction.x, direction.z);

        // Rotate the arc by 90 degrees to align it with the direction's unit circle
        float from = arcLimit.from + 90;
        float to = arcLimit.to + 90;

        // Convert the angles to unit-circle coordinates
        Vector2 fromV = from.ToUnitCirclePoint();
        Vector2 toV = to.ToUnitCirclePoint();

        // Check if the direction is between the limits
        bool isBetweenX = unitDirection.x >= fromV.x && unitDirection.x <= toV.x;
        bool isBetweenY = unitDirection.y >= fromV.y && unitDirection.y <= toV.y;

        // If the direction is in the arc limit, DON'T allow movement in said direction
        if (isBetweenX && isBetweenY)
        {
            return false;
        }
    
        return true;
    }




    /// <summary>
    /// 
    /// </summary>
    /// <param name="form"></param>
    /// <param name="to"></param>
    public void SetArcLimit(float form, float to) => SetArcLimit((form, to));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arc">Float tuple</param>
    public void SetArcLimit((float from, float to) arc)
    {
        // Improve angles TODO
        //print("settign limit " + arc.from + " -> " + arc.to);

        arcLimit = (arc.from, arc.to);
        arcLimit = (arc.to, arc.from); // TODo remove temp

    }

    public void ResetArcLimit() => SetArcLimit(0, 0);

    public void SetArcLimits((float from, float to)[] arcs)
    {
        for (int i = 0; i < arcs.Length; i++)
        {
            SetArcLimit(arcs[i]);
        }
    }
}
