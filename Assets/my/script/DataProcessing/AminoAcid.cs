using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
                float dx = lAtoms[i].pos.x - lAtoms[j].pos.x;
                float dz = lAtoms[i].pos.z - lAtoms[j].pos.z;
                float dy = lAtoms[i].pos.y - lAtoms[j].pos.y;

                double dis = Mathf.Sqrt((dx * dx) + (dy * dy) + (dz * dz));

                if (dis < 1.6f)
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
        g.transform.parent=_father.GetChild(0);
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
                
                g.transform.localScale=new Vector3(_standardStickWidth,(start-end).magnitude/2*1.1f ,_standardStickWidth); //*1.2f 为了让圆柱嵌入球体
                g.transform.up=dir;
                g.GetComponent<Renderer>().material.SetColor("_Color1",lAtoms[i].Color);
                g.transform.GetChild(0).GetComponent<Renderer>().material.color=lAtoms[i].Color;
                g.GetComponent<Renderer>().material.SetColor("_Color2",lAtoms[i].singleDirectionBonds[j].Color);
                g.transform.GetChild(1).GetComponent<Renderer>().material.color=lAtoms[i].singleDirectionBonds[j].Color;
                g.transform.parent=_father.GetChild(1);
            }
        }
}
public void GenerateLongStick(GameObject _g,float _standardStickWidth,Transform _father)
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
                
                g.transform.localScale=new Vector3(_standardStickWidth,(start-end).magnitude/2,_standardStickWidth); 
                g.transform.up=dir;
                g.GetComponent<Renderer>().material.SetColor("_Color1",lAtoms[i].Color);
                g.transform.GetChild(0).GetComponent<Renderer>().material.color=lAtoms[i].Color;
                g.GetComponent<Renderer>().material.SetColor("_Color2",lAtoms[i].singleDirectionBonds[j].Color);
                g.transform.GetChild(1).GetComponent<Renderer>().material.color=lAtoms[i].singleDirectionBonds[j].Color;
                g.transform.parent=_father.GetChild(1);
            }
        }
 }
#endregion

}
