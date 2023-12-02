/**
 * 
 * PathObjectEditorData holds all data for the PathObjectEditor class that needs to be stored
 * Since Unity uses a Immediate Mode GUI (IMGUI) model, the PathObjectEditor is instantiated each time the user
 * selects the corresponding GameObject and thus, some data needs to be stored in another class. 
 * 
 * Author: Magnus Söderberg
 * Last updated 2022-03-22
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathObjectEditorData
{
    [SerializeField]
    List<bool> foldOutSegmentDataInInspector;
    public string[] extrusionAxisArray = new string[] { "Z", "X" };
    [SerializeField]
    public int extrudeAlongAxisComboBoxSelectionIndex = 0;
    [SerializeField]
    public int ExtrudeAlongAxisCB;
    [SerializeField]
    public bool displaySegmentNumbers = true;
    [SerializeField]
    public bool updateMeshContinuously = false;
    [SerializeField]
    public bool drawNormals = false;
    [SerializeField]
    public bool drawTangents = false;
    [SerializeField]
    public bool drawBinormals = false;
    [SerializeField]
    public int straightenSegmentIndex;


    public PathObjectEditorData(int numSegments)
    {
        foldOutSegmentDataInInspector = new List<bool>();

        for (int i = 0; i < numSegments; i++)
        {
            foldOutSegmentDataInInspector.Add(false);
        }

    }

    /// <summary>
    /// This enables the use of indexing the object such as pathObjectEditorData[i]
    /// </summary>

    public bool this[int index]
    {
        get
        {
            if (index == foldOutSegmentDataInInspector.Count)
            {
                foldOutSegmentDataInInspector.Add(false);
            }
            return foldOutSegmentDataInInspector[index];
        }
        set
        {
            if (index == foldOutSegmentDataInInspector.Count)
            {
                foldOutSegmentDataInInspector.Add(value);
            }
            else foldOutSegmentDataInInspector[index] = value;
        }

    }

}
