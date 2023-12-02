using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atom:MonoBehaviour
    {
        public int atomID;
        public int aminoAcidsID;
        public string elementName;   //C

        public string fullElementName;  //CA
        public Color Color;

        public float Radius;
        public string aminoAcidName;  
        public string AtomIndex;
           //A,B,G,D,E,Z,H"来标记这些重原子，对应的希腊字母是"α,β,γ,δ,ϵ,ζ,η
        public int AtomRank;
           //对应A,B,G,D,E,Z,H"
        
        public float bFactor;
        public Vector3 pos;
        public GameObject obj;
        public List<Atom> bonds;  //双向索引
        public List<Atom> singleDirectionBonds; //单项索引，构建stick diagram

        public bool visited; 
        public Atom()
        {
         
        bonds = new List<Atom>();
        singleDirectionBonds = new List<Atom>();
        visited=false;
        }

    internal void ForEach(Func<object, object> value)
    {
        throw new NotImplementedException();
    }
}
