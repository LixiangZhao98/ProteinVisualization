
using System.Collections.Generic;
using UnityEngine;


public class DataMemory : MonoBehaviour
{


    #region ProteinInfo
    [SerializeField]
    [HideInInspector] public List<Vector3> particleflow_dest;
    [SerializeField]
    static  PDBLoader pdb=new PDBLoader();

    public static void LoadDataBypdb(string loadFileName)
    {

        pdb=new PDBLoader();
        pdb.LoadFromPdb(loadFileName);

   }

    public static void GenerateSphere(GameObject _g,float _standardAtomScale,Transform _father)
    {
        pdb.GenerateSphere(_g,_standardAtomScale,_father);


   }
       public static void GenerateStick(GameObject _g,float _standardAtomScale,Transform _father)
    {
        pdb.GenerateLongStick(_g,_standardAtomScale,_father);


   }
       public static void GenerateStrand(GameObject _g,float _standardAtomScale,Transform _father,float _segment,float _step)
    {
        pdb.GenerateStrandTube(_g,_standardAtomScale,_father, _segment, _step);


   }
// static  void Draw()
// {
//         SceneClear();
//         pdb.GenerateSphere(objAtom,standardAtomScale,node.GetChild(0));
//         pdb.GenerateLongStick(objStick,standardStickWidth,node.GetChild(1));
//         pdb.GenerateStrandTube(objStick,MinStickWidth,node.GetChild(2),10,1f);
// }
      public  static  void SceneClear(Transform father)
   {
        OperationOnChildren.DeleteChildren(father.GetChild(0));
        OperationOnChildren.DeleteChildren(father.GetChild(1));
        OperationOnChildren.DeleteChildren(father.GetChild(2));
   }
   
    public static void ClearProteinMemory()
    {
        pdb = new PDBLoader();
    }

    
    #endregion

    #region DensityFieldInfo
    [SerializeField]
  
    static public  DensityField densityField = new DensityField();


    static public void CreateAbstractionField(int gridNum)
    {

        float xstep = (pdb.maxPos.x - pdb.minPos.x) / gridNum;
        float ystep = (pdb.maxPos.y - pdb.minPos.y) / gridNum;
        float zstep = (pdb.maxPos.z - pdb.minPos.z) / gridNum;
        pdb.maxPos+=new Vector3(xstep,ystep,zstep);
        pdb.minPos-=new Vector3(xstep,ystep,zstep);       
        densityField.InitializeDensityFieldByGapDis(pdb.name,pdb.minPos.x, pdb.maxPos.x, gridNum,pdb.minPos.y, pdb.maxPos.y, gridNum,pdb.minPos.z, pdb.maxPos.z, gridNum);
        Debug.Log("Create density field success");

    }

    static public void LoadAbstractionField(float value)
    {

        for(int i=0;i<densityField.GetNodeNum();i++)
        {
            densityField.SetNodeDensity(i,value);
        }
        Debug.Log("Load density field success");

    }
    public static void ClearDensityMemory()
    {
        densityField = new DensityField(); 
    }
    #endregion

    

}
