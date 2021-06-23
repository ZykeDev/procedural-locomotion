using UnityEngine;

public class IKManager : MonoBehaviour
{
    [SerializeField, Range(0, 0.1f)] private float distanceThreshold = 0.05f;
    [SerializeField, Range(1f, 10f)] private float convergenceFactor = 5;
    [SerializeField, Range(1, 20)] private int convergenceSteps = 10;

    public Joint root, end;
    public GameObject target, pole;


    void Update()
    {
        for (int i = 0; i < convergenceSteps; i++)
        {
            // Only rotate if the distance is above the threshold
            if (GetDistance(end.transform.position, target.transform.position) > distanceThreshold)
            {
                // Rotate every joint, starting from the root
                Joint currentJoint = root;
                while (currentJoint != null)
                {
                    float slope = ComputeSlope(currentJoint);
                    //currentJoint.Rotate(-slope * convergenceFactor); // Negative slope
                    currentJoint = currentJoint.child; // Iterate over children
                }
            }
        }
    }


    /// <summary>
    /// Returns the slope of the line connecting the end joint to the target
    /// </summary>
    /// <param name="joint"></param>
    /// <returns></returns>
    private float ComputeSlope(Joint joint)
    {
        // Gradient descent implementation
        // Rotates the joint by a small amount, then checks the gradient difference between x0 and x1
        // This LITERALLY rotates the transform. There is probably another math-only way that doesn't

        float deltaTheta = 0.01f;
        Vector3 deltaAngle = Vector3.forward * deltaTheta;
        float x0 = Vector3.Distance(end.transform.position, target.transform.position);

        joint.Rotate(deltaAngle);

        float x1 = Vector3.Distance(end.transform.position, target.transform.position);

        joint.Rotate(-deltaAngle);

        return (x1 - x0) / deltaTheta;
    }

    private float GetDistance(Vector3 v1, Vector3 v2)
    {
        return Vector3.Distance(v1, v2);
    }




}
