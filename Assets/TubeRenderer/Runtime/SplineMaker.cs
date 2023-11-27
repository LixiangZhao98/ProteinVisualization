/*
	Copyright 2016-2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk

	This is a Unity Asset Store product.
		http://u3d.as/2Tc
		https://unity3d.com/legal/as_terms
*/

/// <summary>
/// Generates a set of points along a (Catmull-Rom) spline curve based on a set of anchor points.
/// The computed points can be routed to TubeRenderer or another reciever.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class SplineMaker : MonoBehaviour
{
	[SerializeField] int _pointsPerSegment = 8;
	[SerializeField] bool _loop = false;
	[SerializeField] Vector3[] _anchorPoints = new Vector3[0];
	[SerializeField] Vector3ArrayEvent _onUpdated = new Vector3ArrayEvent();

	bool _isDirty = true;

	Vector3[] _points;

	[System.Serializable] public class Vector3ArrayEvent : UnityEvent<Vector3[]> {}

	/// <summary>
	/// Gets or sets the resolution per segment.
	/// </summary>
	public int pointsPerSegment {
		get { return _pointsPerSegment; }
		set {
			_pointsPerSegment = Mathf.Max( 0, value );
			_isDirty = true;
		}
	}


	/// <summary>
	/// Gets or sets a value indicating whether this spline is looped.
	/// </summary>
	public bool loop {
		get { return _loop; }
		set {
			_loop = value;
			_isDirty = true;
		}
	}


	/// <summary>
	/// Gets or sets the anchor points of the spline.
	/// </summary>
	public Vector3[] anchorPoints {
		get { return _anchorPoints; }
		set {
			_anchorPoints = value;
			_isDirty = true;
		}
	}

	/// <summary>
	/// Direct reference to the array of computed spline points.
	/// </summary>
	/// <value>The points.</value>
	public Vector3[] points { get { return _points; } }


	/// <summary>
	/// Event that will fire when points have been updated.
	/// </summary>
	public Vector3ArrayEvent onUpdated { get { return _onUpdated; } }


	void Awake()
	{
		// If the tube was created in the editor, then provide two points.
		if( !Application.isPlaying && _anchorPoints.Length == 0 ) anchorPoints = new Vector3[]{ Vector3.zero, Vector3.up };
	}


	void Update()
	{
		if( _isDirty || transform.hasChanged ){
			UpdatePoints();
			_isDirty = false;
			transform.hasChanged = false;
		}
	}


	void UpdatePoints()
	{
		if( _anchorPoints == null || _anchorPoints.Length == 0 ){
			if( _points != null ){
				_points = null;
				_onUpdated.Invoke( _points );
			}
			return;
		}

		if( _anchorPoints.Length == 1 ){
			if( _points == null || _points.Length != 1 ){
				_points = new Vector3[]{ _anchorPoints[0] };
				_onUpdated.Invoke( _points );
			}
			return;
		}

		bool useLoop = _loop && _anchorPoints.Length > 2;
		int pointCount = ( (_anchorPoints.Length-1) * (_pointsPerSegment+1) ) + 1 + ( useLoop ? _pointsPerSegment+1 : 0 );
		if( _points == null || _points.Length != pointCount ) _points = new Vector3[pointCount];

		int c = 0;
		for( int a=0; a<_anchorPoints.Length-1; a++ ){
			_points[ c++ ] = _anchorPoints[a];
			int a0 = a > 0 ? a-1 : useLoop ? _anchorPoints.Length-1 : a;
			int a2 = a+1;
			int a3 = a2 < _anchorPoints.Length-1 ? a+2 : useLoop ? 0 : a2;
			for( int s=0; s<_pointsPerSegment; s++ ){
				float t = (s+1) / (_pointsPerSegment+1f);
				_points[c++] = CatmullRomInterpolation( _anchorPoints[a0], _anchorPoints[a], _anchorPoints[a2], _anchorPoints[a3], t );
			}
		}
		_points[c++] = _anchorPoints[_anchorPoints.Length-1];
		if( useLoop ){
			for( int s=0; s<_pointsPerSegment; s++ ){
				float t = (s+1) / (_pointsPerSegment+1f);
				_points[c++] = CatmullRomInterpolation( _anchorPoints[_anchorPoints.Length-2], _anchorPoints[_anchorPoints.Length-1], _anchorPoints[0], _anchorPoints[1], t );
			}
			_points[c++] = _anchorPoints[0];
		}


		_onUpdated.Invoke( _points );
	}


	void OnValidate()
	{
		pointsPerSegment = _pointsPerSegment;
		loop = _loop;
		anchorPoints = _anchorPoints;
	}


	void OnDidApplyAnimationProperties()
	{
		OnValidate();
	}


	// http://www.iquilezles.org/www/articles/minispline/minispline.htm
	static Vector3 CatmullRomInterpolation( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t )
	{
		float t2 = t * t;
		float t3 = t2 * t;
		return 0.5f * ( (2f * p1) + ( p2 - p0 ) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3 );
	}
		
}