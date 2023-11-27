
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;



public class RuntimeController : MonoBehaviour
{
    public GameObject objAtom;
    public GameObject objStick;
    public Transform node;
    
[SerializeField, SetProperty("DATASET")]
    private Dataset dataset;

    public Dataset DATASET
        {
            get { return dataset; }
            set {
                    dataset = value;
                 Init(); 
            }
        }

[SerializeField, SetProperty("ABSTRACTION")]
    private Abstraction abstraction;

    public Abstraction ABSTRACTION
        {
            get { return abstraction; }
            set {
              
                    abstraction = value;
                Init();  
            }
        }

    
    public float standardAtomScale=2f;
    public float standardStickWidth=0.4f;
    public float strandWidth=0.4f;
    
    PDBLoader pdb;
 


   
   
void DrawBond(List<Atom> LAtoms) //Draw bonds between all atoms
    {
        for (int i = 0; i < LAtoms.Count; i++)
        {
            for (int j = 0; j < LAtoms[i].singleDirectionBonds.Count; j++)
            {
                Debug.DrawLine(LAtoms[i].obj.transform.position, LAtoms[i].singleDirectionBonds[j].obj.transform.position, Color.red,0.01f,false);
            }
        }
    }
   
    
    



    void Init()
    {   
        DeleteChildren(node.GetChild(0));
        DeleteChildren(node.GetChild(1));
        DeleteChildren(node.GetChild(2));
        pdb=new PDBLoader();
        pdb.LoadFromPdb(dataset.ToString());
        pdb.DrawAbstraction(abstraction,objAtom,objStick,standardAtomScale,standardStickWidth,strandWidth,node);
        pdb.ShiftNode(node);

    }
    void Start()
    {  
        Init();
    }

   
 public  void GenerateRibbon(List<Transform> ts, float radius, float thickness)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (Transform t in ts)
        {
            Vector3 normal = Vector3.Cross(t.forward, t.up);  
            Vector3 right = (normal + t.up).normalized * radius;
            Vector3 left = -(normal + t.up).normalized * radius;

            Vector3 topRight = t.position + right + t.up * thickness / 2;
            Vector3 topLeft = t.position + left + t.up * thickness / 2; 
            Vector3 botRight = t.position + t.forward + right - t.up * thickness / 2;
            Vector3 botLeft = t.position + t.forward + left - t.up * thickness / 2;

            vertices.Add(topRight);
            vertices.Add(topLeft);
            vertices.Add(botRight);
            vertices.Add(botLeft);

            int index = vertices.Count - 4;
            triangles.Add(index + 0);
            triangles.Add(index + 2);
            triangles.Add(index + 1);

            triangles.Add(index + 1); 
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

    }
    
void DeleteChildren(Transform t)
{
 for (var i = t.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Destroy(t.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(t.GetChild(i).gameObject);
            }
#else
            Destroy(t.GetChild(i).gameObject);
#endif
        }
}
    void OnGUI() {
        GUIStyle sty=new GUIStyle();
        sty.normal.textColor=Color.black;
        GUI.Label(new Rect(265, 25, 200, 30), dataset.ToString()+ " Count: " + pdb.total.ToString(),sty);
    }


 
}

