using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private List<ConstraintController> limbs;

    [SerializeField] private bool useZigzagMotion = true;
    private float zigzagDifference = 1f;

    public bool IsUpdatingGait { get; private set; }
    private int groundMask;
    private RaycastHit groundHit;
    private Quaternion fromRotation, toRotation;

    public Vector3 CenterOfMass { get; private set; }

    void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");

        limbs = new List<ConstraintController>(GetComponentsInChildren<ConstraintController>());

        // Find the center of mass
        CenterOfMass = ComputeCenterOfMass();
    }

    void Start()
    {
        if (useZigzagMotion)
        {
            for (int i = 0; i < limbs.Count; i++)
            {
                if (i % 2 != 0) limbs[i].ForwardTarget(zigzagDifference);
            }
        }
    }



    void LateUpdate()
    {
        // Set the entity's height based on the limb tips.
        // Do we update this only after a limb has reached its target?
        UpdateGait();

        // Rotate body based on weigthed limb vectors


        // Update the center of mass
        UpdateCenterOfMass();

    }






    private void UpdateGait()
    {
        // Basically, find the target normal of the plane passing from all limb coordinates
        // A plane will always interesct 3 points, but what about the other limbs?
        // We can:
        // Wait until 3 points are disaligned, then create a plane passing through them, and realign the rest n-3.
        // or
        // Always give priority to the top-3 most dialigned points, find the plane, and realign the rest n-3.
        // or
        // Force the distance vector between the body and the ground to never change.
        /*
        List<Vector3> tipCoords = new List<Vector3>();

        for (int i = 0; i < limbs.Count; i++)
        {
            tipCoords.Add(limbs[i].TipTransform.position);
        }
        */

        if (!IsUpdatingGait)
        {
            // Update the distance to the ground below
            //                                -transform.up
            if (Physics.Raycast(CenterOfMass, transform.TransformDirection(Vector3.down), out groundHit, Mathf.Infinity, groundMask))
            {
                Debug.DrawRay(CenterOfMass, transform.TransformDirection(Vector3.down) * groundHit.distance, Color.yellow);
                
                // If there is a difference in height
                if (transform.position.y != groundHit.point.y)
                {
                    Vector3 newPos = new Vector3(transform.position.x, groundHit.point.y, transform.position.z);
                    transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 20);
                }
            }
            /*
            List<Vector3> tips = new List<Vector3>();
            for (int i = 0; i < limbs.Count; i++)
            {
                tips.Add(limbs[i].transform.position);
            }

            // Find the centroid
            Vector3 centroid = tips.Aggregate(Vector3.zero, (tot, v) => tot + v) / tips.Count;

            // Find the normal of the plane with the centroid at its center, using the first and penultimate tip points
            Vector3 normal = Vector3.Cross(tips[0] - centroid, tips[tips.Count - 1] - centroid);

            // Rotate the entity to align with the normal
            fromRotation = transform.rotation;
            toRotation = Quaternion.FromToRotation(transform.up, normal);

            // Rotate only if necessary
            Quaternion delta = transform.rotation * Quaternion.Inverse(toRotation);

            if (delta.eulerAngles.magnitude > 1f)
            {
                //transform.rotation = Quaternion.Slerp(fromRotation, toRotation, Time.deltaTime * 2);
                transform.rotation = toRotation;
            }
            */
        }
    }




    private void UpdateCenterOfMass()
    {
        CenterOfMass = ComputeCenterOfMass();
    }

    private Vector3 ComputeCenterOfMass()
    {
        Vector3 com = transform.position + Vector3.up * 1f;

        return com;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(CenterOfMass, .05f);
    }
}