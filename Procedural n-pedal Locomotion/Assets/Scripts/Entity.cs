using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private List<ConstraintController> limbs;

    [SerializeField] private bool useZigzagMotion = true;
    private float zigzagDifference = 1f;



    void Awake()
    {
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



    void Update()
    {
        // Rotate body based on weigthed limb vectors
        

    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.localPosition, transform.forward);
    }

}
