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
                break;
            }
        }
    }

    public void Rotate(Vector3 rotation)
    {
        transform.Rotate(rotation);
    }
}