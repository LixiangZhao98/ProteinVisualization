using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class AFController : MonoBehaviour
{
public GameObject objAtom;
    public Transform node;
    public GameObject objStick;
    public float standardAtomScale=2f;
    public float standardStickWidth=0.4f;
    public float MinStickWidth=0.2f;


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

        void Init()
    {
     DataMemory.SceneClear(node);
     DataMemory.LoadDataBypdb(dataset.ToString());
     DataMemory.CreateAbstractionField(64);
     DataMemory.GenerateSphere(objAtom,standardAtomScale,node.GetChild(0));
     

    }
    void Start()
    {  
        Init();
    }

    void Update()
    {
        DataMemory.LoadAbstractionField(1f);
    }

}

//         for(int i=0;i<DataMemory.densityField.GetNodeNum();i++)
//      {
//   GameObject g= GameObject.CreatePrimitive(PrimitiveType.Sphere);
//   g.transform.parent=GameObject.Find("Model_").transform;
//   g.transform.position=DataMemory.densityField.GetNodedPos(i);
//   g.transform.localScale=Vector3.one*0.1f;
     
//     }