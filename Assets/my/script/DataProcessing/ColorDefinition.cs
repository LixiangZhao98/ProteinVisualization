using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDefinition 
{
   public static Color HColor=new Color(0.9f,0.9f,0.9f,1f);               //atom1    //=new Color(0.3f,0.3f,0.3f,1f);  //atom2
   public static Color CColor=new Color(0.4f,0.4f,0.4f,1f);               //atom1    //=new Color(0.1f,0.1f,0.1f,1f); //atom2
    
   public static Color OColor=Color.red;
   public static Color NColor=Color.blue;
   public static Color SColor=Color.yellow;
   public static Color[] strandsColorPool=new Color[5]{Color.green,Color.yellow,Color.red,Color.blue,Color.cyan};
}
