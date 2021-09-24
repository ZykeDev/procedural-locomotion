/* 
 * This file is part of the Procedural-Locomotion repo on github.com/ZykeDev 
 * Marco Vincenzi - 2021
 */

using UnityEngine;

// Simply holds the weight value of the attached Game Object

public class Weight : MonoBehaviour
{

    [Range(0, 10)] public float weight = 1f;

}
