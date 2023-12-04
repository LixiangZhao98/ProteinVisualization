
using UnityEngine;


public class animation : MonoBehaviour
{
    public GameObject objAtom;
    public Transform node;
    public GameObject objStick;
    public float standardAtomScale=2f;
    public float standardStickWidth=0.4f;
    public float MinStickWidth=0.2f;
    PDBLoader pdb;
    private float thre;
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

        pdb=new PDBLoader();
        pdb.LoadFromPdb(dataset.ToString());
        SceneClear();
        pdb.GenerateSphere(objAtom,standardAtomScale,node.GetChild(0));
        pdb.GenerateLongStick(objStick,standardStickWidth,node.GetChild(1));
        pdb.GenerateStrandTube(objStick,MinStickWidth,node.GetChild(2),10,1f);
        

    }
    void Start()
    {  
        Init();
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
float left;
float right;
public void SliderChange(float _value)
{

//-----------------------------------------------------------
left=0f;
right=0.25f;
if(_value>=left&&_value<right)
{
 for(int i=0;i<node.GetChild(0).childCount;i++)  
 {
  float r_max= node.GetChild(0).GetChild(i).GetComponent<Atom>().Radius*standardAtomScale;
  float ratio=Interpolation.LinearInterp(_value,left,right,1f,0.5f); //atom x:0-0.25  y:1-0.5
   node.GetChild(0).GetChild(i).localScale=r_max*ratio*Vector3.one;
 }

 if(thre<=PDBLoader.maxRank)
 {
    thre=PDBLoader.maxRank;
    for(int i=0;i<node.GetChild(1).childCount;i++)  
 {
  
   node.GetChild(1).GetChild(i).GetComponent<MeshRenderer>().material.SetFloat("_Cutoff",thre);
 }
 }
}

//-----------------------------------------------------------
left=0.25f;
right=0.5f;

if(_value>=left&&_value<right)
{

for(int i=0;i<node.GetChild(0).childCount;i++)  
 {
  float r_max= node.GetChild(0).GetChild(i).GetComponent<Atom>().Radius*standardAtomScale;
  float ratio=Interpolation.LinearInterp(_value,left,right,0.5f,0f); //atom x:0.25-0.5  y:0.5-0
   node.GetChild(0).GetChild(i).localScale=r_max*ratio*Vector3.one;
 }

 for(int i=0;i<node.GetChild(1).childCount;i++)  
 {
  float scaleY= node.GetChild(1).GetChild(i).localScale.y;
  float scalexz=Interpolation.LinearInterp(_value,0.25f,0.5f,standardStickWidth,MinStickWidth); //stick thickness x:0.25-0.5    y:standardStickWidth-MinStickWidth
   node.GetChild(1).GetChild(i).localScale=new Vector3(scalexz,scaleY,scalexz);
 }

 if(thre<=PDBLoader.maxRank)
 {
    thre=PDBLoader.maxRank;
    for(int i=0;i<node.GetChild(1).childCount;i++)  
 {
  
   node.GetChild(1).GetChild(i).GetComponent<MeshRenderer>().material.SetFloat("_Cutoff",thre);
 }
 }
}



//-----------------------------------------------------------

left=0.5f;
right=0.75f;

if(_value>=left&&_value<right)
{

 thre=Interpolation.LinearInterp(_value,0.5f,0.75f,PDBLoader.maxRank,0f); //stick display x:0.5-0.75    thre:7-0
 for(int i=0;i<node.GetChild(1).childCount;i++)  
 {
  
   node.GetChild(1).GetChild(i).GetComponent<MeshRenderer>().material.SetFloat("_Cutoff",thre);
 }
}


//-----------------------------------------------------------

left=0.75f;
right=1f;

if(_value>=left&&_value<right)
{


 if(thre!=0f)
 {thre=0f;
    for(int i=0;i<node.GetChild(1).childCount;i++)  
 {
  node.GetChild(1).GetChild(i).GetComponent<MeshRenderer>().material.SetFloat("_Cutoff",thre);
 }
 }


}



//-----------------------------------------------------------

left=0.8f;
right=1f;

if(_value>=left&&_value<right)
{


 if(thre!=0f)
 {thre=0f;
    for(int i=0;i<node.GetChild(1).childCount;i++)  
 {
  node.GetChild(1).GetChild(i).GetComponent<MeshRenderer>().material.SetFloat("_Cutoff",thre);
 }
 }


 OperationOnChildren.DeleteChildren(node.GetChild(2));
  float step=Interpolation.LinearInterp(_value,0.75f,1f,1f,0.01f); //tube display x:0.5-0.75    thre:1-0.01
 pdb.GenerateStrandTube(objStick,MinStickWidth/2,node.GetChild(2),1,step);
 
}






//---------------------------------------------------------------------
if(_value>=0.5f)                                //atom close/open
{
  if(node.GetChild(0).gameObject.active)
  {
    node.GetChild(0).gameObject.SetActive(false);
  }
}

else
{
      if(!node.GetChild(0).gameObject.active)
  {
    node.GetChild(0).gameObject.SetActive(true);
  }

}




if(_value>=0.8f)                                //stick close/open
{
  if(node.GetChild(1).gameObject.active)
  {
    node.GetChild(1).gameObject.SetActive(false);
  }
}

else
{
      if(!node.GetChild(1).gameObject.active)
  {
    node.GetChild(1).gameObject.SetActive(true);
  }

}


if(_value<=0.8f)                                //tube close/open
{
  if(node.GetChild(2).gameObject.active)
  {
    node.GetChild(2).gameObject.SetActive(false);
  }
}

else
{
      if(!node.GetChild(2).gameObject.active)
  {
    node.GetChild(2).gameObject.SetActive(true);
  }

}

}
}