using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[DefaultExecutionOrder(-1)]
[RequireComponent(typeof(RigBuilder))]
public class LocomotionSystem : MonoBehaviour
{
    [SerializeField] private GameObject body;

    [Space]
    [Tooltip("Axis along whitch to elevate the limb during locomotion.")]
    public Settings.Axes limbUpwardsAxis = Settings.Axes.Y;

    [SerializeField, Min(0.1f), Tooltip("Distance after which to take a step.")]
    private float stepSize = 1f;

    [Space]
    [SerializeField] private bool useCustomMaxRange = false;
    [SerializeField] private float customMaxRange = 0f;
    [Space]

    [SerializeField, Range(0.1f, 50f), Tooltip("Speed at which to realign the character's body when walking on slopes.")]
    private float realignmentSpeed = 25f;

    [SerializeField, Range(0.01f, 1f), Tooltip("Min height difference above which to start rotating the body.")]
    private float realignmentThreshold = 0.1f;

    [SerializeField, Tooltip("Randomizes the starting locomotion pattern of the limb targets.")]
    private bool randomizeStartingPattern = true;

    [Space]
    [SerializeField, Tooltip("Automatically adds a Capsule Collider to each bone on startup.")]
    private ColliderGeneration generateBoneColliders;


    public MovementController MovementController => GetComponent<MovementController>();

    [HideInInspector] 
    public List<ConstraintController> limbs;

