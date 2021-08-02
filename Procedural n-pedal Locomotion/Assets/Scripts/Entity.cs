using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Entity : MonoBehaviour
{
    [SerializeField] private GameObject body;

    [Tooltip("Axis along whitch to elevate the limb during locomotion.")] 
    public Settings.Axes limbUpwardsAxis = Settings.Axes.Y;

    [SerializeField, Range(0.1f, 50f), Tooltip("Speed at which to realign the entity's body when waling on slopes.")] 
    private float realignmentSpeed = 25f;

    [SerializeField, Range(0.01f, 1f), Tooltip("Min height difference above which to start rotating the body.")]
    private float realignmentThreshold = 0.1f;

    [SerializeField] private bool useZigzagMotion = true;
    private float zigzagDifference = 1f;

    [SerializeField, Tooltip("Automatically add a Capsule Collider to each bone.")] 
    private ColliderGeneration generateBoneColliders;


    public MovementController MovementController => GetComponent<MovementController>();
    public List<ConstraintController> limbs;
    private int groundMask;
    public float BodyWeight { get; private set; }
    public float TotalWeight { get; private set; }
    public bool IsUpdatingGait { get; private set; }
    public bool IsRotating { get; private set; }
    public Vector3 CenterOfMass { get; private set; }
    public enum ColliderGeneration { DontGenerate, CompleteBody, EachLimb }


    void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");

        limbs = new List<ConstraintController>(GetComponentsInChildren<ConstraintController>());

        // Find the center of mass
        CenterOfMass = ComputeCenterOfMass();

        TotalWeight = ComputeWeights();
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

        if (generateBoneColliders != ColliderGeneration.DontGenerate)
        {
            GenerateBoneColliders();
        }
    }



    void FixedUpdate()
    {
        // Update the center of mass
        UpdateCenterOfMass();


        // Set the entity's height based on the limb tips.
        // Do we update this only after a limb has reached its target?
        UpdateGait();
    }




    private void UpdateGait()
    {
        Vector3 direction = transform.TransformDirection(Vector3.down);

        if (Physics.Raycast(CenterOfMass, direction, out RaycastHit groundHit, Mathf.Infinity, groundMask))
        {
            //Debug.DrawRay(CenterOfMass, direction * groundHit.distance, Color.yellow);

            bool isRotEnough = true;

            if (!AreLegsMoving())
            {
                // Rotate to match the limb positions
                Quaternion targetRot = FindRotation();

                // Only rotate if there is enough of a difference in rotations
                isRotEnough = Mathf.Abs(Quaternion.Angle(body.transform.rotation, targetRot)) <= 1.1f;
                if (!isRotEnough)
                {
                    body.transform.rotation = Quaternion.Lerp(body.transform.rotation, targetRot, Time.deltaTime * realignmentSpeed);
                }
                else
                {
                    body.transform.rotation = targetRot;
                }
            }



            // If there is a difference in height AND its not already rotating
            if (transform.position.y != groundHit.point.y && isRotEnough)
            {
                // Update the height
                Vector3 targetPos = new Vector3(transform.position.x, groundHit.point.y, transform.position.z);

                // Only update it if there is enough of a difference
                bool isPosEnough = Mathf.Abs((transform.position - targetPos).magnitude) <= 0.1f;
                if (!isPosEnough)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * realignmentSpeed);
                }
                else
                {
                    transform.position = targetPos;
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

    private float ComputeWeights()
    {
        float w = 0;

        // Add the body's weight
        if (body)
        {
            Weight bodyW = body.GetComponent<Weight>();
            w += bodyW ? bodyW.weight : 1;
            
            // Also update the Body Weight variable
            BodyWeight = bodyW ? bodyW.weight : 1;
            if (BodyWeight == 0) BodyWeight = 0.0001f;
        }

        // Add the weight of each limb
        for (int i = 0; i < limbs.Count; i++)
        {
            Weight root = limbs[i].TwoBoneIKConstraint.data.root.gameObject.GetComponent<Weight>();
            Weight mid = limbs[i].TwoBoneIKConstraint.data.mid.gameObject.GetComponent<Weight>();

            if (root) w += root.weight;
            if (mid)  w += mid.weight;  
        }

        print("total w: " + w);

        return w;
    }


    public void LimitMovement(Vector3 limitingPos)
    {
        // Ignore the Y-axis of the forward vector
        Vector3 forward = new Vector3(body.transform.forward.x, 0, body.transform.forward.z);

        // Find the applied vector from the CoM to the limb target
        Vector3 comToTarget = limitingPos - CenterOfMass;

        // Ignore the y-axis and normalize it
        comToTarget = new Vector3(comToTarget.x, 0, comToTarget.z).normalized;

        // Find the angle between the forward vector and the target vector
        float angle = Vector3.SignedAngle(forward, comToTarget, body.transform.up);

        // TODO check that the sign is correct?

        // Send the values from 0 to angle to the movement controller to limit movement in that direction
        MovementController.SetArcLimit((0, angle));
    }




    /// <summary>
    /// Finds the new rotation values along the X and Z axis to remain stable, based on the relative positons of the limbs
    /// </summary>
    /// <returns></returns>
    private Quaternion FindRotation()
    {
        List<float> angles = new List<float>();
        int rotXDirection, rotZDirection;                       // Signs of rotation

        // Find the rotation along X
        float rotX;
        {
            // Find the angle differences between different limbs
            for (int i = 0; i < limbs.Count - 2; i++)
            {
                Vector3 a = limbs[i].transform.position;        // Pos of the first limb tip
                Vector3 b = limbs[i + 2].transform.position;    // Pos of the second limb tip
                Vector3 c;                                      // Pos C to make a right triangle ACB

                // Skip the calculation if the limbs are (almost) at the same height
                if (Mathf.Abs(a.y - b.y) <= realignmentThreshold)
                {
                    angles.Add(0);
                    continue;
                }

                // Make sure C is parallel to the lower point
                if (a.y > b.y)
                {
                    c = new Vector3(a.x, b.y, a.z);
                    rotXDirection = -1;
                }
                else
                {
                    c = new Vector3(b.x, a.y, b.z);
                    rotXDirection = 1;
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
                angle *= rotXDirection;

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

                // Skip the calculation if the limbs are (almost) at the same height
                if (Mathf.Abs(a.y - b.y) <= realignmentThreshold)
                {
                    angles.Add(0);
                    continue;
                }

                // Make sure C is parallel to the lower point
                if (a.y > b.y)
                {
                    c = new Vector3(a.x, b.y, a.z);
                    rotZDirection = -1;
                }
                else
                {
                    c = new Vector3(b.x, a.y, b.z);
                    rotZDirection = 1;
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
                angle *= rotZDirection;

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

        Quaternion rotation = Quaternion.Euler(eulerAngles);


        return rotation;
    }


    /// <summary>
    /// Returns true if one or more legs are currently moving
    /// </summary>
    /// <returns></returns>
    private bool AreLegsMoving()
    {
        for (int i = 0; i < limbs.Count; i++)
        {
            if (limbs[i].IsMoving)
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Adds Collider components to the Entity depending on the choosen ColliderGeneration
    /// </summary>
    private void GenerateBoneColliders()
    {
        // Chooses how to add colliders to the Entity depending on the generateBoneCollider enum

        // Adds a single Box Collider to the complete body of the entity
        if (generateBoneColliders == ColliderGeneration.CompleteBody)
        {      
            BoxCollider bodyCollider = gameObject.AddComponent<BoxCollider>();

            // Find the biggest Renderer inside the entity
            Vector3 biggestSize = Vector3.zero;
            Vector3 center = Vector3.zero;

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (biggestSize == Vector3.zero)
                {
                    biggestSize = r.bounds.size;
                    center = r.bounds.center;
                }
                else if (r.bounds.size.IsBiggerThan(biggestSize)) 
                {
                    biggestSize = r.bounds.size;
                    center = r.bounds.center;
                }
            }

            // Use its bounds to determine the collider size and center
            bodyCollider.center = center;
            bodyCollider.size = biggestSize / 2;
        }

        // Generate box colliders around each limb bone   TODO
        else if (generateBoneColliders == ColliderGeneration.EachLimb)
        {
            for (int i = 0; i < limbs.Count; i++)
        {
            ConstraintController limb = limbs[i];
            GameObject root = limb.TwoBoneIKConstraint.data.root.gameObject;
            GameObject mid = limb.TwoBoneIKConstraint.data.mid.gameObject;
            GameObject end = limb.TwoBoneIKConstraint.data.tip.parent.gameObject;
            GameObject tip = limb.TwoBoneIKConstraint.data.tip.gameObject;

            Vector3 a = root.transform.localPosition;
            Vector3 b = mid.transform.localPosition;
            Vector3 globalScale = root.transform.lossyScale;


            Vector3 center = (a + b) / 2;                        // Find the midway point to use as the center
            float height = Vector3.Distance(a.DivideBy(globalScale), b.DivideBy(globalScale));
            print(height); 
            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.center = center;
            collider.height = height;
            collider.radius = height / 4;
        }
        }
    }


    public void GenerateWeights()
    {
        // Add a Weight component to the body
        if (body != null)
        {
            if (body.GetComponent<Weight>() == null)
            {
                body.AddComponent<Weight>();
            }
        }

        // If the limbs have not been set yet, find the automatically
        if (limbs == null || limbs.Count == 0)
        {
            limbs = new List<ConstraintController>(GetComponentsInChildren<ConstraintController>());
        }

        // Add a Weight component to all limb parts
        for (int i = 0; i < limbs.Count; i++)
        {
            GameObject root = limbs[i].TwoBoneIKConstraint.data.root.gameObject;
            GameObject mid = limbs[i].TwoBoneIKConstraint.data.mid.gameObject;

            if (root.GetComponent<Weight>() == null)
            {
                root.AddComponent<Weight>();
                mid.AddComponent<Weight>();
            }
        }
    }

    public void RemoveWeights()
    {
        // Remove the Weight component from the body
        if (body != null)
        {
            Weight w = body.GetComponent<Weight>();
            if (w != null) DestroyImmediate(w, false);
        }

        // If the limbs have not been set yet, find the automatically
        if (limbs == null || limbs.Count == 0)
        {
            limbs = new List<ConstraintController>(GetComponentsInChildren<ConstraintController>());
        }

        // Remove the Weight component from all limb parts
        for (int i = 0; i < limbs.Count; i++)
        {
            Weight rootW = limbs[i].TwoBoneIKConstraint.data.root.gameObject.GetComponent<Weight>();
            Weight midW = limbs[i].TwoBoneIKConstraint.data.mid.gameObject.GetComponent<Weight>();

            if (rootW != null) DestroyImmediate(rootW, false);
            if (midW != null)  DestroyImmediate(midW, false);
        }
    }




    // Not used ------------------------------------------------------------------------------------

    /// <summary>
    /// Returns the transform's global scale by recursively multiplying all inherited scales
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    private Vector3 GetInheritedScale(Transform child)
    {
        Vector3 thisScale = child.localScale;

        if (child.parent == null)
        {
            return thisScale;
        }
        else
        {
            Vector3 parentScale = GetInheritedScale(child.parent);

            return Vector3.Scale(thisScale, parentScale);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(CenterOfMass, .05f);
    }
}