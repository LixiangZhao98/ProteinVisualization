using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace utility{
public class Sphere
{
    static List<Vector3> verts;
    static List<int> indices;
    static int N = 10;
    static Mesh m;
public static Mesh BuildModel()
    {
        m=new Mesh();
        verts=new List<Vector3>();
        indices=new List<int>();

        for(float i = -N / 2f; i <= N / 2f; i++)
        {
            for(float j = -N / 2f; j <= N / 2f; j++)
            {
                verts.Add(new Vector3(i, j, -N / 2f));
            }
        }
        for (float i = -N / 2f; i <= N / 2f; i++)
        {
            for (float j = -N / 2f; j <= N / 2f; j++)
            {
                verts.Add(new Vector3(N / 2f, j, i));
            }
        }
        for (float i = -N / 2f; i <= N / 2f; i++)
        {
            for (float j = -N / 2f; j <= N / 2f; j++)
            {
                verts.Add(new Vector3(i, N / 2f, j));
            }
        }
        for (float i = -N / 2f; i <= N / 2f; i++)
        {
            for (float j = -N / 2f; j <= N / 2f; j++)
            {
                verts.Add(new Vector3(-N / 2f, j, i));
            }
        }
        for (float i = -N / 2f; i <= N / 2f; i++)
        {
            for (float j = -N / 2f; j <= N / 2f; j++)
            {
                verts.Add(new Vector3(i, j, N / 2f));
            }
        }
        for (float i = -N / 2f; i <= N / 2f; i++)
        {
            for (float j = -N / 2f; j <= N / 2f; j++)
            {
                verts.Add(new Vector3(i, -N / 2f, j));
            }
        }

        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] = verts[i].normalized;
        }
        
        MakePos(0);
        MakePos(1);
        MakePos(2);
        OtherMakePos(3);
        OtherMakePos(4);
        OtherMakePos(5);
  
        m.vertices = verts.ToArray();
        m.triangles = indices.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;

    }
    public static void MakePos(int num)
    {
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                int index = j * (N + 1) + (N + 1) * (N + 1)* num + i;
                int up = (j + 1) * (N + 1) + (N + 1) * (N + 1)* num + i;
                indices.AddRange(new int[] { index, index + 1, up + 1 });
                indices.AddRange(new int[] { index, up + 1, up });
            }
        }
    }
    public static void OtherMakePos(int num)
    {
        for (int i = 0; i < N + 1; i++)
        {
            for (int j = 0; j < N + 1; j++)
            {
                if (i != N && j != N)
                {
                    int index = j * (N + 1) + (N + 1) * (N + 1) * num + i;
                    int up = (j + 1) * (N + 1) + (N + 1) * (N + 1) * num + i;
                    indices.AddRange(new int[] { index, up + 1, index + 1 });
                    indices.AddRange(new int[] { index, up, up + 1 });
                }
            }
        }
    }

}


}