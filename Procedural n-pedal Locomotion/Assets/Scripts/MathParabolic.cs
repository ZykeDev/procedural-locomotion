using UnityEngine;

public class MathParabolic
{

    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        float f(float x) => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }

    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        float f(float x) => -4 * height * x * x + 4 * height * x;

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
    }




    public static Vector3 Parabola(Vector3 from, Vector3 to, float p)
    {
        float px = 0;// Parabola(from.x, to.x, p);
        float py = 0;//Parabola(from.y, to.y, p);
        float pz = Parabola(from.z, to.z, p);


        return new Vector3(px, py, pz);
    }

    public static Vector2 Parabola(Vector2 from, Vector2 to, float p)
    {
        float px = Parabola(from.x, to.x, p);
        float py = Parabola(from.y, to.y, p);

        return new Vector2(px, py);
    }

    public static float Parabola(float from, float to, float p)
    {
        // Using : y = -x^2 + ax, a parabola from 0 to a
        // Then we displace it by "-from", changing it to : y = -(x-f)^2 + a(x-f)

        float dist = Mathf.Abs(to - from);      // Distance between the points
        float x = from + p * dist;              // Parabola coord scaled by p, readjusted by from

        float y = -Mathf.Pow(x - from, 2) + dist * (x - from);
        y += from;
        
        Debug.Log(System.Math.Round(p * 100, 0) + "% -> " +  y);


        return y;
    }

}