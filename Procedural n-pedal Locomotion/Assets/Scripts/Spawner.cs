using Cinemachine;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject mech, crab, spider, centipede;
    private GameObject current;

    [SerializeField] private Vector3 startPos, startRot;

    [Space]
    [SerializeField]
    private CinemachineVirtualCamera CMcamera;


    public void Start()
    {
        Spawn(spider);
    }

    public void SwapMech() => Spawn(mech);
    public void SwapCrab() => Spawn(crab);
    public void SwapSpider() => Spawn(spider);
    public void SwapCentipede() => Spawn(centipede);


    private void Spawn(GameObject go)
    {
        // Deactivate current
        current?.SetActive(false);

        // Place choosen
        go.transform.position = startPos;
        go.transform.rotation = Quaternion.Euler(startRot);

        // Update Cinemachine references
        CMcamera.Follow = go.transform;
        CMcamera.LookAt = go.transform;

        // Activate choosen
        go.SetActive(true);

        current = go;
    }


}
