using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// Returns the reciprocal vector 1/v
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3 Reciprocal(this Vector3 vector)
    {
        Vector3 reciprocal = new Vector3(1 / vector.x, 1 / vector.y, 1 / vector.z);

        return reciprocal;
    }


    /// <summary>
    /// Divides the vector by another vector
    /// </summary>
    /// <param name="dividend"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static Vector3 DivideBy(this Vector3 dividend, Vector3 divisor)
    {
        Vector3 quotient = Vector3.Scale(dividend, Reciprocal(divisor));

        return quotient;
    }
    
}
