using UnityEditor;
using UnityEngine;

public class OperationOnChildren
{
public static void DeleteChildren(Transform t)
{
 for (var i = t.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                GameObject.Destroy(t.GetChild(i).gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(t.GetChild(i).gameObject);
            }
#else
            Destroy(t.GetChild(i).gameObject);
#endif
        }
}
}
