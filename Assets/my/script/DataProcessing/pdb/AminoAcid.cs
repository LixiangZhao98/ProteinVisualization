using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using utility;

public class AminoAcid
{
int aminoAcidID;
List<Atom> lAtoms;

Atom C;
Atom N;

Vector3 forwardDir;

Vector3 upDir;

   int countN = 0;
    int countH = 0;
    int countO = 0;
    int countC = 0;
    int countS=0;
    int others=0;
   public int total=0;
public AminoAcid(int _AminoAcidID)
{
aminoAcidID=_AminoAcidID;
lAtoms=new List<Atom>();
C=new Atom();
N=new Atom();
}
public AminoAcid(int _AminoAcidID,Atom[] atoms)
{
aminoAcidID=_AminoAcidID;
lAtoms=atoms.ToList();
C=new Atom();
N=new Atom();
}
public Atom[] GetlAtoms()
{
    return lAtoms.ToArray();
}
public int GetAtomsNum()
{
    return lAtoms.Count;
}
public Vector3 GetUpDir()
{
    return upDir;
}
public Vector3 GetForwardDir()
{
    return forwardDir;
}
public int GetAminoAcidID()
{
    return aminoAcidID;
;
}
public Vector3 GetCenter()
{
    Vector3 v=Vector3.zero;
    foreach(var atom in lAtoms)
    {
        v=v+atom.pos;
    }
    return v/lAtoms.Count;
}
public Atom GetCAtom()
{
    return C;
}

public Atom GetNAtom()
{
    return N;
}


public void SetCAtom(Atom _atom)
{
    C=_atom;
}

public void SetNAtom(Atom _atom)
{
    N=_atom;
}


public void AddAtom(Atom _atom)
{
    lAtoms.Add(_atom);
}

public void shiftVec3(Vector3 _shift)
{
    foreach(var a in lAtoms)
    {
        a.pos+=_shift;
    }
}
public void HowManyAtoms(ref int _countN,ref int _countC,ref int _countO,ref int _countH,ref int _countS)
{
    for (int i = 0; i < lAtoms.Count; i++)
        {
            if (lAtoms[i].elementName == "N")
            {
                countN++;
            }
            else if (lAtoms[i].elementName == "C")
            {
                countC++;
            }
            else if (lAtoms[i].elementName == "O")
            {
                countO++;
            }
            else if (lAtoms[i].elementName == "H")
            {
                countH++;
            }
            else if (lAtoms[i].elementName == "S")
            {
                countS++;
            }
            else
            {
               
                others++;
            }
        }
    
    _countN+=countN;
    _countC+=countC;
    _countO+=countO;
    _countH+=countH;
    _countS+=countS;
}

public void AppendBondInsideRes()
{
        for (int i = 0; i < lAtoms.Count; i++)
        {
            for (int j = i + 1; j < lAtoms.Count; j++)
            {
                if(lAtoms[i].elementName.Equals("H")&&lAtoms[j].elementName.Equals("H"))
                return;

                float dx = lAtoms[i].pos.x - lAtoms[j].pos.x;
                float dz = lAtoms[i].pos.z - lAtoms[j].pos.z;
                float dy = lAtoms[i].pos.y - lAtoms[j].pos.y;

                double dis = Mathf.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
                float bondThre=0f;

                if(lAtoms[i].elementName.Equals("S")||lAtoms[j].elementName.Equals("S"))
                bondThre=2f;
                else
                bondThre=1.6f;

                if (dis < bondThre)
                {
                    lAtoms[i].bonds.Add(lAtoms[j]);
                    lAtoms[j].bonds.Add(lAtoms[i]);
                    lAtoms[i].singleDirectionBonds.Add(lAtoms[j]);
                }


            }
        }
}



#region  rendering
public void GenerateSphere(GameObject _g,float _standardAtomScale,Transform _father)
{
    for(int i=0;i<lAtoms.Count;i++)
       {

        GameObject g= GameObject.Instantiate(_g,lAtoms[i].pos,Quaternion.identity);
        g.name=lAtoms[i].fullElementName;
        
        g.transform.GetComponent<Renderer>().material.color=lAtoms[i].Color;
        g.transform.localScale=lAtoms[i].Radius*_standardAtomScale*Vector3.one;
        lAtoms[i].obj=g;
       Atom A= g.AddComponent<Atom>();
       A.Radius=lAtoms[i].Radius;
        g.transform.parent=_father
        ;
       }
}
public void GenerateSphereCombined(ref List<Transform> ts,float _standardAtomScale)
{
    for(int i=0;i<lAtoms.Count;i++)
       {

        
       }
}
public void GenerateShortStick(GameObject _g,float _standardStickWidth,Transform _father)
{
     for (int i = 0; i < lAtoms.Count; i++)
        {
            for (int j = 0; j < lAtoms[i].singleDirectionBonds.Count; j++)
            {
                Vector3 A=lAtoms[i].pos;
                Vector3 B=lAtoms[i].singleDirectionBonds[j].pos;
                Vector3 dir=(A-B).normalized; //A-B
                Vector3 start=A-dir*lAtoms[i].Radius/2;
                Vector3 end=B+dir*lAtoms[i].singleDirectionBonds[j].Radius/2;
                GameObject g= GameObject.Instantiate(_g,(start+end)/2,Quaternion.identity);
                g.name=lAtoms[i].fullElementName+" "+lAtoms[i].singleDirectionBonds[j].fullElementName;

                g.transform.localScale=new Vector3(_standardStickWidth,(start-end).magnitude/2*1.1f ,_standardStickWidth); //*1.2f 为了让圆柱嵌入球体
                g.transform.up=dir;
                g.GetComponent<Renderer>().material.SetColor("_Color1",lAtoms[i].Color);

                g.GetComponent<Renderer>().material.SetColor("_Color2",lAtoms[i].singleDirectionBonds[j].Color);


                g.transform.parent=_father;
            }
        }
}
public void GenerateLongStick(GameObject _g,float _standardStickWidth,Transform _father,Material m,List<Color>Color1,List<Color>Color2,List<int>Rank1,List<int>Rank2,ref int stickIndex)
 {
    
         for (int i = 0; i < lAtoms.Count; i++)
        {
            for (int j = 0; j < lAtoms[i].singleDirectionBonds.Count; j++)
            {
                Vector3 A=lAtoms[i].pos;
                Vector3 B=lAtoms[i].singleDirectionBonds[j].pos;
                Vector3 dir=(A-B).normalized; //A-B
                Vector3 start=A;
                Vector3 end=B;
                GameObject g= GameObject.Instantiate(_g,(start+end)/2,Quaternion.identity);
                g.name=lAtoms[i].fullElementName+" "+lAtoms[i].singleDirectionBonds[j].fullElementName;
                                
                g.transform.localScale=new Vector3(_standardStickWidth,(start-end).magnitude/2*1.04f,_standardStickWidth); 
                g.transform.up=dir;
                // g.GetComponent<Renderer>().material.SetColor("_Color1",lAtoms[i].Color);
                // g.GetComponent<Renderer>().material.SetFloat("_Rank1",lAtoms[i].AtomRank);
                // g.GetComponent<Renderer>().material.SetColor("_Color2",lAtoms[i].singleDirectionBonds[j].Color);
                // g.GetComponent<Renderer>().material.SetFloat("_Rank2",lAtoms[i].singleDirectionBonds[j].AtomRank);

                Color1.Add(lAtoms[i].Color);
                Color2.Add(lAtoms[i].singleDirectionBonds[j].Color);
                Rank1.Add(lAtoms[i].AtomRank);
                Rank2.Add(lAtoms[i].singleDirectionBonds[j].AtomRank);

                g.GetComponent<Renderer>().material=m;
                Vector2[] uv2=new Vector2[g.GetComponent<MeshFilter>().mesh.vertexCount];
                for(int ii=0;ii<uv2.Length;ii++)
                uv2[ii]=new Vector2(stickIndex,0f);
                g.GetComponent<MeshFilter>().mesh.uv2=uv2;
                g.transform.parent=_father;
                stickIndex++;
            }
        }
 }
#endregion

}
