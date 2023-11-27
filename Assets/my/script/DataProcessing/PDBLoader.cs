using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class PDBLoader
{


   const float HRadius=0.58f;  // 原0.78f
   const float CRadius=0.96f;  //0.86
   const float ORadius=0.76f;  //0.66
   const float NRadius=0.80f;
   const float SRadius=1.24f;   //1.04

    // const float HRadius=0.78f;  
    // const float CRadius=0.86f;  
    // const float ORadius=0.66f;  
    // const float NRadius=0.80f;
    // const float SRadius=1.04f;  

    int countN = 0;
    int countH = 0;
    int countO = 0;
    int countC = 0;
    int countS=0;
    int others=0;
   public int total=0;
    
   public  List<AminoAcid> lminoAcids;
   
   public List<Strand> lStrands;


   public  PDBLoader()
    {
     countN = 0;
     countH = 0;
     countO = 0;
     countC = 0;
     countS=0;
     others=0;
     total=0;
     lminoAcids = new List<AminoAcid>();
     lStrands=new List<Strand>();
    }
         
   public void LoadFromPdb(string s)
    {
        string m_Path = Application.dataPath+"/my/Data/SARS-CoV-2 20-09";
        
        StreamReader sr = new StreamReader(m_Path + "/" + s+".pdb");
        string str;
        int strandID=0;
        Strand strand=new Strand(strandID,ColorDefinition.strandsColorPool[strandID%ColorDefinition.strandsColorPool.Length]);
        AminoAcid aminoAcid=new AminoAcid(0);




        while (true)
        {

            
            str = sr.ReadLine();



            if (str == null)
            {
                break;
            }

                 if(str.Trim().Substring(0,3)=="TER")
         {  
            strand.AddAminoAcids(aminoAcid); 
           
            
            lStrands.Add(strand);
            strandID++;
            strand=new Strand(strandID,ColorDefinition.strandsColorPool[strandID%ColorDefinition.strandsColorPool.Length]);
           
         }


            if (str.StartsWith("ATOM"))
            {
           
                         if(aminoAcid.GetAminoAcidID()!=int.Parse(str.Substring(23, 4).Trim()))
                         {
                            if(aminoAcid.GetAtomsNum()!=0)
                            {strand.AddAminoAcids(aminoAcid);}
 
                           
                             aminoAcid=new AminoAcid(int.Parse(str.Substring(23, 4).Trim()));
                         }
                       
                Atom atom = new Atom();
                atom.atomID = int.Parse(str.Substring(7, 5).Trim());
                atom.fullElementName = str.Substring(12,4).Trim();
                atom.elementName = atom.fullElementName.Substring(0,1);
                atom.AtomID= str.Substring(14,2).Trim(); 
                atom.aminoAcidName = str.Substring(17, 3).Trim();
                atom.aminoAcidsID = int.Parse(str.Substring(23, 4).Trim());
                atom.pos = new Vector3(float.Parse(str.Substring(31, 8).Trim()), float.Parse(str.Substring(39, 8).Trim()), float.Parse(str.Substring(47, 8).Trim()));
                atom.bFactor = float.Parse(str.Substring(61, 5).Trim());

                

            if (atom.elementName == "C")
            {
               atom.Color=ColorDefinition.CColor;
               atom.Radius=CRadius;
               countC++;
            }
            else if (atom.elementName == "N")
            {
                atom.Color=ColorDefinition.NColor;
                atom.Radius=NRadius;
                countN++;
            }
            else if (atom.elementName == "O")
            {
                atom.Color=ColorDefinition.OColor;
                atom.Radius=ORadius;
                countO++;
            }
            else if (atom.elementName == "H")
            {
                atom.Color=ColorDefinition.HColor;
                atom.Radius=HRadius;
                countH++;
            }
            else if (atom.elementName == "S")
            {
                atom.Color=ColorDefinition.SColor;
                atom.Radius=SRadius;
                countS++;
            }
            
            
            if(atom.fullElementName=="N"||atom.fullElementName=="CA"||atom.fullElementName=="C")
            {
                strand.AddAtomToBackBone(atom);
                if(atom.fullElementName=="N")
                aminoAcid.SetNAtom(atom);
                if(atom.fullElementName=="C")
                aminoAcid.SetCAtom(atom);
            }
            
            aminoAcid.AddAtom(atom);
            
            
      }

        
        }
  
            sr.Close();
 


        AppendBond();
        HowManyAtoms();
    }
   
void AppendBond()
    {
        for(int s=0;s<lStrands.Count;s++)
        {
            lStrands[s].AppendBondInsideRes();
            lStrands[s].AppendBondBetweenRes();
        }
    }
void HowManyAtoms()
    {

        // foreach(var s in lStrands)
        // {
        //     s.HowManyAtoms(ref countN,ref countC,ref countO,ref countH,ref countS);
        // }
        total=countC+countH+countN+countO+countS+others;
        Debug.Log("N ATOMS = " + countN);
        Debug.Log("C ATOMS = " + countC);
        Debug.Log("O ATOMS = " + countO);
        Debug.Log("H ATOMS = " + countH);
        Debug.Log("S ATOMS = " + countS);
        Debug.Log("Other ATOMS = " + others);
        Debug.Log("Total ATOMS = " + total);
    }

public void ShiftNode(Transform _t)
{
   _t.position-=GetCenter();
}
Vector3 GetCenter()
{
    Vector3 v=Vector3.zero;
    foreach(var strand in lStrands)
    {
        v=v+strand.GetCenter();
    }
    return v/lStrands.Count;
}


#region  rendering

public void DrawAbstraction(Abstraction  _abstraction,GameObject _objAtom,GameObject _objStick,float _standardAtomScale, float _standardStickWidth,float _strandWidth,Transform _father)
    {
     switch (_abstraction)
     {
       case Abstraction.SpaceFilling:
      
         GenerateSphere(_objAtom,2f,_father);
       break;



       case Abstraction.BallAndStick:
        
         GenerateSphere(_objAtom,1f,_father);
         GenerateShortStick(_objStick,_standardStickWidth,_father);
       break;
       case Abstraction.Stick:
         GenerateLongStick(_objStick,_standardStickWidth,_father);
                break;
       case Abstraction.BackBone:
         GenerateBackBone(_objStick,_standardStickWidth,_father);
                break;
       case Abstraction.Ribbon:
         GenerateStrand(_strandWidth,_father);
                break;
        default:break;
     }
      
    }
public void GenerateSphere(GameObject _g,float _standardAtomScale,Transform _father)
{
    foreach(var s in lStrands)
    {
       s.GenerateSphere(_g,_standardAtomScale,_father);
    }
}

 public void GenerateShortStick(GameObject _g,float _standardStickWidth,Transform _father)
 {
            foreach(var s in lStrands)
    {
       s.GenerateShortStick(_g,_standardStickWidth,_father);
    }
 }
public void GenerateLongStick(GameObject _g,float _standardStickWidth,Transform _father)
 {
            foreach(var s in lStrands)
    {
       s.GenerateLongStick(_g,_standardStickWidth,_father);
    }
 }
public void GenerateBackBone(GameObject _g,float _standardStickWidth,Transform _father)
 {
             for (int i = 0; i < lStrands.Count; i++)
        {
            for(int j=0;j<lStrands[i].GetBackBoneNum()-1;j++)
            {
                Vector3 A=lStrands[i].GetBackBone(j).pos;
                Vector3 B=lStrands[i].GetBackBone(j+1).pos;
                Vector3 dir=(A-B).normalized; //A-B
                Vector3 start=A;
                Vector3 end=B;
                GameObject g= GameObject.Instantiate(_g,(start+end)/2,Quaternion.identity);
                
                g.transform.localScale=new Vector3(_standardStickWidth,(start-end).magnitude/2,_standardStickWidth); 
                g.transform.up=dir;
                g.GetComponent<Renderer>().material.SetColor("_Color1",lStrands[i].GetBackBone(j).Color);
                g.transform.GetChild(0).GetComponent<Renderer>().material.color=lStrands[i].GetBackBone(j).Color;
                g.GetComponent<Renderer>().material.SetColor("_Color2",lStrands[i].GetBackBone(j+1).Color);
                g.transform.GetChild(1).GetComponent<Renderer>().material.color=lStrands[i].GetBackBone(j+1).Color;
                g.transform.parent=_father.GetChild(1);

            }
                
            }
 }

 public void GenerateStrand(float _strandWidth,Transform _father)
 {
    int maxPoints=4000;
     for (int i = 0; i <lStrands.Count; i++)
        {
            Vector3[] atomPos=new Vector3[lStrands[i].GetBackBoneNum()];
            for(int j=0;j<lStrands[i].GetBackBoneNum();j++)
            {
                atomPos[j]=lStrands[i].GetBackBone(j).pos;
            }

            Vector3[] SmoothingPos= LineSmoother.GenerateSmoothCurve(atomPos);
            for(int k=0;k<=SmoothingPos.Length/maxPoints;k++)
            {
            GameObject g=new GameObject("strand");
            g.AddComponent<TubeRenderer>();

            if(k==SmoothingPos.Length/maxPoints)
            g.GetComponent<TubeRenderer>().points=SmoothingPos.ToList().GetRange(k*maxPoints,SmoothingPos.Length-k*maxPoints).ToArray();
            else
            g.GetComponent<TubeRenderer>().points=SmoothingPos.ToList().GetRange(k*maxPoints,maxPoints).ToArray();

            g.GetComponent<MeshRenderer>().material=new Material(Shader.Find("HDRP/LitTessellation"));
            g.GetComponent<MeshRenderer>().material.SetFloat("_Metallic",1f);
            g.GetComponent<MeshRenderer>().material.SetFloat("_Smoothness",0.256f);
            g.GetComponent<MeshRenderer>().material.color=lStrands[i].GetColor();
            g.transform.parent=_father.GetChild(2);
            }
            
        }
 }
#endregion


}
