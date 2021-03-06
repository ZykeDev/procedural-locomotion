/* 
 * This file is part of the Procedural-Locomotion repo on github.com/ZykeDev 
 * Marco Vincenzi - 2021
 */

using UnityEngine;

// Keeps the attached Game Object on the ground below.

public class GroundAnchor : MonoBehaviour
{
    private LocomotionSystem Character;
    [HideInInspector] public Transform tip;                  // Tip of the limb. Should be set from the Constraint Controller
    private int layerMask;

    private Vector3 verticalOffset = new Vector3(0, 2f, 0);
    private Vector3 verticalGap = new Vector3(0, 0.001f, 0);    // Short vertical vector
    private Vector3 prevPos;

    [SerializeField, Tooltip("Sets the way the 'upwards' vector is choosen when anchoring the GameObject. Geometrical uses Vector3.down. Local uses -transform.up.")] 
    private Upwards upwards = Upwards.Geometrical;

    private enum Upwards { Geometrical, Local }


    void Awake()
    {
        layerMask = LayerMask.GetMask(Settings.Layer_Ground);
        Character = GetComponentInParent<LocomotionSystem>();
    }

    void FixedUpdate()
    {
        Anchor();
    }

    /// <summary>
    /// Keeps the object anchored to the ground below
    /// </summary>
    private void Anchor()
    {
        if (!Character.IsRotating)
        {
            Vector3 direction = Vector3.down;

            if (upwards == Upwards.Geometrical) direction = Vector3.down;
            else if (upwards == Upwards.Local)  direction = -transform.up;
            

            Vector3 target = prevPos;
            Vector3 cvo = transform.position + verticalOffset;  // Current vertical offset position

            bool isGroundHit = Physics.Raycast(cvo, direction, out RaycastHit groundHit, Mathf.Infinity, layerMask);

            if (isGroundHit)
            {
                //Debug.DrawRay(cvo, direction * groundHit.distance, Color.yellow);
                target = groundHit.point;
            }
            else
            {
                if (tip != null)
                {
                    target = tip.position;
                    prevPos = transform.position;
                }
            }

            if (transform.position != target)
            {
                prevPos = transform.position;
                transform.position = target;
            }            
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + verticalGap, 0.025f);
    }
#endif
}
