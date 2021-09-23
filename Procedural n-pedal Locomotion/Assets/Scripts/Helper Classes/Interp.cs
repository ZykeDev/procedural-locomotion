using UnityEngine;

public class Interp
{
    private static readonly float distThreshold = 0.01f;    // Minimum target distance. If lower, the object snaps to the target.

    /// <summary>
    /// Returns the Parabolically Interpolated value for p along the given axis, scaled over the distance from "from" to "to".
    /// </summary>
    /// <param name="from">Starting coordinate</param>
    /// <param name="to">Target coordinate</param>
    /// <param name="axis">Index of the axis along which to interpolate. Clamped on the interval 0, 1, 2.</param>
    /// <param name="step">Step value between 0 and 1</param>
    /// <returns></returns>
    public static float Perp(Vector3 from, Vector3 to, int axis, float step, float stepHeight)
    {
        /* 
        Using the following parametrised parabola function: https://www.desmos.com/calculator/7ptdgoi9ri

        y = (-x^2 * dx) * m
        
        Where:
        d = |f - t|
        m = 4h / d^2

        */

        axis = Mathf.Clamp(axis, 0, 2);             // Clamp the axis index. No funny busienss.
        float dist = Vector3.Distance(from, to);    // Distance in 3D space

        // If the distance is too short, return the destination directly
        if (dist <= distThreshold)
        {
            return to[axis];
        }
                      
        float m = 4 * stepHeight / (dist * dist);   // Coord conversion factor
        float x = step * dist;                      // Scale the step over the distance
        
        float y = (-(x * x) + (dist * x)) * m;      // Parabola coord at position "step"


        return y;
    }

}