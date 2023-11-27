/*
	TubeRendererInspector.cs
	Created by Carl Emil Carlsen.
	Copyright 2012+ Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
	
	Part of the TubeRenderer Asset Store files.
*/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TubeRenderer))]
public class TubeRendererInspector : Editor
{
	TubeRenderer tube;

	bool isDirty;

	Color disabledColor = new Color( 0.7f, 0.7f, 0.7f );

	SerializedProperty _points;
	SerializedProperty _radiuses;
	SerializedProperty _radius;
	SerializedProperty _pointColors;
    SerializedProperty _twists;
	SerializedProperty _edgeCount;
	SerializedProperty _calculateTangents;
	SerializedProperty _invertMesh;
	SerializedProperty _normalMode;
	SerializedProperty _caps;
	SerializedProperty _uvRect;
	SerializedProperty _uvRectCap;
	SerializedProperty _uvRectCapEndMirrored;
    SerializedProperty _uvAntiStretching;
	SerializedProperty _forwardAngleOffset;
	SerializedProperty _showMeshGizmos;
	SerializedProperty _meshGizmoLength;
	SerializedProperty _showRotationGizmos;
	SerializedProperty _rotationGizmoLength;
	SerializedProperty _pointsFoldout;
	SerializedProperty _radiusesFoldout;
	SerializedProperty _colorsFoldout;
    SerializedProperty _twistsFoldout;
	SerializedProperty _uvFoldout;

	const int meshVertexCountLimit = 65000;
	const string messageHeader = "<b>[TubeRenderer]</b> ";


	void OnEnable()
	{
		tube = target as TubeRenderer;

		_points = serializedObject.FindProperty( "_points" );
		_radiuses = serializedObject.FindProperty( "_radiuses" );
		_radius = serializedObject.FindProperty( "_radius" );
		_pointColors = serializedObject.FindProperty( "_pointColors" );
        _twists = serializedObject.FindProperty( "_twists" );
		_edgeCount = serializedObject.FindProperty( "_edgeCount" );
		_calculateTangents = serializedObject.FindProperty( "_calculateTangents" );
		_invertMesh = serializedObject.FindProperty( "_invertMesh" );
		_normalMode = serializedObject.FindProperty( "_normalMode" );
		_caps = serializedObject.FindProperty( "_caps" );
		_uvRect = serializedObject.FindProperty( "_uvRect" );
		_uvRectCap = serializedObject.FindProperty( "_uvRectCap" );
		_uvRectCapEndMirrored = serializedObject.FindProperty( "_uvRectCapEndMirrored" );
        _uvAntiStretching = serializedObject.FindProperty( "_uvAntiStretching" );
		_forwardAngleOffset = serializedObject.FindProperty( "_forwardAngleOffset" );
		_showMeshGizmos = serializedObject.FindProperty( "_showMeshGizmos" );
		_meshGizmoLength = serializedObject.FindProperty( "_meshGizmoLength" );
		_showRotationGizmos = serializedObject.FindProperty( "_showRotationGizmos" );
		_rotationGizmoLength = serializedObject.FindProperty( "_rotationGizmoLength" );
		_pointsFoldout = serializedObject.FindProperty( "_pointsFoldout" );
		_radiusesFoldout = serializedObject.FindProperty( "_radiusesFoldout" );
		_colorsFoldout = serializedObject.FindProperty( "_colorsFoldout" );
        _twistsFoldout = serializedObject.FindProperty( "_twistsFoldout" );
		_uvFoldout = serializedObject.FindProperty( "_uvFoldout" );
	}


	
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		isDirty = false;

		// Stats
		EditorGUILayout.LabelField( "Stats: ", tube.points.Length.ToString() + " points, " + tube.mesh.vertexCount.ToString() + " verts." );
		
