using UnityEngine;

public class Joint : MonoBehaviour
{
    public Joint child;


    private void Awake()
    {
        foreach (Transform transformChild in transform)
        {
            Joint jointChild = transformChild.GetComponent<Joint>();

            if (jointChild != null)
            {
                child = jointChild;
            }
        }
    }

    public void Rotate(float angle)
    {
        transform.Rotate(Vector3.up * angle);
    }
}
