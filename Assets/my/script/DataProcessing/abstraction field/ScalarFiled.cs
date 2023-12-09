//
//  ScalarField.cs
//  MeTACAST
//
//  Copyright (c) 2022, 2023 Lixiang Zhao. All rights reserved.
//

using System;
using System.Collections.Generic;
using UnityEngine;



    [System.Serializable]
    public class FieldNode
    {
        [SerializeField]
        private Vector3 nodePosition;
        [SerializeField]
        private double nodeValue;
        [SerializeField]
        private double enclosedParticleDis;
        [SerializeField]
        private Vector3 nodeGradient;
        [SerializeField]
        private Vector3 nodeGridPos;
        public FieldNode(Vector3 pos,Vector3 gridPos)
        {
            nodePosition = pos;
            nodeGridPos = gridPos;
        }
        public void SetEnclosedParticleDis(double dis)
        {
            enclosedParticleDis = dis;
        }
        public double GetEnclosedParticleDis()
        {
            return enclosedParticleDis;
        }
        public double GetNodeValue()
        {
            return nodeValue;
        }
        public Vector3 GetNodePosition()
        {
            return nodePosition;
        }
        public Vector3 GetNodeGradient()
        {
            return nodeGradient;
        }
        public Vector3 GetNodeGridPos()
        {
            return nodeGridPos;
        }
        public void SetNodeValue(double value)
        {
            nodeValue = value;
        }
        public void SetNodeGradient(Vector3 g)
        {
            nodeGradient = g;
        }

        public void NodeValuePlusDis(double dis)
        {
            enclosedParticleDis = enclosedParticleDis + dis;
        }
    }
    [System.Serializable]
    public class ScalarField
    {
        [SerializeField]
        public string name;
        [SerializeField]
        private List<FieldNode> fieldNode;
        [SerializeField]
        private int[] boxValue;
        [SerializeField]
        private List<LUTUnit> LUT_;
        [SerializeField]
        private int xNum;  //total nodes number on x axis
        [SerializeField]
        private int yNum;
        [SerializeField]
        private int zNum;
        [SerializeField]
        private float xStep;  //distance between two nodes along x axis
        [SerializeField]
        private float yStep;
        [SerializeField]
        private float zStep;
        [SerializeField]
        Vector3 minPos;
        [SerializeField]
        Vector3 maxPos;
        [SerializeField]
        float maxValue;
        Texture3D fieldTexture;
        private float AveNodeValue;
        public float XSTEP { get { return xStep; } }
        public float YSTEP { get { return yStep; } }
        public float ZSTEP { get { return zStep; } }
        public Vector3 MINPOS { get { return minPos; } }
        public Vector3 MAXPOS { get { return maxPos; } }
        public int XNUM { get { return xNum; } }
        public int YNUM { get { return yNum; } }
        public int ZNUM { get { return zNum; } }

        public float MAXVALUE { get { return maxValue; } }

        public float AVE_NODE_VALUE { get { return AveNodeValue; } }


        public  int VectorToBoxIndex(Vector3 v)  
        {
            int index = (int)((v.x - minPos.x) / XSTEP) + (int)((v.y - minPos.y) / YSTEP) * XNUM + (int)((v.z - minPos.z) / ZSTEP) * XNUM * YNUM;
            if (index >= GetNodeNum() || index <= 0 || v.x > maxPos.x || v.x < minPos.x || v.y > maxPos.y || v.y < minPos.y || v.z > maxPos.z || v.z < minPos.z)
            { return -1; }
            else
                return index;
        }
        public void InitializeFieldByGapDis(string pgName,float xmin, float xmax, int xAxisNum, float ymin, float ymax, int yAxisNum, float zmin, float zmax, int zAxisNum)
         {
           
            name = pgName;
            maxPos=new Vector3(xmax,ymax,zmax);
            minPos=new Vector3(xmin,ymin,zmin);
            fieldNode = new List<FieldNode>();
            xStep = (xmax - xmin) / (xAxisNum-1);
            yStep = (ymax - ymin) / (yAxisNum-1);
            zStep = (zmax - zmin) / (zAxisNum-1);

            for (float z = 0; z < zAxisNum; z+= 1)
            {
               
                for (float y = 0; y < yAxisNum; y+= 1)
                {
                    
                    for (float x= 0; x < xAxisNum; x+= 1)
                    {
                        FieldNode fd = new FieldNode(new Vector3(xmin+x*xStep, ymin+y*yStep, zmin+z*zStep), new Vector3(x, y, z));
                      
                        fieldNode.Add(fd);
                       
                    }
                   
                }
              
            }

            

            xNum = xAxisNum;
            yNum = yAxisNum;
            zNum = zAxisNum;
        
            DiscreteClear();
            fieldTexture = new Texture3D(xNum, yNum, zNum, TextureFormat.RGBA32, true);
            Debug.Log("Create density field success");
        }

        public void UpdateTexture()
        {

        Color[] colorArray = new Color[xNum * yNum * zNum];
        // for (int x = 0; x < xNum; x++)
        // {
        //     for (int y = 0; y < yNum; y++)
        //     {
        //         for (int z = 0; z < zNum; z++)
        //         {
        //             Color c = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        //             colorArray[x + (y * xNum) + (z * xNum * yNum)] = c;
        //         }
        //     }
        // }
        for(int i = 0; i < GetNodeNum(); i++)
        {
            colorArray[i] =new Color((float)GetNodeValue(i), 1.0f, 1.0f, 1.0f);
        }
        fieldTexture.SetPixels(colorArray);
        fieldTexture.Apply();
        }

        public float GetProcessedFloat(float f)
        {
            int EffectiveCount = 2;
            string SNumber = f.ToString();
            char[] CNumberArr = SNumber.ToCharArray();
            int DotIndex = SNumber.IndexOf('.');
            double value = 0;
            int TempPrecision = 0;
            for (int i = DotIndex + 1; i < SNumber.Length; i++)
            {
                TempPrecision++;
                if (CNumberArr[i] != '0')
                {
                    value = Math.Round(f, TempPrecision + EffectiveCount - 1);
                    break;
                }
            }
            return (float)value;
        }

       
        public int NodePosToIndex(int z, int y, int x)
        {
            return (z) * xNum * yNum + (y) * xNum + x;

        }

        public void DiscreteClear()
        {
            boxValue = new int[xNum * yNum * zNum];

            LUT_ = new List<LUTUnit>();
            for (int i = 0; i < xNum*yNum*zNum; i++)
                LUT_.Add(new LUTUnit());

        }

        public void AddToLUT(int index, int targetint)
        {
            LUT_[index].AddToLUT(targetint);
        }

       
        #region Get
        public Vector3 GetNodeGradient(int i)
        {
            return fieldNode[i].GetNodeGradient();
        }
        public Vector3 GetNodedPos(int i)
        {
            return fieldNode[i].GetNodePosition();
        }
        public Vector3 GetNodeGridPos(int i)
        {
            return fieldNode[i].GetNodeGridPos();
        }
        public double GetNodeValue(int i)
        {
            return fieldNode[i].GetNodeValue();
        }
 
        public int GetNodeNum()
        {
            return fieldNode.Count;
        }

        public List<int> GetLUTUnit(int index)
        {
            return LUT_[index].GetLTUnit();
        }

        public Texture3D GetTexture3D()
        {
            return fieldTexture;
        }


        #endregion

        #region Set

     public void LoadFieldValue(float[] values)
    {
        if(values.Length!=GetNodeNum())
        {
            throw new Exception("The input values have different dimension with the field");
        }
        else
        {
        maxValue=float.MinValue;
        for(int i=0;i<values.Length;i++)
        {
            SetNodevalue(i,values[i]);
            if(values[i]>maxValue)
            maxValue=values[i];
        }
        UpdateTexture();
        Debug.Log("Load field success");
        }
    }
         public void LoadFieldValue(float value)
    {

        maxValue=value;
        for(int i=0;i<GetNodeNum();i++)
        {
            SetNodevalue(i,value);
        }
        UpdateTexture();
        Debug.Log("Load field success");
        
    }

         void SetNodevalue(int i, double value)
        {
            fieldNode[i].SetNodeValue(value);
        }
         void SetNodeGradient(int i, Vector3 g)
        {
            fieldNode[i].SetNodeGradient(g);
        }
         void SetAveNodeValue(float f)
        {
            AveNodeValue = f;
        }

        #endregion
    }

    [System.Serializable]
    public class LUTUnit
    {[SerializeField]
        List<int> LUTUnit_;
        public LUTUnit()
        {
            LUTUnit_ = new List<int>();
        }
        public void AddToLUT(int targetint)
        {
            LUTUnit_.Add(targetint);
        }

        public List<int> GetLTUnit()
        {
            return LUTUnit_;
        }
    }
