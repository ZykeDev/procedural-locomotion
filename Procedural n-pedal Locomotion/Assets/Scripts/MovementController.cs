using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    private CharacterController Controller => GetComponent<CharacterController>();
    private Entity Entity => GetComponent<Entity>();

    [SerializeField, Range(0.01f, 20f)] private float speed = 10f;
    [SerializeField, Range(0.1f, 50f)] private float turnSpeed = 5f;
    private float turnVelocity;

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

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, 1 / turnSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Controller.Move(direction * speed * Time.deltaTime);
            
        }


    }
}
