using System.Collections.Generic;
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

    void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");

        limbs = new List<ConstraintController>(GetComponentsInChildren<ConstraintController>());

        // Find local forward vector 
        
    }

    void Start()
    {
        if (useZigzagMotion)
        {
            // FR
            // FL, BL
            // BR
            // ...

            // TODO order them FL, FR, BL, BR

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

        // Force the distance vector between the body and the ground to never change
        float elevation = 0.5f;
        
        if (!IsUpdatingGait)
        {
            // Update the distance to the ground below
            if (Physics.Raycast(transform.position, -transform.up, out groundHit, Mathf.Infinity, groundMask))
            {
                // If there is a difference in height
                if (groundHit.point.y + elevation != transform.position.y)
                {
                    Vector3 newPos = new Vector3(transform.position.x, groundHit.point.y + elevation, transform.position.z);
                    transform.position = newPos;
                }
            }


            // Update the rotation to be parallel to the ground below
            if (Physics.Raycast(transform.position, -transform.up, out groundHit, Mathf.Infinity, groundMask))
            {
                // If there is a difference in rotation
                if (groundHit.normal != transform.up)
                {
                    IsUpdatingGait = true;  
                }
            }       
        }
        

        // TODO - interefers with target positioning. Need to de-rotate them? Or make them rotaion-independant?

        if (IsUpdatingGait)
        {
            fromRotation = transform.rotation;
            toRotation = Quaternion.FromToRotation(transform.up, groundHit.normal);
            transform.rotation = Quaternion.Slerp(fromRotation, toRotation, Time.deltaTime * 10);

            // Check if they are very close
            Quaternion delta = transform.rotation * Quaternion.Inverse(toRotation);
            
            if (delta.eulerAngles.magnitude <= 0.1f)
            {
                transform.rotation = toRotation;
                transform.up = groundHit.normal;
                IsUpdatingGait = false;
            }
        }
    }
}