		// Points
		_pointsFoldout.boolValue = EditorGUILayout.Foldout( _pointsFoldout.boolValue, "Points", true );
		if( _pointsFoldout.boolValue )
		{
			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			int newCount = EditorGUILayout.IntField( "Count", _points.arraySize );
            if( EditorGUI.EndChangeCheck() && newCount > 1 && _points.arraySize != newCount ){
				// Check against Unity mesh limit.
				int targetVertexCount = ComputeVertexCountForProperties( newCount, _edgeCount.intValue, (TubeRenderer.NormalMode) _normalMode.enumValueIndex, (TubeRenderer.CapMode) _caps.enumValueIndex );
				if( targetVertexCount > meshVertexCountLimit ){
					Debug.LogWarning( messageHeader + "Points change for " + tube.name + " was ignored. You are exceeding Unity's 65000 vertex limit.\n" );
				} else {
					// Update point count.
					int lastArraySize = _points.arraySize;
					_points.arraySize = newCount;
					if( _points.arraySize > lastArraySize && lastArraySize > 0 ){
						// Super smart extension
						Vector3 position = _points.GetArrayElementAtIndex(lastArraySize-1).vector3Value;
						Vector3 step = position - _points.GetArrayElementAtIndex(lastArraySize-2).vector3Value;
						for( int p = lastArraySize; p < _points.arraySize; p++ ){
							position += step;
							_points.GetArrayElementAtIndex(p).vector3Value = position;
						}
					}
					if( _radiuses.arraySize != 0 ) _radiuses.arraySize = newCount;
					if( _pointColors.arraySize != 0 ) _pointColors.arraySize = newCount;
                    if( _twists.arraySize != 0 ) _twists.arraySize = newCount;
					isDirty = true;
				}
			}

			EditorGUI.indentLevel++;

			if( newCount < 100 ){
				EditorGUI.BeginChangeCheck();
				for( int i = 0; i < _points.arraySize; i++ ) EditorGUILayout.PropertyField( _points.GetArrayElementAtIndex( i ) );
				if( EditorGUI.EndChangeCheck() ) isDirty = true;
			} else {
				EditorGUILayout.HelpBox( "Too many points to display in inspector.", MessageType.Warning );
			}

			EditorGUI.indentLevel--;

			EditorGUI.indentLevel--;
		}

		// Radius
		if( _radiuses.arraySize != 0 ) GUI.color = disabledColor;
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.Slider( _radius, 0.01f, 10 );
		if( EditorGUI.EndChangeCheck() ) {
			isDirty = true;
			_radiuses.arraySize = 0;
		}
		GUI.color = Color.white;

		// Radiuses
		if( _radiuses.arraySize == 0 ) GUI.color = disabledColor;
		_radiusesFoldout.boolValue = EditorGUILayout.Foldout( _radiusesFoldout.boolValue, "Radiuses", true );
		if( _radiusesFoldout.boolValue )
		{
			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			bool usingArray = _radiuses.arraySize != 0;
			bool newUseArray = EditorGUILayout.Toggle( "Use", usingArray );
			if( EditorGUI.EndChangeCheck() && newUseArray != usingArray ){
				_radiuses.arraySize = newUseArray ? _points.arraySize : 0;
				for( int i = 0; i < _radiuses.arraySize; i++ ) _radiuses.GetArrayElementAtIndex( i ).floatValue = _radius.floatValue;
				isDirty = true;
			}

			EditorGUI.indentLevel++;

			if( _points.arraySize < 100 ){
				EditorGUI.BeginChangeCheck();
				for( int i = 0; i < _radiuses.arraySize; i++ ) EditorGUILayout.PropertyField( _radiuses.GetArrayElementAtIndex( i ) );
				if( EditorGUI.EndChangeCheck() ) isDirty = true;
			} else {
				EditorGUILayout.HelpBox( "Too many points to display in inspector.", MessageType.Warning );
			}
			EditorGUI.indentLevel--;

			EditorGUI.indentLevel--;
		}
		GUI.color = Color.white;
		
		
		// Colors.
		if( _pointColors.arraySize == 0 ) GUI.color = disabledColor;
		_colorsFoldout.boolValue = EditorGUILayout.Foldout( _colorsFoldout.boolValue, "Colors (vertex colors)", true );
		if( _colorsFoldout.boolValue )
		{
			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			bool usingArray = _pointColors.arraySize != 0;
			bool newUseArray = EditorGUILayout.Toggle( "Use", usingArray );
			if( EditorGUI.EndChangeCheck() && newUseArray != usingArray ){
				_pointColors.arraySize = newUseArray ? _points.arraySize : 0;
				for( int i = 0; i < _pointColors.arraySize; i++ ) _pointColors.GetArrayElementAtIndex( i ).colorValue = Color.white;
				isDirty = true;
			}

			EditorGUI.indentLevel++;
			EditorGUI.BeginChangeCheck();
			for( int i = 0; i < _pointColors.arraySize; i++ ) EditorGUILayout.PropertyField( _pointColors.GetArrayElementAtIndex( i ) );
			if( EditorGUI.EndChangeCheck() ) isDirty = true;
			EditorGUI.indentLevel--;

			EditorGUI.indentLevel--;
		}
		GUI.color = Color.white;
		
