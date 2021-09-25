/* 
 * This file is part of the Procedural-Locomotion repo on github.com/ZykeDev 
 * Marco Vincenzi - 2021
 */

using System.Collections.Generic;
using UnityEngine;

// Helper class to handle Vector3-related functions.

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
    /// Returns the product of two Vectors component-wise.
    /// </summary>
    /// <param name="multiplier"></param>
    /// <param name="multiplicand"></param>
    /// <returns></returns>
    public static Vector3 MultiplyBy(this Vector3 multiplier, Vector3 multiplicand)
    {
        return Vector3.Scale(multiplier, multiplicand);
    }


    /// <summary>
    /// Returns the vector with the minimum value at the given component from a list of vectors.
    /// </summary>
    /// <param name="vectors">List of vectors</param>
    /// <param name="axisComp">Component of interest</param>
    /// <returns></returns>
    public static Vector3 Min(this List<Vector3> vectors, Settings.Axes axisComp)
    {
        int axis = (int)axisComp;
        Vector3 min = vectors[0];

        for (int i = 0; i < vectors.Count; i++)
        {
            if (vectors[0][axis] < min[axis]) min = vectors[0];
        }

        return min;
    }


    /// <summary>
    /// Returns the average vector given a list of vectors.
    /// </summary>
    /// <param name="vectors"></param>
    /// <returns></returns>
    public static Vector3 Average(List<Vector3> vectors)
    {
        if (vectors.Count == 0) return Vector3.zero;

        float x = 0f;
        float y = 0f;
        float z = 0f;

        foreach (Vector3 pos in vectors)
        {
            x += pos.x;
            y += pos.y;
            z += pos.z;
        }

        return new Vector3(x / vectors.Count, y / vectors.Count, z / vectors.Count);
    }


    /// <summary>
    /// Returns the weighted average vector from a list of vectors and a list of corresponding weights.
    /// </summary>
    /// <param name="vectors">List of vectors</param>
    /// <param name="weights">List of weights</param>
    /// <returns></returns>
    public static Vector3 WeightedAverage(List<Vector3> vectors, List<float> weights)
    {
        if (vectors.Count != weights.Count)
        {
#if UNITY_EDITOR
            Debug.LogError("Error computing the Center of Mass. Vectors and Weights lists have different lengths.");
#endif
            return Vector3.one;
        }

        float sum = weights.Sum();
        if (sum < 0.99f || sum > 1.01f) // 1 ± 0.1
        {
#if UNITY_EDITOR
            Debug.LogError("Error computing the Center of Mass. Weights do not sum up to 1. (" + sum + ")");
#endif
            return Vector3.one;
        }

        Vector3 avg = Vector3.zero;

        for (int i = 0; i < vectors.Count; i++)
        {
            avg += vectors[i] * weights[i];
        }

        return avg;

    }


    /// <summary>
    /// Returns the total sum of a list of floats.
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static float Sum(this List<float> items)
    {
        float total = 0;
        for (int i = 0; i < items.Count; i++)
        {
            total += items[i];
        }

        return total;
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
        return vector.normalized * vector.magnitude * scale;
    }

    
    /// <summary>
    /// Returns true if all components of vector a are >= than those of vector b.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool IsGreaterThan(this Vector3 a, Vector3 b)
    {
        return (a.x >= b.x) && (a.y >= b.y) && (a.z >= b.z);
    }


    /// <summary>
    /// Rounds to Int all components of a vector.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3 RoundToInt(this Vector3 vector)
    {
        return new Vector3(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
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



    /// <summary>
    /// Returns the deepest child in a Transform's hierarchy using recursion. Only searches the topmost child.
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static Transform GetDeepestChild(this Transform parent)
    {
        if (parent.childCount == 0)
        {
            return parent;
        }

        return GetDeepestChild(parent.GetChild(0));
    }


    /// <summary>
    /// Returns the generation number of a parent transform: the number of child levels in the hierarchy. Only searches the topmost child.
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static int GetGenerationNumber(this Transform parent)
    {
        int generation = 1;

        if (parent.childCount == 0)
        {
            return generation;
        }

        return generation + GetGenerationNumber(parent.GetChild(0));
    }


    /// <summary>
    /// Returns a parent's child at the given depth level. Only searches the topmost child.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static Transform GetChildAtLevel(this Transform parent, int level)
    {
        if (parent.childCount == 0 || level == 0)
        {
            return parent;
        }

        return GetChildAtLevel(parent.GetChild(0), --level);
    }


}
