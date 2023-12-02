using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolation : MonoBehaviour
{
static public float LinearInterp(float x,float x0,float x1,float y0,float y1)
{
   return y0+(y1-y0)/(x1-x0) *(x-x0);
}
}