    [Tooltip("References to every limb root of the model. Add them before clicking Setup.")]
    public List<GameObject> limbObjects;

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
    }

    void Start()
    {
        TotalWeight = ComputeWeights();
        CenterOfMass = transform.position;

        for (int i = 0; i < limbs.Count; i++)
        {
            ConstraintController limb = limbs[i];
            limb.id = i;
            limb.SetStepSize(stepSize);

            if (useCustomMaxRange)
            {
                limb.SetMaxRange(customMaxRange);
            }
        }

        if (randomizeStartingPattern)
        {
            int disparity = 1;
            for (int i = 1; i < limbs.Count; i++)
            {
                limbs[i].DisplaceTarget(i, disparity, limbs.Count);
                if (i % 2 == 0) disparity++;
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


        // Set the character's height based on the limb tips.
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
                    float weight = body.GetComponent<Weight>() ? body.GetComponent<Weight>().weight : 1;
                    body.transform.rotation = Quaternion.Lerp(body.transform.rotation, targetRot, Time.deltaTime * (realignmentSpeed / weight));
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
                bool isPosEnough = Mathf.Abs((transform.position - targetPos).magnitude) <= 0.05f;
                if (!isPosEnough)
                {
                    float weight = body.GetComponent<Weight>() ? body.GetComponent<Weight>().weight : 1;
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * (realignmentSpeed / weight));
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
        // Get all limb and body positions and weights
        List<Vector3> positions = new List<Vector3>();
        List<float> weights = new List<float>();

        positions.Add(body.transform.position);
        weights.Add(body.GetComponent<Weight>() ? body.GetComponent<Weight>().weight : 1);

        for (int i = 0; i < limbs.Count; i++)
        {
            positions.Add(limbs[i].root.position);
            positions.Add(limbs[i].mid.position);
            positions.Add(limbs[i].tip.position);

            (float rootW, float midW, float tipW) = limbs[i].GetChainWeights();

            weights.Add(rootW);
            weights.Add(midW);
            weights.Add(tipW);
        }

        // Make all weights add up to 1
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i] /= TotalWeight;
        }

        // Use a weighted avg to find the position for the center of mass
        Vector3 avg = Extensions.WeightedAverage(positions, weights);

        CenterOfMass = avg;
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
            BodyWeight = w;

            if (BodyWeight == 0)
            {
                Debug.LogError("Body weight cannot be 0.");
            }
        }

        // Add the weight of each limb
        for (int i = 0; i < limbs.Count; i++)
        {
            (float rootW, float midW, float tipW) = limbs[i].GetChainWeights();

            w += rootW;
            w += midW;
            w += tipW;
        }

        return w;
    }


    public void LimitMovement(Vector3 limitingPos, int id)
    {
        // Ignore the y-axis of the forward vector
        Vector3 forward = body.transform.forward;
        forward.y = 0;

        // Find the applied vector from the CoM to the limb target
        Vector3 comToTarget = limitingPos - CenterOfMass;

        // Ignore the y-axis and normalize it
        comToTarget = new Vector3(comToTarget.x, 0, comToTarget.z).normalized;

        // Find the angle between the forward vector and the target vector
        float angle = Vector3.SignedAngle(forward, comToTarget, body.transform.up);

        // Skip if the angle is 0
        if (angle == 0) return;


        float theta = 30;               // Min angle of the arc
        float from = angle - theta;
        float to = angle + theta;

        // Find the closest 90-divisible sector point
        float closestFrom = 90 * (int)(from / 90);
        if (Mathf.Abs(from) % 90 > 45) closestFrom += 90;

        float closestTo = 90 * (int)(to / 90);
        if (Mathf.Abs(to) % 90 > 45) closestTo += 90;

        // Only choose the clostestest point
        if (Mathf.Abs(closestFrom - from) < Mathf.Abs(closestTo - to))
        {
            from = closestFrom;
        }
        else
        {
            to = closestTo;
        }


        // Send the values to the movement controller to limit movement in that direction
        MovementController?.SetArcLimit((from, to), id);
    }

    


    /// <summary>
    /// Finds the new rotation values along the X and Z axis to remain stable, based on the relative positons of the limbs
    /// </summary>
    /// <returns></returns>
    private Quaternion FindRotation()
    {
        List<float> angles = new List<float>();
        int rotXDirection, rotZDirection;       // Signs of rotation

        Vector3 forward = transform.forward;
        //Debug.DrawLine(CenterOfMass, CenterOfMass + forward, Color.red, 1);
        
        // Find the rotation along X
        float rotX;
        {
            Vector3 rotXForward = forward.RoundToInt();

            // Find the angle differences between different limbs       
            for (int i = 0; i < limbs.Count - 2; i++)
            {
                Vector3 a = limbs[i].transform.position;        // Pos of the first limb tip
                Vector3 b = limbs[i + 2].transform.position;    // Pos of the second limb tip
                Vector3 c;                                      // Point C to make a right triangle ACB
                
                // Skip this step if the limbs are (almost) at the same height
                if (Mathf.Abs(a.y - b.y) <= realignmentThreshold)
                {
                    angles.Add(0);
                    continue;
                }

                // Make sure C is parallel to the lower point
                if (a.y > b.y)
                {
                    c = new Vector3(a.x, b.y, a.z);
                }
                else
                {
                    c = new Vector3(b.x, a.y, b.z);
                }

                Vector3 aF = a.MultiplyBy(rotXForward);
                Vector3 bF = b.MultiplyBy(rotXForward);
                Vector3 ahead, behind;

                if (aF.IsGreaterThan(bF))  // if A is ahead of B (forward)
                {
                    ahead = a;
                    behind = b;
                }
                else
                {
                    ahead = b;
                    behind = a;
                }

                if (ahead.y > behind.y)
                {
                    rotXDirection = -1;
                }
                else
                {
                    rotXDirection = 1;
                }


                //Debug.DrawLine(a, b, Color.white);
                //Debug.DrawLine(b, c, Color.white);
                //Debug.DrawLine(a, c, Color.white);

                // Get the triangle sides
                float hypotenuse = Vector3.Distance(a, b);
                float opposite = Vector3.Distance(a, c);
                float adjacent = Vector3.Distance(b, c);

                // Find both angles and use the smalles one (acute)
                float theta = Mathf.Asin(opposite / hypotenuse);
                float gamma = Mathf.Asin(adjacent / hypotenuse);

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
            
            rotX = angleSum * Mathf.Rad2Deg / angles.Count;
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
                }
                else
                {
                    c = new Vector3(b.x, a.y, b.z);
                }

                if (a.y > b.y)
                {
                    rotZDirection = -1;
                }
                else
                {
                    rotZDirection = 1;
                }
                
                //Debug.DrawLine(a, b);
                //Debug.DrawLine(b, c);
                //Debug.DrawLine(a, c);

                // Get the triangle sides
                float hypotenuse = Vector3.Distance(a, b);
                float opposite = Vector3.Distance(a, c);
                float adjacent = Vector3.Distance(b, c);

                // Find both angles and use the smalles one (acute)
                float theta = Mathf.Asin(opposite / hypotenuse);
                float gamma = Mathf.Asin(adjacent / hypotenuse);

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

            rotZ = angleSum * Mathf.Rad2Deg / angles.Count;
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
    /// Adds Collider components to the character depending on the choosen ColliderGeneration
    /// </summary>
    private void GenerateBoneColliders()
    {
        // Chooses how to add colliders to the character depending on the generateBoneCollider enum

        // Adds a single Box Collider to the complete body of the character
        if (generateBoneColliders == ColliderGeneration.CompleteBody)
        {      
            BoxCollider bodyCollider = gameObject.AddComponent<BoxCollider>();

            // Find the biggest Renderer inside the character
            Vector3 biggestSize = Vector3.zero;
            Vector3 center = Vector3.zero;

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (biggestSize == Vector3.zero)
                {
                    biggestSize = r.bounds.size;
                    center = r.bounds.center;
                }
                else if (r.bounds.size.IsGreaterThan(biggestSize)) 
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

                CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
                collider.center = center;
                collider.height = height;
                collider.radius = height / 4;
            }
        }
    }

    public void SetupModel()
    {
        if (limbObjects.Count < 1)
        {
            Debug.LogError("No limbs detected in the Limb Objects list. Add them if you have not already done so.");
            return;
        }

        if (body == null)
        {
            Debug.LogError("Missing the main Body object reference. Add it if you have not already done so.");
            return;
        }


        // Create an IK Manager as a child of the Body
        GameObject IKManager = new GameObject();
        IKManager.transform.parent = body.transform;
        IKManager.name = transform.name + " IK Manager";
        Rig IKManagerRig = IKManager.AddComponent<Rig>();

        // Link the new rig to the RigBuilder
        GetComponent<RigBuilder>().layers = new List<RigLayer>
        {
            new RigLayer(IKManagerRig, true)
        };

        // Empty the limbs list. It will be refilled with the limbObjects below.
        limbs.Clear();

        for (int i = 0; i < limbObjects.Count; i++)
        {
            GameObject tipTarget = new GameObject();
            tipTarget.transform.parent = transform;
            tipTarget.name = "Tip Target " + i;
            tipTarget.AddComponent<GroundAnchor>();

            GameObject boneC = new GameObject();
            boneC.transform.parent = IKManager.transform;
            boneC.name = "Bone Constraint " + i;
            TwoBoneIKConstraint TBIKC = boneC.AddComponent<TwoBoneIKConstraint>();
            ConstraintController CC = boneC.AddComponent<ConstraintController>();

            (Transform root, Transform mid, Transform tip) = GetBoneSegments(limbObjects[i]);

            // Set the TBIKC data
            TBIKC.data.root = root;
            TBIKC.data.mid = mid;
            TBIKC.data.tip = tip;
            TBIKC.data.target = boneC.transform;

            // Align the tip and its target
            tipTarget.transform.position = tip.transform.position;

            // Set the CC data
            int oppositeIndex = i % 2 == 0 ? i + 1 : i - 1; // Opposites will always be (0, 1) (2, 3), ...
            if (oppositeIndex < limbObjects.Count)
            {
                CC.opposite = limbObjects[oppositeIndex].transform;
            }

            int aheadIndex = i + 2;
            if (aheadIndex < limbObjects.Count)
            {
                CC.ahead = limbObjects[aheadIndex].transform;
            }

            int behindIndex = i - 2;
            if (behindIndex >= 0)
            {
                CC.behind = limbObjects[behindIndex].transform;
            }

            CC.target = tipTarget.transform;
            CC.transform.position = tipTarget.transform.position;

            // Add the CC to the list of limbs
            limbs.Add(CC);
        }
    }


    public void ResetModel()
    {
        // Clear the Rig references
        RigBuilder rb = GetComponent<RigBuilder>();
        DestroyImmediate(rb.layers[0].rig.gameObject);
        rb.layers = new List<RigLayer>();

        // Destroy all ground anchors
        GroundAnchor[] anchors = GetComponentsInChildren<GroundAnchor>();
        for (int i = anchors.Length - 1; i >= 0; i--)
        {
            DestroyImmediate(anchors[i].gameObject);
        }

        // Empty the limbs list
        limbs.Clear();

        body = null;
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
            GameObject root = limbs[i].TwoBoneIKConstraint.data.root?.gameObject;
            GameObject mid = limbs[i].TwoBoneIKConstraint.data.mid?.gameObject;

            if (root && root.GetComponent<Weight>() == null)
            {
                root.AddComponent<Weight>();
            }

            if (mid && mid.GetComponent<Weight>() == null)
            {
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





    // Returns the root, mid and tip bones of an IK chain gameObject.
    private (Transform, Transform, Transform) GetBoneSegments(GameObject limb)
    {
        // root 
        Transform root = limb.transform;

        Transform tip = limb.transform.GetDeepestChild();

        // Find the middle bone in the chain
        int generations = limb.transform.GetGenerationNumber();
        int midpoint = generations / 2;
        Transform mid = limb.transform.GetChildAtLevel(midpoint);

        return (root, mid, tip);
    }


    // Unused ------------------------------------------------------------------------------------

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
        Gizmos.DrawSphere(CenterOfMass, .04f);
    }
}