/* 
 * This file is part of the Procedural-Locomotion repo on github.com/ZykeDev 
 * Marco Vincenzi - 2021
 */

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    private CharacterController Controller => GetComponent<CharacterController>();
    private LocomotionSystem LocomotionSystem => GetComponent<LocomotionSystem>();

    // Pair of degrees between which to limit directional movement.
    // i.e. (0, 90) only allows movement in a direction if its forward vector
    // points towards a 90� to 360� range around the center.
    private List<(float from, float to)> ArcLimits = new List<(float from, float to)>();

    [SerializeField, Tooltip("Allows the character to only move in a direction where limb targets are permitted.")] 
    private bool useDirectionLimiter = false;

    [Space]
    [SerializeField, Tooltip("Speed at which the character moves")]
    private float speed = 3f;

    [SerializeField, Range(0.1f, 10f), Tooltip("Speed at which the character turns on itself.")] 
    private float turnSpeed = 3f;
    private float turnVelocity;


    [Tooltip("Enables the sprint feature. Sprint can be used by holding the Shift key.")]
    public bool enableSprint = true;

    [Range(1f, 10f)]
    public float sprintMultiplier = 2f;

    [Space]
    [SerializeField, Tooltip("Shifts the position of the Character Controller's center.")]
    private Vector3 centerShift = Vector3.zero; 

    private void Start()
    {
        UpdateCenter();

        // Minimize the skin width value
        Controller.skinWidth = 0.0001f;

        // Set the pill height to 1 to not interfere with ground bobbing
        Controller.height = 1;
    }

    void Update()
    {
        UpdateCenter();

        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        bool isSprinting = Input.GetKey(Settings.Sprint_Key);

        Vector3 direction = new Vector3(hor, 0f, ver).normalized;

        // Make the direction local to the transform
        direction = transform.TransformDirection(direction);

        bool canMove = CanMove(direction);

        if (canMove && direction.magnitude >= 0.1f)
        {
            float bodyWeight = LocomotionSystem ? LocomotionSystem.BodyWeight : 1;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, bodyWeight / turnSpeed);
            float targetSpeed = speed / bodyWeight;

            if (isSprinting && enableSprint)
            {
                targetSpeed *= sprintMultiplier;
            }

            // Rotate and Move
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Controller.Move(direction * targetSpeed * Time.deltaTime);
        }
    }

    private void UpdateCenter()
    {
        // Use the CoM as the geometrical center
        if (LocomotionSystem)
        {
            // Transform the CoM to Local Coordinates
            Controller.center = transform.InverseTransformPoint(LocomotionSystem.CenterOfMass + centerShift);
        }
    }



    /// <summary>
    /// Returns true if the character is allowed to move in the given direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool CanMove(Vector3 direction)
    {
        // Ignore if the feature is off
        if (!useDirectionLimiter) return true;

        // Convert the direction Vector3 to Vector2
        // TODO this only works for movement on a xz-plane.
        // Might be useful to take into account the y-component for vertical motion.
        Vector2 unitDirection = new Vector2(direction.x, direction.z);

        for (int i = 0; i < ArcLimits.Count; i++)
        {
            (float from, float to) arc = ArcLimits[i];

            // Skip if there is no limit
            if (arc == (0, 0)) continue;

            // Convert the direction into an angle
            float dirAngle = Mathf.Atan2(unitDirection.x, unitDirection.y) * Mathf.Rad2Deg;
  
            // Rotate everything by 90 degrees to align with the direction's unit circle
            float from = -arc.from + 90;
            float to = -arc.to + 90;
            dirAngle = -dirAngle + 90;

            // Rearrange the tuple so that From <= To
            if (from > to) (from, to) = (to, from);

            // If the direction point is in the arc segment, DON'T move in that direction
            bool isBetween = from <= dirAngle && dirAngle <= to;
            if (isBetween)
            {
                return false;
            }
        }
    
        return true;
    }


    public void ResetArcLimit(int id) => SetArcLimit((0, 0), id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="form"></param>
    /// <param name="to"></param>
    public void SetArcLimit(float form, float to, int id) => SetArcLimit((form, to), id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arc">Float tuple</param>
    public void SetArcLimit((float from, float to) arc, int id)
    {
        // Add empty limits to the list until it can fit the given ID
        if (ArcLimits.Count <= id)
        {
            for (int i = ArcLimits.Count; i <= id; i++)
            {
                ArcLimits.Add((0, 0));
            }
        }

        // Add the set limit to the list, at the same index as the limb
        ArcLimits[id] = arc;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.DrawWireDisc(LocomotionSystem.CenterOfMass, Vector3.up, 1.5f);
    }
#endif

}
