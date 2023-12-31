
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
       public static void GenerateStick(GameObject _g,float _standardAtomScale,Transform _father,Material m)
    {

        pdb.GenerateLongStick(_g,_standardAtomScale,_father, m);


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
   
    #endregion

    #region DensityFieldInfo
    [SerializeField]
  
    static public  ScalarField densityField = new ScalarField();


    static public void CreateAbstractionField(int gridNum)
    {

        float xstep = (pdb.maxPos.x - pdb.minPos.x) / gridNum;
        float ystep = (pdb.maxPos.y - pdb.minPos.y) / gridNum;
        float zstep = (pdb.maxPos.z - pdb.minPos.z) / gridNum;
        pdb.maxPos+=new Vector3(xstep,ystep,zstep);
        pdb.minPos-=new Vector3(xstep,ystep,zstep);       
        densityField.InitializeFieldByGapDis(pdb.name,pdb.minPos.x, pdb.maxPos.x, gridNum,pdb.minPos.y, pdb.maxPos.y, gridNum,pdb.minPos.z, pdb.maxPos.z, gridNum);
 

    }
    static public void LoadAbstractionField(float value)
    {
        densityField.LoadFieldValue(value);
    }

    static public void LoadAbstractionField(float[] values)
    {
        densityField.LoadFieldValue(values);
    }

    static public Texture3D GetTexture3D()
    {
       return densityField.GetTexture3D();
    }
    #endregion

    

}
