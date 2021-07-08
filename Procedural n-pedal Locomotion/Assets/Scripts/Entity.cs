using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private List<ConstraintController> limbs;

    [SerializeField, Range(0.1f, 50f), Tooltip("Speed at which to realign the entity's body when waling on slopes.")] 
    private float realignmentSpeed = 25f;

    [SerializeField] private bool useZigzagMotion = true;
    private float zigzagDifference = 1f;

    public bool IsUpdatingGait { get; private set; }
    public bool IsRotating { get; private set; }
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


        // Update the distance to the ground below
        //                                -transform.up
        if (Physics.Raycast(CenterOfMass, transform.TransformDirection(Vector3.down), out groundHit, Mathf.Infinity, groundMask))
        {
            Debug.DrawRay(CenterOfMass, transform.TransformDirection(Vector3.down) * groundHit.distance, Color.yellow);

            // If there is a difference in height
            if (transform.position.y != groundHit.point.y)
            {
                // Update the height and rotation
                Vector3 targetPos = new Vector3(transform.position.x, groundHit.point.y, transform.position.z);
                Quaternion targetRot = FindRotation();

                bool isPosEnough = Mathf.Abs((transform.position - targetPos).magnitude) <= 0.1f;
                bool isRotEnough = Mathf.Abs(Quaternion.Angle(transform.rotation, targetRot)) <= 1.1f;

                print(transform.rotation + ", " + targetRot + ", " + isRotEnough);

                if (isPosEnough && isRotEnough)
                {
                    transform.position = targetPos;
                    transform.rotation = targetRot;
                    IsRotating = false;
                }
                else
                {
                    IsRotating = true;
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * realignmentSpeed);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * realignmentSpeed);
                }
            }
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


    /// <summary>
    /// Finds the new rotation values along the X and Z axis to remain stable, based on the relative positons of the limbs
    /// </summary>
    /// <returns></returns>
    private Quaternion FindRotation()
    {
        Quaternion rotation = new Quaternion();
        List<float> angles = new List<float>();
        float rotationAdjustment = 1; //90;

        // Find the rotation along X
        float rotX;
        {
            // Find the angle differences between different limbs
            for (int i = 0; i < limbs.Count - 2; i++)
            {
                Vector3 a = limbs[i].transform.position;        // Pos of the first limb tip
                Vector3 b = limbs[i + 2].transform.position;    // Pos of the second limb tip
                Vector3 c;                                      // Pos C to make a right triangle ACB
                int rotDirection;                               // Sign of the rotation

                // Make sure C is parallel to the lower point
                if (a.y > b.y)
                {
                    c = new Vector3(a.x, b.y, a.z);
                    rotDirection = 1;
                }
                else
                {
                    c = new Vector3(b.x, a.y, b.z);
                    rotDirection = -1;
                }

                // Get the triangle sides
                float hypotenuse = Vector3.Distance(a, b);
                float opposite = Vector3.Distance(a, c);
                float adjacent = Vector3.Distance(b, c);

                // Find both angles and use the smalles one (acute)
                float theta = Mathf.Asin(opposite / hypotenuse) * Mathf.Rad2Deg;
                float gamma = Mathf.Asin(adjacent / hypotenuse) * Mathf.Rad2Deg;

                // Find the lesser angle between the two
                float angle = theta <= gamma ? theta : gamma;

                // Adjust the rotation to match the body
                angle += rotDirection * rotationAdjustment;

                angles.Add(angle);
            }

            // Average out the angles
            float angleSum = 0;
            for (int i = 0; i < angles.Count; i++)
            {
                angleSum += angles[i];
            }

            rotX = angleSum / angles.Count;
        }

        // Find the rotation along Z
        float rotZ;
        {
            // Find the angle differences between different limbs
            for (int i = 0; i < limbs.Count - 1; i += 2)
            {
                Vector3 a = limbs[i].transform.position;        // Pos of the first limb tip
                Vector3 b = limbs[i + 1].transform.position;    // Pos of the second limb tip
                Vector3 c;                                      // Pos C to make a right triangle ACB
                int rotDirection;                               // Sign of the rotation

                // Make sure C is parallel to the lower point
                if (a.y > b.y)
                {
                    c = new Vector3(a.x, b.y, a.z);
                    rotDirection = 1;
                }
                else
                {
                    c = new Vector3(b.x, a.y, b.z);
                    rotDirection = -1;
                }

                // Get the triangle sides
                float hypotenuse = Vector3.Distance(a, b);
                float opposite = Vector3.Distance(a, c);
                float adjacent = Vector3.Distance(b, c);

                // Find both angles and use the smalles one (acute)
                float theta = Mathf.Asin(opposite / hypotenuse) * Mathf.Rad2Deg;
                float gamma = Mathf.Asin(adjacent / hypotenuse) * Mathf.Rad2Deg;

                // Find the lesser angle between the two
                float angle = theta <= gamma ? theta : gamma;

                // Adjust the rotation to match the body
                angle += rotDirection * rotationAdjustment;

                angles.Add(angle);
            }

            // Average out the angles
            float angleSum = 0;
            for (int i = 0; i < angles.Count; i++)
            {
                angleSum += angles[i];
            }

            rotZ = angleSum / angles.Count;
        }


        Vector3 eulerAngles = transform.rotation.eulerAngles;
        eulerAngles.x = Mathf.RoundToInt(rotX);
        eulerAngles.z = Mathf.RoundToInt(rotZ);

        rotation = Quaternion.Euler(eulerAngles);

        //print("Rot to " + eulerAngles);

        return rotation;
    }




    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(CenterOfMass, .05f);

        Vector3 a = limbs[0].transform.position;        // Pos of the first limb tip
        Vector3 b = limbs[0 + 2].transform.position;    // Pos of the second limb tip
        Vector3 c;                                      // Pos C to make a right triangle ACB

        // Make sure C is parallel to the lower point
        if (a.y > b.y)
        {
            c = new Vector3(a.x, b.y, a.z);
        }
        else
        {
            c = new Vector3(b.x, a.y, b.z);
        }

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(a, c);
        Gizmos.DrawLine(b, c);
    }
}