/**
 * 
 * The Bezier class holds the data for all the points that make up the path
 * The class also determines how the mesh is extruded along the path
 * 
 * Author: Magnus Söderberg
 * Last updated 2022-03-22
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[System.Serializable]
public class Bezier {

    //[SerializeField]
    public event Action UpdateMeshEvent;
    [SerializeField]
    public List<int> NumMeshesPerSegment;
    [SerializeField]
    public int NumMeshes;
    [SerializeField]
    public List<Vector3> points;
    [SerializeField]
    private Vector3 tangent;

    public Bezier(Vector3 startingPos)
    {
        points = new List<Vector3>();
        points.Add(startingPos);
        points.Add(new Vector3(10f, 20f, 0) + startingPos);
        points.Add(new Vector3(30f, 20f, 0) + startingPos);
        points.Add(new Vector3(40f, 0, 0) + startingPos);
    }
        public Bezier(Vector3[] vs)
    {
        points = new List<Vector3>();
        points.AddRange(vs);

    }

    /// <summary>
    /// Returns the distances for all the points needed to extrude the mesh on the path.
    /// If a Mesh overlaps between two segments, the remainer distance is put as first element in the next segment.
    /// This means that the mesh continues its extrusion smoothly over segment borders.
    /// </summary>
    public List<float[]> MeshLengthsOnCurve(List<float> distinctVertexPositions, List<float> lengthsOfSegments)
    {
        NumMeshes = 0;

        //All distances for all segments
        List<float[]> totalList = new List<float[]>();
        //Current segment
        List<float> lengthsOnCurrentSegment = new List<float>();
        NumMeshesPerSegment = new List<int>();

        float remainder = 0;
        int index = 1;
        bool segmentChange = false;

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            lengthsOnCurrentSegment.Clear();
            int numMeshPerSegment = 0;
            float currentDistance = 0;

            //First distance of the first segment is always 0
            if (segmentIndex == 0)
            {
                lengthsOnCurrentSegment.Add(0);
            }

            while (true)
            {
                //If this is the last distinct distance in the Mesh, increase the number of meshes count
                //and reset index
                if (index == distinctVertexPositions.Count)
                {
                    index = 1;
                    numMeshPerSegment++;
                    NumMeshes++;

                }

                //If this is within a segment take the difference in length between neighboring distinct distances of the Mesh
                if (!segmentChange)
                {
                    currentDistance += distinctVertexPositions[index] - distinctVertexPositions[index - 1];
                }
                //If the previous mesh overlapped between two segments then correct for the remainder distance
                else
                {
                    currentDistance = distinctVertexPositions[index] - distinctVertexPositions[index - 1] - remainder;
                    segmentChange = false;
                }
                //If the currentDistance <0 then there is a problem with the calculated distanct distances
                if (currentDistance < 0)
                {
                    Debug.LogWarning("currentDistance less than 0. Set Remainder to zero");
                    remainder = 0;
                }
                if (currentDistance <= lengthsOfSegments[segmentIndex] && currentDistance > 0)
                {
                    lengthsOnCurrentSegment.Add(currentDistance);
                    index++;
                }
                //If the currentDistance is longer then the current segment then save the remainer distance
                else
                {
                    float previousDistance = currentDistance - distinctVertexPositions[index] + distinctVertexPositions[index - 1];

                    remainder = lengthsOfSegments[segmentIndex] - previousDistance;
                    segmentChange = true;
                    break;
                }

            }

            totalList.Add(lengthsOnCurrentSegment.ToArray());
            NumMeshesPerSegment.Add(numMeshPerSegment);
        }


        return totalList;

    }



    /// <summary>
    /// Places all control points (shown as red balls in the scene) in a straight line (same tangent) 
    /// so the intersection between segment become smooth
    /// </summary>
    public void StraightenControlPoints(int segment) {

        Vector3[] p1 = GetPointsInSegment(segment-1);
        Vector3[] p2 = GetPointsInSegment(segment);

        //Get the tangent of the point before the anchor point
        Vector3 tang1 = p1[3] - p1[2];

        //Get the distance of the control point that we are changing
        float distance = Vector3.Distance(p2[0], p2[1]);

        //Get the new point that we move the control point to
        Vector3 newPoint = p2[0] + (tang1.normalized * distance);

        points[segment * 3 + 1] = newPoint;

    }

   

    /// <summary>
    /// Provided a distance on the segment (corresponding to a vertex position of a mesh) the method
    /// calculates a t-value.
    /// The t-value is later used to obtain world position of that mesh vertex position. 
    /// </summary>
    public float CalculatetValue(float[] array, float length) {

        int upperIndex = 0;
        int lowerIndex = 0;
        float[] tArray = new float[array.Length];
        float[] minArray = new float[array.Length];

        //Make a list that goes from 0 to 1 with same number of elements as reference points
        for (int i = 0; i < tArray.Length; i++) {
            tArray[i] = (float)i / (tArray.Length - 1);  
        }

        //Find the index in the reference-length array that corresponds closest to the length
        for (int i = 0; i < array.Length; i++) {
            minArray[i] = array[i] - length;
            minArray[i] = Mathf.Abs(minArray[i]);
        }

        float min = minArray.Min();
        int index = Array.IndexOf(minArray, min);

        //If the length is the last reference point (end of Bezier curve)
        if (array[index] == length) {
            return tArray[index];
        }
        //If the closest estimation is larger than the length then we are going to interpolate a value between index and index-1
        else if (array[index] > length) {

            upperIndex = index;
            lowerIndex = index - 1;
        }
        else {
            upperIndex = index + 1;
            lowerIndex = index;
        }

        //Get the fraction where the length is located between array[upperIndex] and array[lowerIndex]
        float t = (length - array[lowerIndex]) / (array[upperIndex] - array[lowerIndex]);

        //Get the t-value through interpolation
        float tvalue = Mathf.Lerp(tArray[lowerIndex], tArray[upperIndex], t);

        //Rounding to four decimal points to make sure no rounding errors occur (float is terrible, precision ~6-9 digits)
        tvalue = (float)Mathf.Round(tvalue * 10000) / 10000;

        return tvalue;
    }


    /// <summary>
    /// Returns a list of reference lengths for each segment of the path
    /// The reference lengths are used to speed up the interpolation when calculating the t-values for the mesh vertex positions
    /// </summary>
    public List<float[]> ReferenceLengths() {

        List<float> ListOfLengthsPerSegment = new List<float>();
        List<float[]> totalListOfLengths = new List<float[]>();
        List<Vector3> PointsOnCurve = new List<Vector3>();

        PointsOnCurve.Add(points[0]);
        Vector3 previousPoint = PointsOnCurve[0];

        for (int i = 0; i < NumSegments; i++) {
            Vector3[] p = GetPointsInSegment(i);
            ListOfLengthsPerSegment.Add(0);
            float distance = 0;
            float t = 0;

            while (t < 1f) {

                t += 0.0025f;

                Vector3 pointOnCurve = GetPoint(p, t);
                distance += Vector3.Distance(previousPoint, pointOnCurve);
                ListOfLengthsPerSegment.Add(distance);
                previousPoint = pointOnCurve;
            }

            totalListOfLengths.Add(ListOfLengthsPerSegment.ToArray());
            ListOfLengthsPerSegment.Clear();

        }

        return totalListOfLengths;
    }


    public Vector3[] GetAnchorPoints() {
        List<Vector3> pp = new List<Vector3>();
        for (int i = 0; i < NumPoints; i++) {
            if(i % 3 == 0) {
                pp.Add(points[i]); 
            }
        }
        return pp.ToArray();
    }

    public void PositionChanged(Vector3 changeinPos) {
        for (int i = 0; i < points.Count; i++) {
            points[i] += changeinPos;
        }
    }

    public void MoveStartingPositionOfSegment(int segment, Vector3 newPos)
    {
        points[segment * 3] = newPos;

        if (UpdateMeshEvent != null)
            UpdateMeshEvent();
    }


    public Vector3 singlePoint(int i) {
        return points[i];
    }

    public Vector3[] AnchorPoints {
        get {
            return GetAnchorPoints();
        }
        set {
            SetAnchorPoints(value);
            if (UpdateMeshEvent != null)
               UpdateMeshEvent();

        }
    }
    public int NumPoints {
        get {
            return points.Count;
        }
    }
    public int NumSegments {
        get {
            return (points.Count - 4) / 3 + 1;
        }
    }

    public int NumAnchorPoints {
        get {
            return (NumSegments + 1);
        }
    }

    private void SetAnchorPoints(Vector3[] apoints) {
        int j = 0;
        for (int i = 0; i < NumPoints; i++) {
            if(i % 3 == 0) {
                points[i] = apoints[j];
                    j++;
            }
        }
    } 

    public Vector3[] GetPointsInSegment(int i) { 

        return new Vector3[] { singlePoint(i * 3), singlePoint(i * 3 + 1), singlePoint(i * 3 + 2), singlePoint(i * 3 + 3) };
    }

    public List<Vector3> GetPoints() {
        return points;
    }

    public Vector3 GetPoint(Vector3[] points, float t) {

        //Using Bernstein polynomials to get any point on a Bezier curve 

        float oneMinusT = 1f - t;
        float oneMinusTSquared = oneMinusT * oneMinusT;
        float t2 = t * t;

        return points[0] * (oneMinusTSquared * oneMinusT) +
               points[1] * (3f * oneMinusTSquared * t) +
               points[2] * (3f * oneMinusT * t2) +
               points[3] * (t2 * t);

    }

    /// <summary>
    /// Returns the normalized tangent of a Bezier at a certain t-value.
    /// </summary>
    public Vector3 GetTangent(Vector3[] points, float t) {

        Vector3 a = Vector3.Lerp(points[0], points[1], t);
        Vector3 b = Vector3.Lerp(points[1], points[2], t);
        Vector3 c = Vector3.Lerp(points[2], points[3], t);
        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        tangent = (e - d).normalized;

        return tangent;

    }

    /// <summary>
    /// Makes all points straight according to first point in segment in some axis
    /// </summary>
    public void StraightenSegment(int segment, string axis)
    {

        Vector3[] pointsInSegment = GetPointsInSegment(segment);

        if (axis.Equals("Z"))
        {
            for (int i = 0; i < pointsInSegment.Length; i++)
            {
                points[segment * 3 + i] = points[segment * 3 + i] + new Vector3(pointsInSegment[0].x - points[segment * 3 + i].x, 0, 0);
            }
        }
        else if (axis.Equals("Y"))
        {
            for (int i = 0; i < pointsInSegment.Length; i++)
            {
                points[segment * 3 + i] = points[segment * 3 + i] + new Vector3(0, pointsInSegment[0].y - points[segment * 3 + i].y, 0);
            }
        }
        else if (axis.Equals("X"))
        {
            for (int i = 0; i < pointsInSegment.Length; i++)
            {
                points[segment * 3 + i] = points[segment * 3 + i] + new Vector3(0, 0, pointsInSegment[0].z - points[segment * 3 + i].z);
            }
        }

        if (UpdateMeshEvent != null)
            UpdateMeshEvent();


    }

    /// <summary>
    /// Makes the whole path straight with respect to the first point and some axis 
    /// </summary>

    public void StraightenCurve(string axis) {

        for (int i = 0; i < NumSegments; i++)
        {
            StraightenSegment(i, axis);

        }

        if (UpdateMeshEvent != null)
            UpdateMeshEvent();

    }


    public void AddSegment() {

        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);


    }

    public void RemoveSegment(int segment) {
        
        for (int i = 0; i < 3; i++) {
           points.RemoveAt(segment * 3 + 1); 
        }

    }

    public void MovePoint(int i, Vector3 pos) {
        Vector3 deltaMove = pos - points[i];
        points[i] += deltaMove;

    }

    public Vector3 GetCenterPivot()
    {
        float maxZ = float.MinValue;
        float minZ = float.MaxValue;

        float maxX = float.MinValue;
        float minX = float.MaxValue;

        float maxY = float.MinValue;
        float minY = float.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].x > maxX)
            {
                maxX = points[i].x;

            }
            if (points[i].x < minX)
            {
                minX = points[i].x;
            }

            if (points[i].y > maxY)
            {
                maxY = points[i].y;

            }
            if (points[i].y < minY)
            {
                minY = points[i].y;
            }

            if (points[i].z > maxZ)
            {
                maxZ = points[i].z;

            }
            if (points[i].z < minZ)
            {
                minZ = points[i].z;

            }
        }

        float middlepointX = (maxX + minX) / 2;
        float middlepointY = (maxY + minY) / 2;
        float middlepointZ = (maxZ + minZ) / 2;

        return new Vector3(middlepointX, middlepointY, middlepointZ);
    }

    public void RotatePoints(int RotationAngle)
    {
        Quaternion q = Quaternion.Euler(0, RotationAngle, 0);
        Vector3 pivot = GetCenterPivot();

        for (int i = 0; i < points.Count; i++)
        {
            points[i] = q * (points[i] - pivot) + pivot;
        }
    }

    


}
