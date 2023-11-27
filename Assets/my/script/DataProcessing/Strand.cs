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
   {
      lAminoAcids[i].GetCAtom().bonds.Add(lAminoAcids[i+1].GetNAtom());
      lAminoAcids[i+1].GetNAtom().bonds.Add(lAminoAcids[i].GetCAtom());
      lAminoAcids[i].GetCAtom().singleDirectionBonds.Add(lAminoAcids[i+1].GetNAtom());
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
#endregion
}
