using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SplineMesh;
using UnityEngine;
using utility;


public class PDBLoader
{
 public string name;
 public Vector3 maxPos;
 public Vector3 minPos;
static public int maxRank;
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

   public Texture3D colTexture3D;


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
        this.name=s;
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
            aminoAcid=new AminoAcid(0);
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

                if(atom.fullElementName.Length==1)
                atom.AtomIndex=" ";
                else
                atom.AtomIndex=atom.fullElementName.Substring(1,1); 
                
                atom.aminoAcidName = str.Substring(17, 3).Trim();
                atom.aminoAcidsID = int.Parse(str.Substring(23, 4).Trim());
                atom.pos = new Vector3(float.Parse(str.Substring(31, 8).Trim()), float.Parse(str.Substring(39, 8).Trim()), float.Parse(str.Substring(47, 8).Trim()));

                if(atom)
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
            else
            {
                atom.Color=ColorDefinition.otherColor;
                atom.Radius=SRadius;
                others++;
            }
            
            
            if(atom.fullElementName=="N"||atom.fullElementName=="CA"||atom.fullElementName=="C")
            {
                strand.AddAtomToBackBone(atom);
                atom.AtomRank=0;
                if(atom.fullElementName=="N")
                aminoAcid.SetNAtom(atom);
                if(atom.fullElementName=="C")
                aminoAcid.SetCAtom(atom);
            }
            else
            {atom.AtomRank=-1;}

            
            aminoAcid.AddAtom(atom);  
      }
        }

        sr.Close();

        DataProcessing();

    }

void CalAtomRank()
{
foreach(var strand_ in lStrands)
        {
        Stack sAtoms=new Stack();
        sAtoms.Push(strand_.GetBackBone(0));
        while(sAtoms.Count!=0)
        {
            Atom atom=(Atom)sAtoms.Pop();
            int rank=atom.AtomRank;
            foreach(var b in atom.bonds)
            {
                if(!b.visited)
                {
              sAtoms.Push(b);
              b.visited=true;
              if(b.AtomRank==-1)
              {
                b.AtomRank=rank+1;
                 if(maxRank<b.AtomRank)
                  {
                  maxRank=b.AtomRank;
                 }
              }
                }
              
            }
        
        }
    }
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

        total=countC+countH+countN+countO+countS+others;
        Debug.Log("N ATOMS = " + countN);
        Debug.Log("C ATOMS = " + countC);
        Debug.Log("O ATOMS = " + countO);
        Debug.Log("H ATOMS = " + countH);
        Debug.Log("S ATOMS = " + countS);
        Debug.Log("Other ATOMS = " + others);
        Debug.Log("Total ATOMS = " + total);
        Debug.Log("Strand Count = " + lStrands.Count);
        foreach(var s in lStrands)
        {
            Debug.Log("backbone Count = " + s.GetBackBoneNum());
        }        
    }
void DataShift()
{
 foreach(var s in lStrands)  //shift data to the center
    {
        s.shiftVec3(-GetCenter());
    }
}

