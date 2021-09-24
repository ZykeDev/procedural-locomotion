/* 
 * This file is part of the Procedural-Locomotion repo on github.com/ZykeDev 
 * Marco Vincenzi - 2021
 */

using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

// Class to swap the currently selected model at run-time.

public class Spawner : MonoBehaviour
{
    [SerializeField, Tooltip("Prefab.")]
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


    public void Quit() => Application.Quit();   
    

    public void SwapMech() => Spawn(mech);
    public void SwapCrab() => Spawn(crab);
    public void SwapSpider() => Spawn(spider);
    public void SwapCentipede() => Spawn(centipede);


    private void Spawn(GameObject go)
    {
        Destroy(current);

        go = Instantiate(go);

        // Place it
        go.transform.position = startPos;
        go.transform.rotation = Quaternion.Euler(startRot);

        // Update Cinemachine references
        CMcamera.Follow = go.transform;
        CMcamera.LookAt = go.transform;

        current = go;
    }


}
