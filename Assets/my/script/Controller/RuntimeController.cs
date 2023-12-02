
using UnityEngine;



public class RuntimeController : MonoBehaviour
{
    public GameObject objAtom;
    public GameObject objStick;
    public GameObject objSplineMesh;
    public Transform node;
    
[SerializeField, SetProperty("DATASET")]
    private Dataset dataset;

    public Dataset DATASET
        {
            get { return dataset; }
            set {
                    dataset = value;
                if(Application.isPlaying)
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
        if(pdb!=null){
                       if(Application.isPlaying)
        Draw();
                 
        }
        }
        }

    
    public float standardAtomScale=2f;
    public float standardStickWidth=0.4f;
    public float strandWidth=0.4f;
    
    PDBLoader pdb;
 



    void Init()
    {   

        pdb=new PDBLoader();
        pdb.LoadFromPdb(dataset.ToString());
        Draw();

       

    }
    void Start()
    {  
        Init();
    }

   

   void Draw()
   {
        SceneClear();
        pdb.DrawAbstraction(abstraction,objSplineMesh,objAtom,objStick,standardAtomScale,standardStickWidth,strandWidth,node);
        

   }
      void SceneClear()
   {
        OperationOnChildren.DeleteChildren(node.GetChild(0));
        OperationOnChildren.DeleteChildren(node.GetChild(1));
        OperationOnChildren.DeleteChildren(node.GetChild(2));
   }


    void OnGUI() {
        GUIStyle sty=new GUIStyle();
        sty.normal.textColor=Color.black;
        GUI.Label(new Rect(265, 25, 200, 30), dataset.ToString()+ " Count: " + pdb.total.ToString(),sty);
    }


 
}

