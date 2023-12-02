/**
 * 
 * PathObjectEditor is instantiated whenever the user selects a GameObject which has a PathObject script component attached to it.
 * The class shows the custom inspector for a PathObject where the user can see and change the settings of the path. 
 * The class is also responsible for drawing the Bezier curve in the scene.
 * 
 * Author: Magnus Söderberg
 * Last updated 2022-03-22
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(PathObject))]

public class PathObjectEditor : Editor
{
    private PathObjectEditorData data;
    private PathObject pathObject;
    private Bezier bezier;

    private Vector3 lastPos;

    private GUIStyle sectionTitleStyle;
    private GUIStyle pathObjectTitleStyle;
    private GUIStyle SegmentTextStyle;
    private GUIStyle largerFontButton;

    void OnEnable()
    {
        pathObject = (PathObject)target;

        if (pathObject.bezier == null)
        {
            pathObject.CreateNewPath();
        }

        data = pathObject.editorData;
        bezier = pathObject.bezier;
        lastPos = pathObject.transform.position;

        //Put the PathObject component at the top
        UnityEditorInternal.ComponentUtility.MoveComponentUp((PathObject)target);

        //PathObject Title  in green
        pathObjectTitleStyle = new GUIStyle();
        pathObjectTitleStyle.fontStyle = FontStyle.Bold;
        pathObjectTitleStyle.fontSize = 18;
        pathObjectTitleStyle.alignment = TextAnchor.UpperCenter;
        pathObjectTitleStyle.normal.textColor = Color.green;

        //Other section titles in white
        sectionTitleStyle = new GUIStyle();
        sectionTitleStyle.fontStyle = FontStyle.Bold;
        sectionTitleStyle.fontSize = 16;
        sectionTitleStyle.normal.textColor = Color.white;

        //The segment numbers indicating the start of a segment
        SegmentTextStyle = new GUIStyle();
        SegmentTextStyle.fontStyle = FontStyle.Bold;
        SegmentTextStyle.fontSize = 34;


    }

    void OnSceneGUI()
    {
        if (data.updateMeshContinuously)
            pathObject.UpdateMesh();

        Input();
        DrawBezierCurve();

        if (data.displaySegmentNumbers)
            ShowSegmentNumbers();

    }

    public override void OnInspectorGUI()
    {

        //For the "Update Mesh" and "Update Collider" buttons
        largerFontButton = new GUIStyle(GUI.skin.button);
        largerFontButton.fontSize = 14;

        DrawDefaultInspector();

        //PathObject Title
        GUILayout.Space(10);

        //Separator in inspector
        var rect0 = EditorGUILayout.BeginHorizontal();
        Handles.DrawLine(new Vector2(rect0.x - 15, rect0.y), new Vector2(rect0.width + 15, rect0.y));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Path Object", pathObjectTitleStyle);
        GUILayout.Space(10);


        //Choose Mesh
        EditorGUILayout.BeginHorizontal();
        Mesh currentMesh = pathObject.mesh;
        try
        {
            pathObject.mesh = (Mesh)EditorGUILayout.ObjectField("Mesh", pathObject.mesh, typeof(Mesh), true);
            if(currentMesh != pathObject.mesh && pathObject.mesh.GetType().Name.Equals("Mesh") && pathObject.material != null) {
                pathObject.UpdateNewMesh();
            }

        }
        catch (ExitGUIException ex) { ex.ToString(); } // this exception is thrown by Unity so must handle it
        catch (Exception e)
        {
            Debug.LogError("MESH ASSIGNMENT ERROR " + e.ToString());
            pathObject.mesh = null;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        //Choose Material
        EditorGUILayout.BeginHorizontal();
        Material currentMaterial = pathObject.material;
        try
        {
            pathObject.material = (Material)EditorGUILayout.ObjectField("Material", pathObject.material, typeof(Material), true);

            if (currentMaterial != pathObject.material && pathObject.material.GetType().Name.Equals("Material") && pathObject.mesh != null)
            {
                pathObject.UpdateNewMesh();
            }
        }
        catch (ExitGUIException ex) { ex.ToString(); }
        catch (Exception e)
        {
            Debug.LogError("MATERIAL ASSIGNMENT ERROR " + e.ToString());
            pathObject.material = null;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        //Choose extrusion axis
        EditorGUILayout.BeginHorizontal();

        int ExtrudeAlongAxisIndex = EditorGUILayout.Popup("Extrude Mesh Along Axis", data.ExtrudeAlongAxisCB, data.extrusionAxisArray);
        if (ExtrudeAlongAxisIndex != data.ExtrudeAlongAxisCB)
        {
            data.ExtrudeAlongAxisCB = ExtrudeAlongAxisIndex;
            pathObject.ChangeExtrusionAxis(data.extrusionAxisArray[data.ExtrudeAlongAxisCB]);
            if (pathObject.mesh != null)
            {
                pathObject.UpdateNewMesh();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Vertices count: " + pathObject.VertCount);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(20);

        //-- PATH -- //

        GUILayout.Label("Path", sectionTitleStyle);

        var rect = EditorGUILayout.BeginHorizontal();
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (GUILayout.Button(new GUIContent("Update Mesh", "Manually updates the mesh according to any changes. Left Mouse Button + Ctrl does the same thing"), largerFontButton))
        {
            pathObject.UpdateMesh();
        }

        GUILayout.Space(10);

        data.updateMeshContinuously = GUILayout.Toggle(data.updateMeshContinuously, "Update Mesh Continuously (not recommended)");

        GUILayout.Space(10);

        data.drawNormals = GUILayout.Toggle(data.drawNormals, "Draw Normals (blue)");

        GUILayout.Space(10);

        data.drawTangents = GUILayout.Toggle(data.drawTangents, "Draw Tangents (red)");

        GUILayout.Space(10);

        data.drawBinormals = GUILayout.Toggle(data.drawBinormals, "Draw Binormals (cyan)");


        GUILayout.Space(10);

        data.displaySegmentNumbers = GUILayout.Toggle(data.displaySegmentNumbers, "Show Segment Numbers");

        GUILayout.Space(10);

        float rotation = pathObject.RotationAngle;
        
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Rotate Path"))
        {
            pathObject.RotatePath();
            if(pathObject.mesh != null) 
                pathObject.UpdateMesh();
        }
        pathObject.RotationAngle = EditorGUILayout.IntSlider("Rotation Angle", pathObject.RotationAngle, 0, 360);

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (GUILayout.Button(new GUIContent("Add Segment", "Adds another segment to the end of the existing path. Shift+left mouse does the same thing")))
        {
            Undo.RecordObject(pathObject, "Add Segment");

            bezier.AddSegment();

            if(pathObject.mesh != null)
                pathObject.UpdateMesh();

            
        }
        GUILayout.Space(10);

        if (GUILayout.Button(new GUIContent("Smooth all Control Points", "Makes the path smooth at segment intersections")))
        {
            for (int i = 1; i < bezier.NumSegments; i++)
            {
                bezier.StraightenControlPoints(i);
            }
            pathObject.UpdateMesh();
        }


        GUILayout.Space(10);



        if (GUILayout.Button("Straighten path along Y-axis"))
        {
            Undo.RecordObject(pathObject, "Straighten Path");
            bezier.StraightenCurve("Y");
            pathObject.UpdateMesh();
        }
        if (GUILayout.Button("Straighten path along Z-axis"))
        {
            Undo.RecordObject(pathObject, "Straighten Path");
            bezier.StraightenCurve("Z");
            pathObject.UpdateMesh();
        }

        if (GUILayout.Button("Straighten path along X-axis"))
        {
            Undo.RecordObject(pathObject, "Straighten Path");
            bezier.StraightenCurve("X");
            pathObject.UpdateMesh();
        }

            GUILayout.Space(10);

        // -- MESH COLLIDER -- //

        GUILayout.Label("Mesh Collider", sectionTitleStyle);

        var lineMeshCollider = EditorGUILayout.BeginHorizontal();
        Handles.DrawLine(new Vector2(lineMeshCollider.x - 15, lineMeshCollider.y), new Vector2(lineMeshCollider.width + 15, lineMeshCollider.y));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (GUILayout.Button(new GUIContent("Update Collider", "Manually updates the mesh collider"), largerFontButton))
            pathObject.UpdateCollider();

        GUILayout.Space(10);

        bool enableCollider = GUILayout.Toggle(pathObject.UseMeshCollider, "Enable Mesh Collider");

        if (enableCollider != pathObject.UseMeshCollider)
        {
            pathObject.UseMeshCollider = enableCollider;

            if (enableCollider)
            {
                pathObject.UpdateCollider();
            }
            else
            {
                pathObject.DisableCollider();
            }
        }


        GUILayout.Space(10);

        pathObject.CustomWidthOfCollider = EditorGUILayout.FloatField("Width of collider", pathObject.CustomWidthOfCollider);

        GUILayout.Space(10);
        
        pathObject.CustomHeightOfCollider = EditorGUILayout.FloatField("Height of collider", pathObject.CustomHeightOfCollider);

        GUILayout.Space(10);
        
        if (GUILayout.Button("Reset collider"))
            pathObject.ResetCollider();



        // -- SEGMENTS -- //

        GUILayout.Space(20);
        GUILayout.Label("Segments", sectionTitleStyle);
        var rect3 = EditorGUILayout.BeginHorizontal();
        Handles.DrawLine(new Vector2(rect3.x - 15, rect3.y), new Vector2(rect3.width + 15, rect3.y));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        for (int i = 0; i < bezier.NumSegments; i++)
        {
            //Foldout for each segment
            data[i] = EditorGUILayout.Foldout(data[i], "Segment " + (i + 1), true);

            if (data[i])
            {
                GUILayout.Space(10);
                
                Vector3 currentPosOfSegment = bezier.GetPointsInSegment(i)[0];
                Vector3 newSegmentPos = EditorGUILayout.Vector3Field("Position of start of segment", currentPosOfSegment);
                if(newSegmentPos != currentPosOfSegment)
                {
                    bezier.MoveStartingPositionOfSegment(i, newSegmentPos);
                }

                GUILayout.Space(20);
                
                if (GUILayout.Button("Straighten segment along X-axis"))
                {
                    bezier.StraightenSegment(i, "X");
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Straighten segment along Y-axis"))
                {
                    bezier.StraightenSegment(i, "Y");
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Straighten segment along Z-axis"))
                {
                    bezier.StraightenSegment(i, "Z");
                }

                GUILayout.Space(10);

                if (i != 0)
                {

                    GUILayout.Space(10);
                    if (GUILayout.Button("Straighten Control Points"))
                    {
                        bezier.StraightenControlPoints(i);

                        pathObject.UpdateMesh();
                    }
                }


                GUILayout.Space(30);

                // REMOVE SEGMENT

                if(bezier.NumSegments > 1)
                {
                    if (GUILayout.Button("Remove Segment"))
                    {
                        bezier.RemoveSegment(i);
                        pathObject.UpdateMesh();
                    }
                }

                GUILayout.Space(30);
            }


                GUILayout.Space(20);

        }

        EditorUtility.SetDirty(pathObject);

    }

    void ShowSegmentNumbers()
    {
        Vector3[] segmentPoints = bezier.GetAnchorPoints(); 

        for (int i = 0; i < segmentPoints.Length - 1; i++)
        {
            Handles.Label(segmentPoints[i] + new Vector3(0, 10, 0), "" + (i + 1), SegmentTextStyle);
        }

    }


    //Hold shift+left mouse button to create a new segment on the path
    void Input()
    {

        Event guiEvent = Event.current;
        Ray mouseInput = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        //Press Left Mouse Button + Shift to add a new Segment
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(pathObject, "Add Segment");

            bezier.AddSegment();

            if (pathObject.mesh != null)
                pathObject.UpdateMesh();

        }

        //Press Left Mouse Button + Control to Update the Mesh
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control)
        {
            if (pathObject.mesh != null)
                pathObject.UpdateMesh();

        }

    }

    /// <summary>
    /// Draws the path in the scene including the control points and move handles
    /// </summary>
    public void DrawBezierCurve()
    {

        Vector3 currentPos = pathObject.transform.position;
        if (currentPos != lastPos)
        {
            bezier.PositionChanged(currentPos - lastPos);
        }
        lastPos = currentPos;


        //Draw lines (in black) between control points (red spheres)
        //Draw the bezier curve (in green) between all points 
 
        Handles.color = Color.black;
        
        for (int i = 0; i < bezier.NumSegments; i++)
        {
            Vector3[] points = bezier.GetPointsInSegment(i);

            Handles.DrawLine(points[1], points[0]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 5);

        }

        for (int i = 0; i < bezier.NumPoints; i++)
        {
            Vector3 newPos = new Vector3();

            Handles.color = Color.red;

            //Adds Position handle at the beginning of each section
            if (i % 3 == 0)
            {
                newPos = Handles.PositionHandle(bezier.singlePoint(i), Quaternion.identity);

            }
            //Adds a red sphere at remaining points
            else
            {
                newPos = Handles.FreeMoveHandle(bezier.singlePoint(i), Quaternion.identity, 4f, Vector3.zero, Handles.SphereHandleCap);
            }

            if (bezier.singlePoint(i) != newPos)
            {
                Undo.RecordObject(pathObject, "Move Point");
                bezier.MovePoint(i, newPos);

            }

        }

    }


}
