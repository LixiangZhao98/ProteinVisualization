/*
	Created by Carl Emil Carlsen.
	Copyright 2017 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(SplineMaker))]
public class SplineMakerInspector : Editor
{
	SplineMaker _spline;

	SerializedProperty _pointsPerSegment;
	SerializedProperty _loop;
	SerializedProperty _anchorPoints;
	SerializedProperty _onUpdated;

	ReorderableList _anchorPointsList;


	void OnEnable()
	{
		_spline = target as SplineMaker;

		_pointsPerSegment = serializedObject.FindProperty( "_pointsPerSegment" );
		_loop = serializedObject.FindProperty( "_loop" );
		_onUpdated = serializedObject.FindProperty( "_onUpdated" );
		_anchorPoints = serializedObject.FindProperty( "_anchorPoints" );

		_anchorPointsList = new ReorderableList( serializedObject, _anchorPoints, true, true, true, true);
		_anchorPointsList.drawElementCallback =  
			( Rect rect, int index, bool isActive, bool isFocused ) => {
			SerializedProperty pointProp = _anchorPointsList.serializedProperty.GetArrayElementAtIndex( index );
			rect.y += 2;
			rect.height = EditorGUIUtility.singleLineHeight;
			pointProp.vector3Value = EditorGUI.Vector3Field( rect, "Anchor " + index, pointProp.vector3Value );
		};
		_anchorPointsList.drawHeaderCallback = (Rect rect) => {  
			EditorGUI.LabelField(rect, "Anchors");
		};
	}

	
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField( _pointsPerSegment );
		EditorGUILayout.PropertyField( _loop );
		_anchorPointsList.DoLayoutList();
		EditorGUILayout.PropertyField( _onUpdated );

		serializedObject.ApplyModifiedProperties();
	}


	void OnSceneGUI()
	{
		serializedObject.Update();

		for( int a = 0; a < _anchorPoints.arraySize; a++ ){
			SerializedProperty pointProp = _anchorPointsList.serializedProperty.GetArrayElementAtIndex( a );
			Vector3 worldPoint = _spline.transform.TransformPoint( pointProp.vector3Value );
			worldPoint = Handles.DoPositionHandle( worldPoint, Quaternion.identity );
			pointProp.vector3Value = _spline.transform.InverseTransformPoint( worldPoint );
		}

		Vector3[] points = _spline.points;
		Handles.color = Color.magenta;

		if( points != null && points.Length > 1 ){
			float size = ( points[0] - points[1] ).magnitude * 0.2f;
			foreach( Vector3 point in points ){
				Handles.SphereHandleCap( -99999, point, Quaternion.identity, size, EventType.Repaint );
			}
		}

		serializedObject.ApplyModifiedProperties();
	}

}