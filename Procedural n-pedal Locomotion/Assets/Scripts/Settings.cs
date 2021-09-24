/* 
 * This file is part of the Procedural-Locomotion repo on github.com/ZykeDev 
 * Marco Vincenzi - 2021
 */

// Public class to store global constant values.

public static class Settings
{
    // Untraversable tag string
    public static readonly string Tag_Untraversable = "Untraversable";

    // Groun layer string
    public static readonly string Layer_Ground = "Ground";

    // Minimum target distance. If lower, the object snaps to the target.
    public static readonly float Step_Distance_Thresh = 0.01f;       

    public enum Axes { X, Y, Z }
}
