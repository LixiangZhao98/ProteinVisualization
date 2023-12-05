using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Strand 
{
    int strandID;
    Color color;
    List<AminoAcid> lAminoAcids;
    
    List<Atom> backBone;
       int countN = 0;
    int countH = 0;
    int countO = 0;
    int countC = 0;
    int countS=0;
    int others=0;
   public int total=0;
    public Strand(int _strandID,Color _color)
    {
      lAminoAcids= new List<AminoAcid>();
      backBone=new List<Atom>();
      strandID=_strandID;
      color=_color;
    }
        public Strand(int _strandID)
    {
      lAminoAcids= new List<AminoAcid>();
      backBone=new List<Atom>();
      strandID=_strandID;
      color=Color.red;
    }

   public Strand(int _strandID,Color _color,AminoAcid[] _aminoAcids)
    {
      lAminoAcids= _aminoAcids.ToList();
      backBone=new List<Atom>();
      strandID=_strandID;
      color=Color.red;
    }

    public Color GetColor()
    {
      return color;
    }
        public int GetStrandID()
    {
      return strandID;
    }
public Vector3 GetCenter()
{
    Vector3 v=Vector3.zero;
    foreach(var aminoAcid in lAminoAcids)
    {
        v=v+aminoAcid.GetCenter();
    }
    return v/lAminoAcids.Count;
}
public AminoAcid[] GetAminoAcids()
{
    return lAminoAcids.ToArray();
}
public List<Atom> GetAtoms()
{
    List<Atom> atoms=new List<Atom>();
    foreach(var a in lAminoAcids)
    {
     atoms.AddRange( a.GetlAtoms());
    }
    return atoms;
}
public Atom[] GetBackBone()
{
    return backBone.ToArray();
}
public Atom GetBackBone(int i)
{
    return backBone[i];
}
public int GetAminoAcidsNum()
{
    return lAminoAcids.Count;
}
public int GetBackBoneNum()
{
    return backBone.Count;
}
    public void AddAminoAcids(AminoAcid _aminoAcid)
{
    lAminoAcids.Add(_aminoAcid);
}

    public void AddAtomToBackBone(Atom _atoms)
{
    backBone.Add(_atoms);
}

public void shiftVec3(Vector3 _shift)
{
    foreach(var a in lAminoAcids)
    {
        a.shiftVec3(_shift);
    }

}

public void HowManyAtoms(ref int _countN,ref int _countC,ref int _countO,ref int _countH,ref int _countS)
{
   foreach(var a in lAminoAcids)
   {
      a.HowManyAtoms(ref countN,ref countC,ref countO,ref countH,ref countS);
   }
    
    _countN+=countN;
    _countC+=countC;
    _countO+=countO;
    _countH+=countH;
    _countS+=countS;
}

public void AppendBondInsideRes()
{
   
   foreach(var a in lAminoAcids)
   {
       a.AppendBondInsideRes();
   }
}

public void AppendBondBetweenRes()
{
   for(int i=0;i< lAminoAcids.Count-1;i++)
   {                if (((lAminoAcids[i].GetCAtom().pos-lAminoAcids[i+1].GetNAtom().pos).magnitude) < 1.6f)
                {
      lAminoAcids[i].GetCAtom().bonds.Add(lAminoAcids[i+1].GetNAtom());
      lAminoAcids[i+1].GetNAtom().bonds.Add(lAminoAcids[i].GetCAtom());
      lAminoAcids[i].GetCAtom().singleDirectionBonds.Add(lAminoAcids[i+1].GetNAtom());
                }

   }
}


#region  rendering
public void GenerateSphere(GameObject _g,float _standardAtomScale,Transform _father)
{
   foreach(var a in lAminoAcids)
   {

       a.GenerateSphere(_g,_standardAtomScale,_father);
       }
}
public void GenerateSphereCombined(ref List<Transform> ts,float _standardAtomScale)
{
   foreach(var a in lAminoAcids)
   {

       a.GenerateSphereCombined( ref ts,_standardAtomScale);
       }
}
public void GenerateShortStick(GameObject _g,float _standardStickWidth,Transform _father)
{
   foreach(var a in lAminoAcids)
   {

       a.GenerateShortStick(_g,_standardStickWidth,_father);
       }
}
public void GenerateLongStick(GameObject _g,float _standardStickWidth,Transform _father)
{
   foreach(var a in lAminoAcids)
   {

       a.GenerateLongStick(_g,_standardStickWidth,_father);
       }
}
public void GenerateBackBone(GameObject _g,float _standardStickWidth,Transform _father)
{
            for(int i=0;i<backBone.Count-1;i++)
            {
                Vector3 A=backBone[i].pos;
                Vector3 B=backBone[i+1].pos;
                Vector3 dir=(A-B).normalized; //A-B
                Vector3 start=A;
                Vector3 end=B;
                GameObject g= GameObject.Instantiate(_g,(start+end)/2,Quaternion.identity);
                g.name=backBone[i].fullElementName+" "+backBone[i+1].fullElementName;
                g.transform.localScale=new Vector3(_standardStickWidth,(start-end).magnitude/2*1.04f,_standardStickWidth); 
                g.transform.up=dir;
                g.GetComponent<Renderer>().material.SetColor("_Color1",backBone[i].Color);
                g.GetComponent<Renderer>().material.SetColor("_Color2",backBone[i+1].Color);

                g.transform.parent=_father;
                
            }
}
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
#endregion
}
