using System.Collections.Generic;
using UnityEngine;

namespace utility{
public class LineSmoother
{

    public static Vector3[] GenerateSmoothCurve(Vector3[] points, float segments,float step) //总点数是 segment/step
    {

        List<Vector3> smoothedPoints = new List<Vector3>();

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 p0 = (i > 0) ? points[i - 1] : points[i];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = (i + 2 < points.Length) ? points[i + 2] : p2;

            for (float j = 0; j <= segments; j=j+step)
            {
                float t = j / (float)segments;
                Vector3 interpolated = CatmullRom(p0, p1, p2, p3, t);
                smoothedPoints.Add(interpolated);
            }
        }
        smoothedPoints.Add(points[points.Length-1]);

        return smoothedPoints.ToArray();
    }

    static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f*(2*p1+(p2-p0)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t);
    }
}
}