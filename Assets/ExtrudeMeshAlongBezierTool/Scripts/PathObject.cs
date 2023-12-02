/**
 * 
 * Attach the PathObject script to an empty GameObject in Unity to create a path
 * 
 * PathObject is a class that holds reference to the B�zier curve, i.e, all the points that make up the total path
 * as well as the mesh that is extruded along the path.
 * This class is responsible for reading the mesh data, obtaining all necessary vectors and finally 
 * extruding the mesh along the B�zier curve.  
 * 
 * Author: Magnus S�derberg
 * magnus.c.soderberg@gmail.com
 * Last updated 2022-03-24
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[ExecuteInEditMode]

[Serializable]
public class PathObject : MonoBehaviour
{
    [SerializeField, HideInInspector]
    public PathObjectEditorData editorData;

    [SerializeField, HideInInspector]
    public Bezier bezier;

    [SerializeField, HideInInspector]
    public Mesh mesh;

    [SerializeField, HideInInspector]
    public Material material;

    [SerializeField, HideInInspector]
    private List<float> distinctVertexPositions;
    [SerializeField, HideInInspector]
    private List<Vector3> verticesOffset;

    [SerializeField, HideInInspector]
    private List<Vector3> tangentsAveraged;
    [SerializeField, HideInInspector]
    private List<Vector3> tangents;
    [SerializeField, HideInInspector]
    private List<Vector3> points;
    [SerializeField, HideInInspector]
    private List<Vector3> binorms;
    [SerializeField, HideInInspector]
    private List<Vector3> normals;

    [SerializeField, HideInInspector]
    public int VertCount;

    //Measurements of the mesh to be extruded
    [SerializeField, HideInInspector]
    public float MeshWidth;
    [SerializeField, HideInInspector]
    private float MeshLength;
    [SerializeField, HideInInspector]
    private float MeshHeight;

    //Measurements of the mesh collider
    [SerializeField, HideInInspector]
    public float CustomWidthOfCollider;
    [SerializeField, HideInInspector]
    public float CustomHeightOfCollider;
    [SerializeField, HideInInspector]
    public bool UseMeshCollider = true;
    [SerializeField, HideInInspector]
    public bool NewMeshNeedsUpdate;

    [SerializeField, HideInInspector]
    public int RotationAngle;

    [SerializeField, HideInInspector]
    public ExtrusionAxis extrusionAxis = ExtrusionAxis.Z;

    //Enum to keep track of whether the mesh is extruded along the Z or X axis. 
    public enum ExtrusionAxis
    {
        Z,
        X
    }

    public void RotatePath()
    {
        bezier.RotatePoints(RotationAngle);
    }

    public void ChangeExtrusionAxis(string axis)
    {
        if(axis.Equals("X"))
        {
            extrusionAxis = ExtrusionAxis.X;
        }
        else if(axis.Equals("Z"))
        {
            extrusionAxis = ExtrusionAxis.Z;
        }
    }

    public void CreateNewPath()
    {

        bezier = new Bezier(transform.position);
        editorData = new PathObjectEditorData(bezier.NumSegments);

        //Update the path if changes are made to the position of the points of the path
        bezier.UpdateMeshEvent += UpdateMesh;
    }


    #region EXTRUDE_MESH

    /// <summary>
    /// If a new Mesh has been selected (or the extrusion axis has changed) the mesh data needs to be obtained again
    /// </summary>
    public void UpdateNewMesh()
    {
        if (mesh == null)
        {
            Debug.LogError("No mesh chosen");
            return;
        }

        GetNewMeshData();
        UpdateMesh();

        //Get the measurements of the mesh
        //Get each distinct distance in the mesh along the extrusion axis 
        void GetNewMeshData()
        {

            //Find the minimum value of the mesh along the extrusion axis so that the mesh starts on 0 in that direction
            float minMeshValue = float.MaxValue;
            verticesOffset = new List<Vector3>();
            distinctVertexPositions = new List<float>();

            float widthOfMeshMinValue = float.MaxValue;
            float widthOfMeshMaxValue = float.MinValue;

            float heightOfMeshMinValue = float.MaxValue;
            float heightOfMeshMaxValue = float.MinValue;

            //Determine the width, height and length of mesh
            foreach (var vert in mesh.vertices)
            {
                Vector3 p = vert;
                float vertexValue = 0;
                float vertexValueWidthAxis = 0;

                if (extrusionAxis.Equals(ExtrusionAxis.Z))
                {
                    vertexValue = p.z;
                    vertexValueWidthAxis = p.x;
                }
                else if (extrusionAxis.Equals(ExtrusionAxis.X))
                {
                    vertexValue = p.x;
                    vertexValueWidthAxis = p.z;
                }
                minMeshValue = Mathf.Min(minMeshValue, vertexValue); //for each vertex check if it is smaller then previously obtained value

                widthOfMeshMinValue = Mathf.Min(widthOfMeshMinValue, vertexValueWidthAxis);
                widthOfMeshMaxValue = Mathf.Max(widthOfMeshMaxValue, vertexValueWidthAxis);

                heightOfMeshMinValue = Mathf.Min(heightOfMeshMinValue, p.y);
                heightOfMeshMaxValue = Mathf.Max(heightOfMeshMaxValue, p.y);
            }

            //Determine all distinct vertex distances in the mesh along the extrusion axis
            //Offset the mesh vertices so they have their first position at 0 in the extrusion axis
            foreach (var vert in mesh.vertices)
            {
                float offsetValue = 0;
                Vector3 vertOffset = vert;

                if (extrusionAxis.Equals(ExtrusionAxis.Z))
                {
                    offsetValue = (float)Mathf.Round((vert.z - minMeshValue) * 10000) / 10000;
                    vertOffset.z = offsetValue;
                }
                else if (extrusionAxis.Equals(ExtrusionAxis.X))
                {
                    offsetValue = (float)Mathf.Round((vert.x - minMeshValue) * 10000) / 10000;
                    vertOffset.x = offsetValue;
                }

                distinctVertexPositions.Add(offsetValue);
                verticesOffset.Add(vertOffset);
            }

            distinctVertexPositions = distinctVertexPositions.Distinct().ToList();
            distinctVertexPositions.Sort();

            //For mesh collider
            MeshLength = distinctVertexPositions[distinctVertexPositions.Count - 1];
            MeshHeight = Mathf.Abs(heightOfMeshMinValue) + Mathf.Abs(heightOfMeshMaxValue);
            MeshWidth = Mathf.Abs(widthOfMeshMinValue) + Mathf.Abs(widthOfMeshMaxValue);

            CustomWidthOfCollider = MeshWidth;
            CustomHeightOfCollider = MeshHeight;

        }
    }
    /// <summary>
    /// Extrudes the mesh along the path
    /// </summary>
    public void UpdateMesh()
    {
        if (mesh == null)
        {
            Debug.LogError("No Mesh chosen");
            return;
        }
            
        if (material == null)
        {
            Debug.LogWarning("No material chosen");
        }

        GetPointsAndTangents();
        PlaceMeshesAlongPath();

        if (UseMeshCollider)
            UpdateCollider();

        //Determine all world positions and related vectors needed to extrude the mesh along the path
        void GetPointsAndTangents()
        {
            points = new List<Vector3>();
            tangents = new List<Vector3>();
            binorms = new List<Vector3>();
            normals = new List<Vector3>();

            //Get references points along for each segment (one segment is one Bezier curve)
            List<float[]> evenlySpacedReferenceLengths = bezier.ReferenceLengths();

            //Get the length of each segment
            List<float> segmentLengths = new List<float>();
            foreach (var segment in evenlySpacedReferenceLengths)
            {
                segmentLengths.Add(segment[segment.Length - 1]);
            }

            //Get all distances along the path where vertices should be placed.
            List<float[]> distances = bezier.MeshLengthsOnCurve(distinctVertexPositions, segmentLengths);

            List<float[]> tvalues = new List<float[]>();

            //Calculate t-values for each unique vertex point along the path
            for (int i = 0; i < evenlySpacedReferenceLengths.Count; i++)
            {
                List<float> t = new List<float>();
                for (int j = 0; j < distances[i].Length; j++)
                {
                    t.Add(bezier.CalculatetValue(evenlySpacedReferenceLengths[i], distances[i][j]));
                }
                tvalues.Add(t.ToArray());
            }

            //Get the world position and corresponding tangent on the path needed
            for (int i = 0; i < bezier.NumSegments; i++)
            {
                int numPoints = 0;
                Vector3[] p = bezier.GetPointsInSegment(i);
                for (int j = 0; j < tvalues[i].Length; j++)
                {

                    points.Add(bezier.GetPoint(p, tvalues[i][j]));
                    tangents.Add(bezier.GetTangent(p, tvalues[i][j]));
                    numPoints++;
                }

            }

            //Average the tangents between neighboring points so the mesh lies flat on the path
            tangentsAveraged = new List<Vector3>();

            for (int i = 0; i < tangents.Count; i++)
            {
                if (i == 0 || i == tangents.Count - 1)
                {
                    tangentsAveraged.Add(tangents[i]);
                }
                else tangentsAveraged.Add(tangents[i - 1] + tangents[i]);

                tangentsAveraged[i] = tangentsAveraged[i].normalized;
            }

            tangents = tangentsAveraged;

            //Binormals
            for (int i = 0; i < tangents.Count; i++)
            {
                binorms.Add(GetBinormal(tangents[i]));
            }

            //Normals
            for (int i = 0; i < binorms.Count; i++)
            {
                normals.Add(GetNormal(tangents[i], binorms[i]));
            }

        }
        
        void PlaceMeshesAlongPath()
        {
            List<Vector3> verticesOfTotalMesh = new List<Vector3>();
            List<Vector3> normalsOfTotalMesh = new List<Vector3>();
            Quaternion[] orientation = new Quaternion[tangents.Count];
            Vector3[] verticesWithExtrusionAxisZero = new Vector3[verticesOffset.Count];

            //Populate an array that tells us at what distance each vertex should be placed
            int[] distanceIndexOfEachVertex = new int[verticesOffset.Count];

            for (int i = 0; i < verticesOffset.Count; i++)
            {
                float vertexValue = 0;

                if (extrusionAxis.Equals(ExtrusionAxis.Z))
                {
                    vertexValue = verticesOffset[i].z;

                }
                else if (extrusionAxis.Equals(ExtrusionAxis.X))
                {
                    vertexValue = verticesOffset[i].x;
                }

                for (int j = 0; j < distinctVertexPositions.Count; j++)
                {
                    if (Mathf.Abs(vertexValue - distinctVertexPositions[j]) < 0.001f)
                    {  // To avoid problems with rounding errors 
                        distanceIndexOfEachVertex[i] = j;
                    }

                }
            }

            //Get the orientation of each point on the path, i.e, the quaternion that is used to rotate the mesh vertex along the path
            for (int i = 0; i < tangents.Count; i++)
            {
                orientation[i] = GetOrientation(tangents[i], binorms[i]);
            }

            //Populate an array that is same as mesh.vertices except is has 0 component in the extrusion axis
            //Makes it easier to determine the displacement of each vertex on the path
            for (int i = 0; i < verticesOffset.Count; i++)
            {
                verticesWithExtrusionAxisZero[i] = (verticesOffset[i]);

                if (extrusionAxis.Equals(ExtrusionAxis.Z))
                {
                    verticesWithExtrusionAxisZero[i].z = 0;
                }
                else if (extrusionAxis.Equals(ExtrusionAxis.X))
                {
                    verticesWithExtrusionAxisZero[i].x = 0;

                }

            }

            int meshNumber = 0;

            //Determine the world position and corresponding normal of each point of the total mesh on the path
            for (int i = 0; i < bezier.NumSegments; i++)
            {
                //For every mesh on current segment
                for (int k = meshNumber; k < meshNumber + bezier.NumMeshesPerSegment[i]; k++)
                {
                    for (int j = 0; j < verticesOffset.Count; j++)
                    {
                        //The distance in the extrusion axis of this vertex
                        int d = distanceIndexOfEachVertex[j];

                        //World position of current vertex on the path
                        Vector3 newPos = orientation[d + k * (distinctVertexPositions.Count - 1)] * verticesWithExtrusionAxisZero[j] + points[d + k * (distinctVertexPositions.Count - 1)] - gameObject.transform.position; 
                        verticesOfTotalMesh.Add(newPos);

                        //Normal of current vertex
                        Vector3 newNorm = orientation[d + k * (distinctVertexPositions.Count - 1)] * mesh.normals[j];                                                                                 
                        normalsOfTotalMesh.Add(newNorm);

                    }

                }

                meshNumber += bezier.NumMeshesPerSegment[i];
            }


            //Triangles
            List<int> tris = new List<int>();

            for (int i = 0; i < bezier.NumMeshes; i++)
            {
                for (int j = 0; j < mesh.triangles.Length; j++)
                {
                    tris.Add(mesh.triangles[j] + verticesOffset.Count * i);
                }
            }

            //The UVs
            List<Vector2> UVs = new List<Vector2>();

            for (int i = 0; i < bezier.NumMeshes; i++)
            {
                for (int j = 0; j < mesh.uv.Length; j++)
                {
                    UVs.Add(mesh.uv[j]);
                }

            }

            //Make the total mesh
            Mesh newMesh = new Mesh();
            newMesh.Clear();

            newMesh.vertices = verticesOfTotalMesh.ToArray();
            newMesh.normals = normalsOfTotalMesh.ToArray();

            newMesh.uv = UVs.ToArray();
            newMesh.triangles = tris.ToArray();
            GetComponent<MeshFilter>().sharedMesh = newMesh;
            GetComponent<MeshRenderer>().material = material;

            VertCount = verticesOfTotalMesh.Count;
        }

    }
    #endregion

    #region MESH_COLLIDER
    /// <summary>
    /// The collider extrudes along the path just as the mesh does. The mesh collider consists of
    /// cube meshes linked together.
    /// </summary>
    public void UpdateCollider()
    {

        if (UseMeshCollider)
        {
            Mesh cubeMesh;
            int[] cubeTris;
            Vector3[] cubeVertsAdjusted;
            Vector3[] cubeVerts;

            List<Vector3> colliderPoints = new List<Vector3>();
            List<Vector3> colliderTangents = new List<Vector3>();
            List<Vector3> colliderVerts = new List<Vector3>();
            List<int> colliderTris = new List<int>();
            List<Quaternion> colliderOrientations = new List<Quaternion>();

            GetMeshColliderData();
            ExtendMeshColliderAlongMesh();
            MakeMeshCollider();


            void GetMeshColliderData()
            {

                //Create a cube to get the vertices of a cube
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                MeshFilter mf = cube.GetComponent<MeshFilter>();
                Mesh mymesh = mf.sharedMesh;
                cubeVerts = mymesh.vertices;
                cubeTris = mymesh.triangles;
                DestroyImmediate(cube);

                cubeVertsAdjusted = new Vector3[cubeVerts.Length];

                float lengthOfCube = MeshLength;
                float widthOfCube = MeshWidth;
                float heightOfCube = MeshHeight;

                //If the user has changed the size of the mesh collider 
                if(CustomWidthOfCollider != MeshWidth)
                {
                    widthOfCube = CustomWidthOfCollider;
                }
                if (CustomHeightOfCollider != MeshHeight)
                {
                    heightOfCube = CustomWidthOfCollider;
                }

                //Set the size of one cube mesh collider
                //The collider cube is extruded along the z-axis
                for (int i = 0; i < cubeVerts.Length; i++)
                {
                    // Set the length
                    if (cubeVerts[i].z > 0)
                        cubeVertsAdjusted[i].z = lengthOfCube;
                    else if (cubeVerts[i].z < 0)
                        cubeVertsAdjusted[i].z = 0;
                    // Set the width
                    if (cubeVerts[i].x > 0)
                        cubeVertsAdjusted[i].x = widthOfCube/2;
                    else if (cubeVerts[i].x < 0)
                        cubeVertsAdjusted[i].x = -widthOfCube/2;
                    //Set the height
                    if (cubeVerts[i].y > 0)
                        cubeVertsAdjusted[i].y = heightOfCube/2;
                    else if (cubeVerts[i].y < 0)
                        cubeVertsAdjusted[i].y = -heightOfCube/2;

                }

            }

            void ExtendMeshColliderAlongMesh()
            {

                int numOfMeshes = 0;

                //Find out how many meshes there are
                for (int i = 0; i < bezier.NumSegments; i++)
                {
                    numOfMeshes += bezier.NumMeshesPerSegment[i];  
    
                }

                //Skip all other points on the path other than the first and last point of each mesh
                int skip = distinctVertexPositions.Count - 1;
                int pointsToAdd = 0;

                //Add all world points and tangents needed for the mesh collider
                for (int i = 0; i < bezier.NumSegments; i++)
                {
                    for (int j = 0; j < bezier.NumMeshesPerSegment[i]; j++)
                    {
                        colliderPoints.Add(points[pointsToAdd]);
                        colliderTangents.Add(tangentsAveraged[pointsToAdd]);
                        
                        pointsToAdd += skip;
                    }
                }

                //Get the orientation
                for (int i = 0; i < colliderTangents.Count; i++)
                {
                    colliderOrientations.Add(GetOrientation(colliderTangents[i]));
                }


                //Get the new vertices
                for (int i = 0; i < colliderPoints.Count; i++)
                {
                    for (int j = 0; j < cubeVertsAdjusted.Length; j++)
                    {
                        Vector3 newPos = colliderOrientations[i] * cubeVertsAdjusted[j] + colliderPoints[i] - gameObject.transform.position;
                        colliderVerts.Add(newPos);
                    }

                }

                //Get new triangles
                for (int i = 0; i < numOfMeshes; i++)
                {
                    for (int j = 0; j < cubeTris.Length; j++)
                    {
                        colliderTris.Add(cubeTris[j] + i * cubeVerts.Length);
                    }
                }
                
            }

            void MakeMeshCollider()
            {
                cubeMesh = new Mesh();
                cubeMesh.vertices = colliderVerts.ToArray();
                cubeMesh.triangles = colliderTris.ToArray();
                MeshCollider meshCol = GetComponent<MeshCollider>();
                meshCol.sharedMesh = cubeMesh;
            }

        }
        else
        {
            GetComponent<MeshCollider>().sharedMesh = null;
        }

    }

    public void DisableCollider()
    {
        MeshCollider meshCol = GetComponent<MeshCollider>();
        meshCol.sharedMesh = null;
    }

    public void ResetCollider()
    {
        CustomWidthOfCollider = MeshWidth;
        CustomHeightOfCollider = MeshHeight;
        UpdateCollider();
    }

    #endregion

    #region HELPER_METHODS
    Quaternion GetOrientation(Vector3 tng, Vector3 binorm)
    {
        Vector3 normal = GetNormal(tng, binorm);  // This is causing the upside down stuff
        if (extrusionAxis.Equals(ExtrusionAxis.Z))
        {
            return Quaternion.LookRotation(tng, normal);

        }
        else if (extrusionAxis.Equals(ExtrusionAxis.X))
        {
            return Quaternion.LookRotation(-binorm, normal);
        }

        return Quaternion.identity;

    }

    Quaternion GetOrientation(Vector3 tng)
    {
        Vector3 binorm = Vector3.Cross(tng, Vector3.up);
        Vector3 nrm = GetNormal(tng, binorm);

        return Quaternion.LookRotation(tng, nrm);

    }

    Vector3 GetBinormal(Vector3 tangent)
    {
        return Vector3.Cross(Vector3.up, tangent).normalized;
    }

    Vector3 GetNormal(Vector3 tng, Vector3 binorm)
    {
        return Vector3.Cross(tng, binorm).normalized;
    }

    #endregion

    /// <summary>
    /// Draws the tangents, normals and binormals of each point on the Bezier curve
    /// </summary>
    public void OnDrawGizmos()
    {

        if (editorData.drawNormals)
        {
            for (int i = 0; i < normals.Count; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(points[i], points[i] + normals[i] * 15f);
                
            }
        }
        if(editorData.drawTangents)
        {
            for (int i = 0; i < tangents.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(points[i], points[i] + tangents[i] * 15f);
            }
        }

        if (editorData.drawBinormals)
        {
            for (int i = 0; i < binorms.Count; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(points[i], points[i] + binorms[i] * 15f);
            }
        }


    }






}