        // Twists.
        if( _twists.arraySize == 0 ) GUI.color = disabledColor;
        _twistsFoldout.boolValue = EditorGUILayout.Foldout( _twistsFoldout.boolValue, "Twists", true );
        if( _twistsFoldout.boolValue )
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            bool usingArray = _twists.arraySize != 0;
            bool newUseArray = EditorGUILayout.Toggle( "Use", usingArray );
            if( EditorGUI.EndChangeCheck() && newUseArray != usingArray ){
                _twists.arraySize = newUseArray ? _points.arraySize : 0;
                isDirty = true;
            }

            EditorGUI.indentLevel++;

            if( _points.arraySize < 100 ){
                EditorGUI.BeginChangeCheck();
                for( int i = 0; i < _twists.arraySize; i++ ) EditorGUILayout.PropertyField( _twists.GetArrayElementAtIndex( i ) );
                if( EditorGUI.EndChangeCheck() ) isDirty = true;
            } else {
                EditorGUILayout.HelpBox( "Too many points to display in inspector.", MessageType.Warning );
            }
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel--;
        }
        GUI.color = Color.white;

		// Tangents.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( _calculateTangents );
		if( EditorGUI.EndChangeCheck() ) isDirty = true;
		
		// Invert mesh.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( _invertMesh );
		if( EditorGUI.EndChangeCheck() ) isDirty = true;
		
		// Normal mode.
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PrefixLabel( "Normal Mode" );
		TubeRenderer.NormalMode newNormalMode = (TubeRenderer.NormalMode) EditorGUILayout.EnumPopup( (TubeRenderer.NormalMode) _normalMode.enumValueIndex );
		if( EditorGUI.EndChangeCheck() ){
			// Check against Unity mesh limit.
			int targetVertexCount = ComputeVertexCountForProperties( _points.arraySize, _edgeCount.intValue, newNormalMode, (TubeRenderer.CapMode) _caps.enumValueIndex );
			if( targetVertexCount > meshVertexCountLimit ){
				Debug.LogWarning( messageHeader + "Normal mode change for " + tube.name + " was ignored. You are exceeding Unity's 65000 vertex limit.\n" );
			} else {
				// Update.
				_normalMode.enumValueIndex = (int) newNormalMode;
				isDirty = true;
			}
		}
		EditorGUILayout.EndHorizontal();

		// Caps.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel( "Caps" );
		TubeRenderer.CapMode newCapMode = (TubeRenderer.CapMode) EditorGUILayout.EnumPopup( (TubeRenderer.CapMode) _caps.enumValueIndex );
		if( EditorGUI.EndChangeCheck() ){
			// Check against Unity mesh limit.
			int targetVertexCount = ComputeVertexCountForProperties( _points.arraySize, _edgeCount.intValue, (TubeRenderer.NormalMode) _normalMode.intValue, newCapMode );
			if( targetVertexCount > meshVertexCountLimit ){
				Debug.LogWarning( messageHeader + "Caps change for " + tube.name + " was ignored. You are exceeding Unity's 65000 vertex limit.\n" );
			} else {
				// Update.
				_caps.enumValueIndex = (int) newCapMode;
				isDirty = true;
			}
		}
		EditorGUILayout.EndHorizontal();

		// Edge count.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel( "Edge Count" );
		int newEdgeCount = EditorGUILayout.IntSlider( _edgeCount.intValue, 3, 64 );
		if( EditorGUI.EndChangeCheck() ){
			// Check against Unity mesh limit.
			int targetVertexCount = ComputeVertexCountForProperties( _points.arraySize, newEdgeCount, (TubeRenderer.NormalMode) _normalMode.enumValueIndex, (TubeRenderer.CapMode) _caps.enumValueIndex );
			if( targetVertexCount > meshVertexCountLimit ){
				Debug.LogWarning( messageHeader + "Edge count change for " + tube.name + " was ignored. You are exceeding Unity's 65000 vertex limit.\n" );
			} else {
				// Update.
				_edgeCount.intValue = newEdgeCount;
				isDirty = true;
			}
		};
		EditorGUILayout.EndHorizontal();

		// Forward angle offset.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.Slider( _forwardAngleOffset, -180f, 180 );
		if( EditorGUI.EndChangeCheck() ) isDirty = true;
		
		// UV mapping.
		_uvFoldout.boolValue = EditorGUILayout.Foldout( _uvFoldout.boolValue, "UV Mapping", true );
		if( _uvFoldout.boolValue )
		{
			EditorGUI.indentLevel++;

			// UV rect
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( _uvRect );
			if( EditorGUI.EndChangeCheck() ) isDirty = true;
			
			if( _caps.enumValueIndex != (int) TubeRenderer.CapMode.None )
			{
				// UV rect
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( _uvRectCap );
				if( EditorGUI.EndChangeCheck() ) isDirty = true;

				// UV rect cap mirrored
				if( _caps.enumValueIndex == (int) TubeRenderer.CapMode.Both || _caps.enumValueIndex == (int) TubeRenderer.CapMode.End ){
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( _uvRectCapEndMirrored );
					if( EditorGUI.EndChangeCheck() ) isDirty = true;
				}
			}

            // UV Anti Stretching.
            EditorGUILayout.PropertyField( _uvAntiStretching );
			
			EditorGUI.indentLevel--;
		}

		// Mesh gizmos.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( _showMeshGizmos );
		if( EditorGUI.EndChangeCheck() ) isDirty = true;
	
		// Mesh gizmos length.
		if( _showMeshGizmos.boolValue ){
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Slider( _meshGizmoLength, 0.01f, 1f );
			if( EditorGUI.EndChangeCheck() ) isDirty = true;
		}

		// Mesh gizmos.
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( _showRotationGizmos );
		if( EditorGUI.EndChangeCheck() ) isDirty = true;

		// Mesh gizmos length.
		if( _showRotationGizmos.boolValue ){
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Slider( _rotationGizmoLength, 0.01f, 1f );
			if( EditorGUI.EndChangeCheck() ) isDirty = true;
		}
		
		// Update and serialize the tube.
		serializedObject.ApplyModifiedProperties();

		// Request that the tube gets an update call.
        if( isDirty ){
            EditorUtility.SetDirty( tube );
        }
	}
	
	
	
	public void OnSceneGUI()
	{
		// labels
		if( tube.points.Length > 1 ){
			Handles.Label( tube.transform.localToWorldMatrix.MultiplyPoint3x4( tube.points[0] ), "p0" );
			Handles.Label( tube.transform.localToWorldMatrix.MultiplyPoint3x4( tube.points[tube.points.Length-1] ), "p" + (tube.points.Length-1) );
		}
		
		// TODO: Handles. this has be carefully thought through before it's integrated
		/*
		Vector3[] newPoints = new Vector3[tube.points.Length];
		for( int p=0; p<tube.points.Length; p++ ){
			newPoints[p] = tube.transform.worldToLocalMatrix.MultiplyPoint( Handles.PositionHandle( tube.transform.localToWorldMatrix.MultiplyPoint( tube.points[p]), Quaternion.identity ) );
		}
		if( GUI.changed ){
			tube.points = newPoints;
			EditorUtility.SetDirty( tube );
		}
		*/
	}


	static int ComputeVertexCountForProperties( int pointCount, int edgeCount, TubeRenderer.NormalMode normalMode, TubeRenderer.CapMode capMode )
	{
		int targetVertexCount = 0;

		switch( normalMode ){
		case TubeRenderer.NormalMode.Smooth: targetVertexCount = pointCount * (edgeCount+1); break;
		case TubeRenderer.NormalMode.Hard: targetVertexCount = (pointCount-1) * edgeCount * 4; break;
		case TubeRenderer.NormalMode.HardEdges: targetVertexCount = pointCount * 2 * edgeCount; break;
		}
		if( capMode == TubeRenderer.CapMode.Both ) targetVertexCount += ( (edgeCount+1) + 1 ) * 2;
		else if( capMode == TubeRenderer.CapMode.Begin || capMode == TubeRenderer.CapMode.End ) targetVertexCount += (edgeCount+1) + 1;

		return targetVertexCount;
	}
}