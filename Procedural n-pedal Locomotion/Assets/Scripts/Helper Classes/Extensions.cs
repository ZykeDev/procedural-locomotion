using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// Returns the reciprocal vector 1/v.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3 Reciprocal(this Vector3 vector)
    {
        return new Vector3(1 / vector.x, 1 / vector.y, 1 / vector.z);
    }


    /// <summary>
    /// Divides the vector by another vector.
    /// </summary>
    /// <param name="dividend"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static Vector3 DivideBy(this Vector3 dividend, Vector3 divisor)
    {
        return Vector3.Scale(dividend, Reciprocal(divisor));
    }


    /// <summary>
    /// Scales the applied vector from a to b by a factor of scale.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Vector3 RescaleApplied(this Vector3 a, Vector3 b, float scale)
    {
        // (((b-a) / |b-a|) * r) + a
        return ((b - a).normalized * scale) + a;
    }


    /// <summary>
    /// Rescales a vector's length by a factor of scale from its origin point.
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Vector3 Rescale(this Vector3 vector, float scale)
    {
        return vector.normalized * (vector.magnitude * scale);
    }

    
    /// <summary>
    /// Returns true if all components of vector a are >= than those of vector b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool IsBiggerThan(this Vector3 a, Vector3 b)
    {
        return (a.x >= b.x) && (a.y >= b.y) && (a.z >= b.z);
    }




    /// <summary>
    /// Converts an angle into a Vector 2 describing its position on a unit circle.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Vector2 ToUnitCirclePoint(this float angle)
    {
        float x = Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = Mathf.Sin(angle * Mathf.Deg2Rad);

        x = (float)System.Math.Round(x, 2);
        y = (float)System.Math.Round(y, 2);

        return new Vector2(x, y);
    }


 
}