public List<Atom> GetAtoms()
{
           List<Atom> lAtom=new List<Atom>();
        for (int i = 0; i < lStrands.Count; i++)  //Find the max and min   
        {
             lAtom.AddRange(lStrands[i].GetAtoms());
        }
        return lAtom;
}
void CalMaxMinPos()
{
 Vector3  vAve = GetCenter();
        float xmin,ymin,zmin,xmax,ymax,zmax;
        xmin = vAve.x;
        xmax = vAve.x;
        ymin = vAve.y;
        ymax = vAve.y;
        zmin = vAve.z;
        zmax = vAve.z;
     

         List<Atom> lAtom=GetAtoms();
               for (int i = 0; i < lAtom.Count; i++)    
        {
            if (xmin > lAtom[i].pos.x)
                xmin = lAtom[i].pos.x;
            if (xmax < lAtom[i].pos.x)
                xmax = lAtom[i].pos.x;
            if (ymin > lAtom[i].pos.y)
                ymin = lAtom[i].pos.y;
            if (ymax < lAtom[i].pos.y)
                ymax = lAtom[i].pos.y;
            if (zmin > lAtom[i].pos.z)
                zmin = lAtom[i].pos.z;
            if (zmax < lAtom[i].pos.z)
                zmax = lAtom[i].pos.z;
        }     
        minPos=new Vector3(xmin,ymin,zmin);
        maxPos=new Vector3(xmax,ymax,zmax);
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


public void DataProcessing()
{
        AppendBond();
        HowManyAtoms();
        CalAtomRank();
        DataShift();
        CalMaxMinPos();                
}



#region  rendering

public void DrawAbstraction(Abstraction  _abstraction,GameObject _objSplineMesh,GameObject _objAtom,GameObject _objStick,float _standardAtomScale, float _standardStickWidth,float _strandWidth,Transform _father,Material m)
    {
     switch (_abstraction)
     {
       case Abstraction.SpaceFilling:
         GenerateSphere(_objAtom,2f,_father.GetChild(0));
       break;



       case Abstraction.BallAndStick:
        
         GenerateSphere(_objAtom,1f,_father.GetChild(0));
         GenerateShortStick(_objStick,_standardStickWidth,_father.GetChild(1));
       break;
       case Abstraction.Stick:
         GenerateLongStick(_objStick,_standardStickWidth,_father.GetChild(1),m);
                break;
       case Abstraction.BackBone:
         GenerateBackBone(_objStick,_standardStickWidth,_father.GetChild(1));
                break;
       case Abstraction.Ribbon:
         GenerateStrandTube(_objSplineMesh,_strandWidth,_father.GetChild(2),1,0.1f);
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
public void GenerateSphereCombined(ref List<Transform> ts,float _standardAtomScale)
{
    foreach(var s in lStrands)
    {
       s.GenerateSphereCombined(ref ts,_standardAtomScale);
    }
}
 public void GenerateShortStick(GameObject _g,float _standardStickWidth,Transform _father)
 {
            foreach(var s in lStrands)
    {
       s.GenerateShortStick(_g,_standardStickWidth,_father);
    }
 }
public void GenerateLongStick(GameObject _g,float _standardStickWidth,Transform _father,Material m)
 {
    List<Color>Color1=new List<Color>();
    List<Color>Color2=new List<Color>();
    List<int>Rank1=new List<int>();
    List<int>Rank2=new List<int>();
    int  stickIndex=0;
            foreach(var s in lStrands)
    {
       s.GenerateLongStick(_g,_standardStickWidth,_father, m,Color1,Color2,Rank1,Rank2,ref stickIndex);
    }
    LoadRCBuffer(ref m,Color1,Color2,Rank1,Rank2);
 }
public void GenerateBackBone(GameObject _g,float _standardStickWidth,Transform _father)
 {
                foreach(var s in lStrands)
    {
       s.GenerateBackBone(_g,_standardStickWidth,_father);
    }


 }

 public void GenerateStrandTube(GameObject _g,float _strandWidth,Transform _father,float _segment,float _step)
 {
    int maxPoints=4000;
     for (int i = 0; i <lStrands.Count; i++)
        {
            Vector3[] atomPos=new Vector3[lStrands[i].GetBackBoneNum()];
            for(int j=0;j<lStrands[i].GetBackBoneNum();j++)
            {
                atomPos[j]=lStrands[i].GetBackBone(j).pos;
            }

            Vector3[] SmoothingPos= LineSmoother.GenerateSmoothCurve(atomPos,_segment,_step);
            for(int k=0;k<=SmoothingPos.Length/maxPoints;k++)
            {
            GameObject g=new GameObject("strandTube");
            g.transform.rotation=_father.rotation;
            g.transform.position=_father.position;
            g.transform.localScale=_father.localScale;
            g.AddComponent<TubeRenderer>();

            if(k==SmoothingPos.Length/maxPoints)
            g.GetComponent<TubeRenderer>().points=SmoothingPos.ToList().GetRange(k*maxPoints,SmoothingPos.Length-k*maxPoints).ToArray();
            else
            g.GetComponent<TubeRenderer>().points=SmoothingPos.ToList().GetRange(k*maxPoints,maxPoints).ToArray();
            
            g.GetComponent<TubeRenderer>().radius=_strandWidth;
            g.GetComponent<MeshRenderer>().material=new Material(Shader.Find("HDRP/LitTessellation"));
            g.GetComponent<MeshRenderer>().material.SetFloat("_Metallic",1f);
            g.GetComponent<MeshRenderer>().material.SetFloat("_Smoothness",0.256f);
            g.GetComponent<MeshRenderer>().material.color=lStrands[i].GetColor();

            g.transform.parent=_father;
            }
            
        }
 }

  public void GenerateRibbon(GameObject _g,float _strandWidth,Transform _father)
 {
    
     for (int i = 0; i <lStrands.Count; i++)
        {

            Vector3[] atomPos=new Vector3[lStrands[i].GetBackBoneNum()];
            for(int j=0;j<lStrands[i].GetBackBoneNum();j++)
            {
                atomPos[j]=lStrands[i].GetBackBone(j).pos;
            }

           Vector3[] SmoothingPos= LineSmoother.GenerateSmoothCurve(atomPos,10,1f);
           GameObject g=new GameObject("strandMesh");
           SplineNode[]splineNodes=new SplineNode[SmoothingPos.Length];
           float width=0.3f;
           float thickness=0.1f;
           for(int j=0;j<SmoothingPos.Length;j++)
           {
            
            if(j==SmoothingPos.Length-1)
            {splineNodes[j]=new SplineNode(SmoothingPos[j],SmoothingPos[j-1]);
            splineNodes[j].Scale=new Vector2(width,thickness);}
            else if(j==0)
            {
            {splineNodes[j]=new SplineNode(SmoothingPos[j],SmoothingPos[j+1]);
            splineNodes[j].Scale=new Vector2(width,thickness);}
            }
            else
            {splineNodes[j]=new SplineNode(SmoothingPos[j],(SmoothingPos[j+1]));
            splineNodes[j].Scale=new Vector2(width,thickness);}
            
           }
        // SplineNode[]splineNodes=new SplineNode[3];
        //    splineNodes[0]=new SplineNode(Vector3.zero,Vector3.one);
        //    splineNodes[1]=new SplineNode(Vector3.one,new Vector3(1f,1f,1.5f)) ;
        //    splineNodes[2]=new SplineNode(new Vector3(1f,1f,1.5f),Vector3.up) ;


           Spline spline= g.AddComponent<Spline>();
           spline.AddNodes(splineNodes);
           SplineExtrusion splineExtrusion= g.AddComponent<SplineExtrusion>();
           splineExtrusion.sampleSpacing=2f;
           splineExtrusion.textureScale=1f;
           splineExtrusion.material=new Material(Shader.Find("HDRP/LitTessellation"));
           splineExtrusion.material.color=Color.red;
            splineExtrusion.material.SetFloat("_Metallic",1f);
            splineExtrusion.material.SetFloat("_Smoothness",0.256f);
            splineExtrusion.material.SetFloat("_DoubleSidedEnable",1f);
           g.transform.parent=_father;

        }
 }

 public void LoadRCBuffer(ref Material m,List<Color>Color1,List<Color>Color2,List<int>Rank1,List<int>Rank2) //load color ranke buffer 
 {

    ComputeBuffer AtomRank1Buffer=new ComputeBuffer(Rank1.Count, sizeof(int), ComputeBufferType.Default);
    ComputeBuffer AtomRank2Buffer=new ComputeBuffer(Rank2.Count, sizeof(int), ComputeBufferType.Default);
    ComputeBuffer AtomColor1Buffer=new ComputeBuffer(Color1.Count, sizeof(float)*4, ComputeBufferType.Default);
    ComputeBuffer AtomColor2Buffer=new ComputeBuffer(Color2.Count, sizeof(float)*4, ComputeBufferType.Default);

    AtomRank1Buffer.SetData(Rank1);

    AtomRank2Buffer.SetData(Rank2);

    AtomColor1Buffer.SetData(Color1);

    AtomColor2Buffer.SetData(Color2);


    m.SetBuffer("_AtomRank1Buffer", AtomRank1Buffer);
    m.SetBuffer("_AtomRank2Buffer", AtomRank2Buffer);
    m.SetBuffer("_AtomColor1Buffer", AtomColor1Buffer);
    m.SetBuffer("_AtomColor2Buffer", AtomColor2Buffer);
    m.SetInt("_ConnectNum", Rank1.Count);

 }



    
#endregion


}
