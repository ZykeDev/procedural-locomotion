using UnityEngine;

public class Interp
{
    private static float minThreshold = 0.01f;

    /// <summary>
    /// Returns the Parabolically Interpolated value for p along the given axis, scaled over the distance from "from" to "to".
    /// </summary>
    /// <param name="from">Starting coordinate</param>
    /// <param name="to">Target coordinate</param>
    /// <param name="axis">Index of the axis along which to interpolate. Clamped on the interval 0, 1, 2.</param>
    /// <param name="p">Step value between 0 and 1</param>
    /// <returns></returns>
    public static float Perp(Vector3 from, Vector3 to, int axis, float p)
    {
        /* 
        Using the following parametrised parabola function: https://www.desmos.com/calculator/7ptdgoi9ri

        y = (-s^2 * ds) * m
        s = x - f
        d = |f - t|
        m = 4h/d^2

        */

        axis = Mathf.Clamp(axis, 0, 2);             // Clamp the axis index. No funny busienss
        float dist = Vector3.Distance(from, to);    // Distance in 3D space
        //dist = Mathf.Abs(from[axis] - to[axis]);

        // If the distance is too short, return the destination directly
        if (dist <= minThreshold)
        {
            return to[axis];
        }

        float height = 0.5f;                        // Parametrical height of the peak
        float m = 4 * height / (dist * dist);       // Peak coord conversion factor

        float x = p * dist;                         // Scale the p over the distance
        float s = x - from[axis];                   // Translate along the given axis

        float y = (-(s * s) + (dist * s)) * m;      // Parabola coord at position p

        return y;
    }

}