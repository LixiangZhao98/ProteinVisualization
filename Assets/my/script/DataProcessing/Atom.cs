using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atom
    {
        public int atomID;
        public int aminoAcidsID;
        public string elementName;   //C

        public string fullElementName;  //CA
        public Color Color;

        public float Radius;
        public string aminoAcidName;  
        public string AtomID;   //A,B,G,D,E,Z,H"来标记这些重原子，对应的希腊字母是"α,β,γ,δ,ϵ,ζ,η
        public float bFactor;
        public Vector3 pos;
        public GameObject obj;
        public List<Atom> bonds;  //双向索引
        public List<Atom> singleDirectionBonds; //单项索引，构建stick diagram

        public Atom()
        {
         
        bonds = new List<Atom>();
        singleDirectionBonds = new List<Atom>();
        }

    }
