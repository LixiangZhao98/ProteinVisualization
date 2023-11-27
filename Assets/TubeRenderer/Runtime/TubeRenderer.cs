/*
	Copyright 2012-2020 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk

	This is a Unity Asset Store product.
	http://u3d.as/2Tc
	https://unity3d.com/legal/as_terms
*/


using UnityEngine;
using UnityEngine.Rendering;
using TubeRendererInternals;

[ AddComponentMenu( "Effects/TubeRenderer" ) ]
[ RequireComponent( typeof( MeshFilter ) ) ]
[ RequireComponent( typeof( MeshRenderer ) ) ]
[ ExecuteInEditMode ]

/// <summary>
/// Generates a tube mesh based on a set of points and a bunch of settings described below.
/// </summary>
public class TubeRenderer : MonoBehaviour
{
	public enum NormalMode { Smooth, Hard, HardEdges }
	public enum CapMode { None, Begin, End, Both }

	[SerializeField] Vector3[] _points = new Vector3[0];
	[SerializeField] float[] _radiuses = new float[0];
	[SerializeField] float _radius = 0.1f;
	[SerializeField] Color32[] _pointColors = new Color32[0];
	[SerializeField] float[] _twists = new float[0];
	[SerializeField] int _edgeCount = 12; // Minimum is three
	[SerializeField] bool _calculateTangents;
	[SerializeField] bool _invertMesh;
	[SerializeField] NormalMode _normalMode = NormalMode.Smooth;
	[SerializeField] CapMode _caps = CapMode.Both;
	[SerializeField] bool _postprocessContinously = true;
	[SerializeField] Rect _uvRect = new Rect(0,0,1,1);
	[SerializeField] Rect _uvRectCap = new Rect(0,0,1,1);
	[SerializeField] bool _uvRectCapEndMirrored;
	[SerializeField] bool _uvAntiStretching;
	[SerializeField] float _forwardAngleOffset;
	[SerializeField] Mesh _mesh;
	[SerializeField] bool _showMeshGizmos;
	[SerializeField] float _meshGizmoLength = 0.1f;
	[SerializeField] bool _showRotationGizmos;
	[SerializeField] float _rotationGizmoLength = 0.2f;
	[SerializeField] bool _pointsFoldout, _radiusesFoldout, _colorsFoldout, _twistsFoldout, _uvFoldout; // For the inspector

	Vector3[] _vertices;
	Vector3[] _normals;
	int[] _triangles;
	Vector2[] _uvs;
	Vector4[] _tangents;
	Color32[] _colors32;
	Vector3[] _circlePointLookup;
	Vector3[] _circleNormalLookup;
	Vector3[] _circleTangentLookup;
	Quaternion[] _rotations;
	Vector3[] _directions;
	float[] _lengths;
	float[] _steepnessAngles;
	float[] _radiusDiffs;
	Vector3 _pastBeginUp;
	Vector3[] _tempVector3s;
	int[] _tempQuadLookup;
	float _length;

	MeshFilter _filter;

	bool _dirtyCircle = true;
	bool _dirtyRotations = true;
	bool _dirtySteepnessAngles = true;
	bool _dirtyVertexCount = true;
	bool _redrawFlag = true;
	bool _dirtyTriangles = true;
	bool _dirtyUVs = true;
	bool _dirtyColors = true;

	const float tau = Mathf.PI * 2;
	const int meshVertexCountLimit = 65000;
	const string logPrepend = "<b>[" + nameof( TubeRenderer ) + "]</b> ";


	////////////
	// PUBLIC //
	////////////

	/// <summary>
	/// Center points for the tube. Forward rotations will be calculated from the point at index 0 and upwards. The array is NOT copied; the tube will keep the reference to the array.
	/// </summary>
	public Vector3[] points {
		get { return _points; }
		set {
			if( value == null ){
				_points = new Vector3[0];
				return;
			}
			if( value.Length == 1 ){
				_points = value;
				return;
			}
			if( value.Length != _points.Length ){
				// Check against Unity mesh limit.
				int targetVertexCount = ComputeVertexCountForProperties( value.Length, _edgeCount, _normalMode, _caps );
				if( targetVertexCount > meshVertexCountLimit ){
					Debug.LogWarning( logPrepend + "Points change for " + name + " was ignored. You are exceeding Unity's 65000 vertex limit.\n" );
					return;
				}
				_dirtyVertexCount = true;
				_dirtyTriangles = true;
				_dirtyUVs = true;
				_dirtyColors = true;
			}
			if( _uvAntiStretching ) _dirtyUVs = true;
			_dirtyRotations = true;
			_dirtySteepnessAngles = true;
			_redrawFlag = true;
			_points = value;
		}
	}
	
	/// <summary>
	/// Radius values for the tube. Each value corresponds to the point at the same array index. Array length must fit the number of points. If 'radius' has been set then 'radiuses' will be ignored. The array is NOT copied; the tube will keep the reference to the array.
	/// </summary>
	public float[] radiuses {
		get { return _radiuses; }
		set {
			if( value == null ){
				_radiuses = new float[0];
			} else {
				_radiuses = value;
				_dirtySteepnessAngles = true;
			}
			_redrawFlag = true;
		}
	}
	
	/// <summary>
	/// Radius for the entire tube. If 'radiusses' has been set then 'radius' will be ignored. Default is 0.1.
	/// </summary>
	public float radius {
		get { return _radius; }
		set {
			if( _radiuses.Length != 0 ){
				_radiuses = new float[0];
				_steepnessAngles = null;
			}
			if( Mathf.Approximately( value, _radius ) ) return;
			_redrawFlag = true;
			_radius = value;
		}
	}
	
	/// <summary>
	/// Vertex colors for the tube. Each value corresponds to the point at the same array index. Array length must fit the number of points. The array is NOT copied; the tube will keep the reference to the array.
	/// </summary>
	public Color32[] colors {
		get { return _pointColors; }
		set {
			if( value == null ) return;
			if( value != null && value.Length == 0 ){
				_pointColors = null;
			} else {
				_pointColors = value;
			}
			_dirtyColors = true;
		}
	}

	/// <summary>
	/// Twist angles (degrees) for each point in the tube. Array length must fit the number of points. The array is NOT copied; the tube will keep the reference to the array.
	/// </summary>
	public float[] twists {
		get { return _twists; }
		set {
			if( value == null ){
				_twists = new float[0];
			} else {
				_twists = value;
				_dirtyRotations = true;
			}
			_redrawFlag = true;
		}
	}
	
	/// <summary>
	/// Edge resolution. Minimum is 3. Default is 12.
	/// </summary>
	public int edgeCount {
		get { return _edgeCount; }
		set {
			if( value == _edgeCount ) return;
			if( value < 3 ){
				Debug.LogWarning( logPrepend + "Edge count change for " + name + " was ignored. A tube must have at least three edges." );
				return;
			}
			// Check against Unity mesh limit.
			int targetVertexCount = ComputeVertexCountForProperties( _points.Length, value, _normalMode, _caps );
			if( targetVertexCount > meshVertexCountLimit ){
				Debug.LogWarning( logPrepend + "Edge count change for " + name + " was ignored. You are exceeding Unity's 65000 vertex limit.\n" );
				return;
			}
			_dirtyCircle = true;
			_dirtyVertexCount = true;
			_redrawFlag = true;
			_dirtyTriangles = true;
			_dirtyUVs = true;
			_dirtyColors = true;
			_edgeCount = value;
		}
	}
	
	/// <summary>
	/// Calculation of tangents. Default is false (to boost performance).
	/// </summary>
	public bool calculateTangents {
		get { return _calculateTangents; }
		set {
			if( value == _calculateTangents ) return;
			if( !value ){
				_tangents = null;
				_mesh.tangents = null;
			}
			_dirtyVertexCount = true; // We need UpdateVertexCount call to ensure length of tangent array.
			_redrawFlag = true;
			_calculateTangents = value;
		}
	}
	
	/// <summary>
	/// Mesh inversion (render the tube inside out). In most cases you should do 'Cull Front' in your shader instead. Default is false.
	/// </summary>
	public bool invertMesh {
		get { return _invertMesh; }
		set {
			if( value == _invertMesh ) return;
			_dirtyTriangles = true;
			_redrawFlag = true;
			_invertMesh = value;
		}
	}

	/// <summary>
	/// How normals are rendered. Default is NormalMode.Smooth.
	/// </summary>
	public NormalMode normalMode {
		get { return _normalMode; }
		set {
			if( value == _normalMode ) return;
			// Check against Unity mesh limit.
			int targetVertexCount = ComputeVertexCountForProperties( _points.Length, _edgeCount, value, _caps );
			if( targetVertexCount > meshVertexCountLimit ){
				Debug.LogWarning( logPrepend + "Normal mode change for " + name + " was ignored. You are exceeding Unity's 65000 vertex limit.\n" );
				return;
			}
			_dirtyCircle = true;
			_dirtyVertexCount = true;
			_redrawFlag = true;
			_dirtyTriangles = true;
			_dirtyUVs = true;
			_dirtyColors = true;
			_normalMode = value;
		}
	}

	/// <summary>
	/// Closed end points. Default is true.
	/// </summary>
	public CapMode caps {
		get { return _caps; }
		set {
			if( value == _caps ) return;
			// Check against Unity mesh limit.
			int targetVertexCount = ComputeVertexCountForProperties( _points.Length, _edgeCount, _normalMode, value );
			if( targetVertexCount > meshVertexCountLimit ){
				Debug.LogWarning( logPrepend + "Caps mode change for " + name + " was ignored. You are exceeding Unity's 65000 vertex limit.\n" );
				return;
			}
			_dirtyVertexCount = true;
			_redrawFlag = true;
			_dirtyTriangles = true;
			_dirtyUVs = true;
			_dirtyColors = true;
			_caps = value;
		}
	}
	 
	/// <summary>
	/// Postprocess continously (if AddPostprocess has been called). When true, postprocesses will be called every update. When false, they will only be called when tube properties are changed. Default is true.
	/// </summary>
	public bool postprocessContinously {
		get { return _postprocessContinously; }
		set { _postprocessContinously = value; }
	}
	
	/// <summary>
	/// UV mapping rect for wrapped tube body. Default is Rect(0,0,1,1).
	/// </summary>
	public Rect uvRect {
		get { return _uvRect; }
		set {
			if( value == _uvRect ) return;
			_dirtyUVs = true;
			_uvRect = value;
		}
	}
	
	/// <summary>
	/// UV mapping rect for tube caps (if caps is true). Default is Rect(0,0,1,1).
	/// </summary>
	public Rect uvRectCap {
		get { return _uvRectCap; }
		set {
			if( value == _uvRectCap ) return;
			_dirtyUVs = true;
			_uvRectCap = value;
		}
	}
	
	/// <summary>
	/// Mirrored uv mapping for cap at end point (points[points.Length-1]). Default is false.
	/// </summary>
	public bool uvRectCapEndMirrored {
		get { return _uvRectCapEndMirrored; }
		set {
			if( value == _uvRectCapEndMirrored ) return;
			_dirtyUVs = true;
			_uvRectCapEndMirrored = value;
		}
	}

	/// <summary>
	/// Avoid stretching texture when segments have different lengths. Default is false.
	/// </summary>
	public bool uvAntiStretching {
		get { return _uvAntiStretching; }
		set {
			if( value == _uvAntiStretching ) return;
			_dirtyUVs = true;
			_uvAntiStretching = value;
		}
	}
	
	/// <summary>
	/// Rotation offset around the tubes forward direction.
	/// </summary>
	public float forwardAngleOffset {
		get { return _forwardAngleOffset; }
		set {
			if( Mathf.Approximately( value, _forwardAngleOffset ) ) return;
			//_dirtyRotations = true;
			_dirtyCircle = true;
			_redrawFlag = true;
			_forwardAngleOffset = value;
		}
	}

	/// <summary>
	/// Gets the total length of the tube.
	/// </summary>
	public float length { get { return _length; } }
	
	/// <summary>
	/// Get the tube mesh. Useful for combining multiple tubes into a static mesh. Do not manipulate directly.
	/// </summary>
	public Mesh mesh { get{ return _mesh; } }
	
	/// <summary>
	/// Draw gizmos that indicate mesh normals and tangents. Default is false.
	/// </summary>
	public bool showMeshGizmos { get { return _showMeshGizmos; } set { _showMeshGizmos = value; }  }

	/// <summary>
	/// Length of mesh gizmos. Default is 0.1.
	/// </summary>
	public float meshGizmoLength { get { return _meshGizmoLength; } set { _meshGizmoLength = value; } }

	/// <summary>
	/// Draw gizmos indicating rotation of each point. Default is false.
	/// </summary>
	public bool showRotationGizmos { get { return _showRotationGizmos; } set { _showRotationGizmos = value; }  }

	/// <summary>
	/// Length of rotation gizmos. Default is 0.1.
	/// </summary>
	public float rotationGizmoLength { get { return _rotationGizmoLength; } set { _rotationGizmoLength = value; } }

	/// <summary>
	/// Force update to generate the tube mesh immediately.
	/// </summary>
	public void ForceUpdate()
	{
		LateUpdate();
	}
	
	
	/// <summary>
	/// Shortcut to Mesh.MarkDynamic(). Call this if the tube will be updated often so that Unity can optimise memory use.
	/// </summary>
	public void MarkDynamic()
	{
		_mesh.MarkDynamic();
	}
	
	
	/// <summary>
	/// Add a method to receive and manipulate mesh data before it is applied. Useful for creating distortion effects or complex variations.
	/// </summary>
	public void AddPostprocess( Postprocess postprocess )
	{
		Postprocesses += postprocess;
	}
	
	
	/// <summary>
	/// Remove a postprocess method that have previously been assigned using the 'AddPostprocess' method.
	/// </summary>
	public void RemovePostprocess( Postprocess postprocess )
	{
		if( Postprocesses != null ) Postprocesses -= postprocess;
	}
	
	/// <summary>
	/// Method for passing mesh data.
	/// </summary>
	// TODO replace with [System.Serializable] class MeshDataEvent : UnityEvent<Vector3[], Vector3[], Vector4[]>{}
	public delegate void Postprocess( Vector3[] vertices, Vector3[] normals, Vector4[] tangents );
	Postprocess Postprocesses;


	/// <summary>
	/// Gets the rotation at point.
	/// Ensure that the tube has been updated before calling this, for example by calling ForceUpdate beforehand.
	/// </summary>
	public Quaternion GetRotationAtPoint( int index )
	{
		if( index < 0 || index > _rotations.Length-1 ) return Quaternion.identity;
		return _rotations[index];
	}


	/// <summary>
	/// Gets the distance from point at index to next point.
	/// Ensure that the tube has been updated before calling this, for example by calling ForceUpdate beforehand.
	/// </summary>
	public float GetLengthAtPoint( int index )
	{
		if( index < 0 || index > _lengths.Length-1 ) return 0;
		return _lengths[index];
	}



	/////////////
	// PRIVATE //
	/////////////


	void Awake()
	{
		// Ensure that we have a mesh filter.
		_filter = gameObject.GetComponent<MeshFilter>();
		if( !_filter ) _filter = gameObject.AddComponent<MeshFilter>();
		
		// Ensure that we have a mesh. If tube was duplicated or pasted, then make sure it gets a seprate mesh.
		if( !_mesh || ( Application.isEditor && Event.current != null && ( Event.current.commandName == "Duplicate" || Event.current.commandName == "Paste" ) ) ){
			_mesh = new Mesh();
			_mesh.name = "Tube " + gameObject.GetInstanceID();
			_filter.sharedMesh = _mesh;
		}
		
		// Ensure that we have mesh renderer and material.
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		if( meshRenderer == null ) meshRenderer = gameObject.AddComponent<MeshRenderer>();
		if( meshRenderer.sharedMaterial == null ) meshRenderer.sharedMaterial = RenderPipelineHelpers.CreateRenderPipelineCompatibleMaterial();

		// If the tube was created in the editor, then provide two points.
		if( !Application.isPlaying && points.Length == 0 ) points = new Vector3[]{ Vector3.zero, Vector3.up };
	}
	
	
	void LateUpdate()
	{
		// Return if no points.
		if( _points.Length < 2 ){
			if( _mesh.vertexCount > 0 ) _mesh.Clear();
			return;
		}

		// When postprocessing we need to recalculate mesh data every update.
		if( Postprocesses != null && _postprocessContinously ) _redrawFlag = true;
		
		// Update only what needs updating.
		if( _dirtyVertexCount ) UpdateVertexCount();				// Before Everything!
		if( _dirtyRotations ) UpdateRotations();					// Before UpdateSteepnessAngles!
		if( _dirtySteepnessAngles ) UpdateSteepnessAngles();		// Before ReDraw!
		if( _dirtyCircle ) UpdateCircleLookup();					// Before ReDraw!
		if( _redrawFlag ) ReDraw();									// Update vertices, normals and tangents.
		if( _dirtyTriangles ) UpdateTriangles();					// After ReDraw!
		if( _dirtyUVs ) UpdateUVs();								// After ReDraw!
		if( _dirtyColors ) UpdateColors(); 							// After ReDraw!
	}


	// When the user makes a change in the inspector
	void OnValidate()
	{
		_dirtyCircle = true;
		_dirtyRotations = true;
		_dirtySteepnessAngles = true;
		_dirtyVertexCount = true;
		_dirtyTriangles = true;
		_dirtyUVs = true;
		_dirtyColors = true;
		_redrawFlag = true;

		if( _mesh && !_calculateTangents && _mesh.tangents != null ) _mesh.tangents = null;
	}


	void OnDidApplyAnimationProperties()
	{
		OnValidate();
	}
	
	
	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;

		if( _showMeshGizmos )
		{
			for( int v=0; v<_vertices.Length; v++ )
			{
				// Normals.
				Gizmos.color = new Color( 0, 1, 1, 0.5f );
				Gizmos.DrawLine( _vertices[v], _vertices[v] + _normals[v] * _meshGizmoLength );
				
				// Tangents.
				if( calculateTangents && _tangents.Length != 0 ){
					if( _tangents[v].w < 0 ) Gizmos.color = new Color( 1, 0, 1, 0.5f );
					else if( _tangents[v].w > 0 ) Gizmos.color = new Color( 1, 1, 0, 0.5f );
					else Gizmos.color = Color.white;
					Gizmos.DrawLine( _vertices[v], _vertices[v] + new Vector3( _tangents[v].x, _tangents[v].y, _tangents[v].z ) * _meshGizmoLength );
				}
			}
		}

		if( _showRotationGizmos )
		{
			// Rotations.
			bool usingRadiuses = _radiuses.Length != 0;
			float dotSize = _rotationGizmoLength * 0.2f;
			for( int p = 0; p < _points.Length; p++ ){
				float offset = ( usingRadiuses ? _radiuses[p] : _radius ) + _rotationGizmoLength * 4;
				Vector3 up = _rotations[p] * Vector3.up;
				Vector3 right = _rotations[p] * Vector3.right;
				Vector3 forward = _rotations[p] * Vector3.forward;
				Vector3 endPoint = _points[p] + up * offset;
				Gizmos.color = new Color( 0, 1, 0, 0.7f );
				Gizmos.DrawLine( _points[p], endPoint );
				Gizmos.DrawSphere( endPoint, dotSize );
				endPoint = _points[p] + right * offset;
				Gizmos.color = new Color( 1, 0, 0, 0.7f );
				Gizmos.DrawLine( _points[p], endPoint );
				Gizmos.DrawSphere( endPoint, dotSize );
				endPoint = _points[p] + forward * offset;
				Gizmos.color = new Color( 0, 0, 1, 0.7f );
				Gizmos.DrawLine( _points[p], endPoint );
				Gizmos.DrawSphere( endPoint, dotSize );
			}
		}
	}
	

	void UpdateVertexCount()
	{
		// Count the number of vertices we need.
		int targetVertexCount = ComputeVertexCountForProperties( _points.Length, _edgeCount, _normalMode, _caps );

		// Update array lengths.
		if( _vertices == null || _vertices.Length != targetVertexCount ){
			_vertices = new Vector3[ targetVertexCount ];
			_normals = new Vector3[ targetVertexCount ];
			mesh.Clear();
			_pastBeginUp = Vector3.zero;
		}
		if( calculateTangents ) {
			if( _tangents == null || _tangents.Length != targetVertexCount ) _tangents = new Vector4[ targetVertexCount ];
		} else {
			if( _tangents == null || _tangents.Length != 0 ) _tangents = new Vector4[0];
		}

		_dirtyVertexCount = false;
	}


	// Redraw updates and uploads data that is dependent on point positions and radiuses, that is: vertices, normals and tangets. 
	void ReDraw()
	{
		int v = 0;
		Matrix4x4 matrix = new Matrix4x4();

		// Calculate vertices and update bounds.
		Vector3 minBounds = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
		Vector3 maxBounds = new Vector3( -float.MaxValue, -float.MaxValue, -float.MaxValue );
		bool usingRadiuses = _radiuses.Length != 0;
		for( int p=0; p<_points.Length; p++ )
		{
			if( usingRadiuses ){
				int rad = p % radiuses.Length;
				// Create transform matrix.
				matrix.SetTRS( _points[ p ], _rotations[ p ], Vector3.one * _radiuses[ rad ] );
				// Check min and max bounds.
				if( _points[ p ].x - _radiuses[ rad ] < minBounds.x ) minBounds.x = _points[ p ].x - _radiuses[ rad ];
				if( _points[ p ].y - _radiuses[ rad ] < minBounds.y ) minBounds.y = _points[ p ].y - _radiuses[ rad ];
				if( _points[ p ].z - _radiuses[ rad ] < minBounds.z ) minBounds.z = _points[ p ].z - _radiuses[ rad ];
				if( _points[ p ].x + _radiuses[ rad ] > maxBounds.x ) maxBounds.x = _points[ p ].x + _radiuses[ rad ];
				if( _points[ p ].y + _radiuses[ rad ] > maxBounds.y ) maxBounds.y = _points[ p ].y + _radiuses[ rad ];
				if( _points[ p ].z + _radiuses[ rad ] > maxBounds.z ) maxBounds.z = _points[ p ].z + _radiuses[ rad ];

			} else {
				// Create transform matrix.
				matrix.SetTRS( _points[ p ], _rotations[ p ], Vector3.one * _radius );
				// Check min and max bounds.
				if( _points[ p ].x - _radius < minBounds.x ) minBounds.x = _points[ p ].x - _radius;
				if( _points[ p ].y - _radius < minBounds.y ) minBounds.y = _points[ p ].y - _radius;
				if( _points[ p ].z - _radius < minBounds.z ) minBounds.z = _points[ p ].z - _radius;
				if( _points[ p ].x + _radius > maxBounds.x ) maxBounds.x = _points[ p ].x + _radius;
				if( _points[ p ].y + _radius > maxBounds.y ) maxBounds.y = _points[ p ].y + _radius;
				if( _points[ p ].z + _radius > maxBounds.z ) maxBounds.z = _points[ p ].z + _radius;
			}
			
			// Calculate vertices.
			for( int e=0; e<_edgeCount; e++ ) _vertices[v++] = matrix.MultiplyPoint3x4( _circlePointLookup[e] );
			_vertices[v] = _vertices[v-edgeCount]; // uv wrapping //
			v++;
		}

		// Add caps.
		switch( _normalMode ){
			case NormalMode.Smooth: break;
			case NormalMode.Hard: v = (_points.Length-1) * _edgeCount * 4; break;
			case NormalMode.HardEdges: v = _points.Length * _edgeCount * 2; break;
		}
		int invertSign = _invertMesh ? -1 : 1;
		if( _caps == CapMode.Both || _caps == CapMode.Begin ){
			Quaternion rotation = _rotations[0];
			Vector3 normal = rotation * Vector3.back * invertSign;
			Vector4 tangent = rotation * Vector3.right;
			tangent.w = -1;
			for( int e=0; e<_edgeCount+1; e++ ){
				_vertices[v] = _vertices[e];
				_normals[v] = normal;
				if( calculateTangents ) _tangents[v] = tangent;
				v++;
			}
			_vertices[v] = _points[0]; // center vertex
			_normals[v] = normal;
			if( calculateTangents ) _tangents[v] = tangent;
		}
		if( _caps == CapMode.Both || _caps == CapMode.End ){
			Vector3 normal = _rotations[_points.Length-1] * Vector3.forward * invertSign;
			Vector4 tangent = _rotations[_points.Length-1] * Vector3.left;
			tangent.w = -1;
			int vBegin = (_points.Length-1)*(_edgeCount+1);
			if( _caps == CapMode.Both ) v++;
			for( int e=0; e<_edgeCount+1; e++ ){
				_vertices[v] = _vertices[ vBegin+e ];
				_normals[v] = normal;
				if( calculateTangents ) _tangents[v] = tangent;
				v++;
			}
			_vertices[v] = _points[_points.Length-1]; // center vertex
			_normals[v] = normal;
			if( calculateTangents ) _tangents[v] = tangent;
		}

		// Draw tube in requested normal mode.
		switch( _normalMode ){
			case NormalMode.Smooth: ReDrawSmoothNormals(); break;
			case NormalMode.Hard: ReDrawHardNormals(); break;
			case NormalMode.HardEdges: ReDrawHardNormalEdges(); break;
		}
		
		// Post process.
		if( Postprocesses != null ) Postprocesses( _vertices, _normals, _tangents );
		
		// Update mesh (note that uvs and colors are set in their update methods).
		_mesh.vertices = _vertices;
		_mesh.normals = _normals;

		if( calculateTangents ) _mesh.tangents = _tangents;

		// Update bounds.
		Vector3 boundsSize = new Vector3( maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, maxBounds.z - minBounds.z );
		Vector3 boundsCenter = new Vector3( minBounds.x + boundsSize.x * 0.5f, minBounds.y + boundsSize.y * 0.5f, minBounds.z + boundsSize.z * 0.5f );
		_mesh.bounds = new Bounds( boundsCenter, boundsSize );

		_redrawFlag = false;
	}


	void ReDrawSmoothNormals()
	{
		int invertSign = _invertMesh ? -1 : 1;
		int v = 0;
		bool usingRadiuses = radiuses.Length != 0;
		for( int p=0; p<_points.Length; p++ ){
			for( int e=0; e<_edgeCount; e++ ){
				Vector3 normal = _circleNormalLookup[ e ] * invertSign;
				Quaternion rotation = usingRadiuses ? _rotations[p] * Quaternion.AngleAxis( _steepnessAngles[p], _circleTangentLookup[e] ) : _rotations[p];
				_normals[v] = rotation * normal;
				if( calculateTangents ){
					_tangents[v] = Vector3.Cross( rotation * _circleTangentLookup[e], _normals[v] );
					_tangents[v].w = -1;
				}
				v++;
			}
			// UV wrapping.
			_normals[v] = _normals[v-edgeCount];
			if( _calculateTangents ) _tangents[v] = _tangents[v-edgeCount];
			v++;
		}
	}


	void ReDrawHardNormals()
	{
		int v;

		// Store the first two segments temporarily to avoid overwriting what we are about to read.
		int ringVertexCount = ( _edgeCount + 1 ) * 2;
		if( _tempVector3s == null || _tempVector3s.Length != ringVertexCount ) _tempVector3s = new Vector3[ ringVertexCount ];
		for( int p=0; p<2; p++ ){
			for( int e=0; e<_edgeCount+1; e++ ){
				v = p*(_edgeCount+1)+e;
				_tempVector3s[v] = _vertices[v];
			}
		}

		// Go backwards and copy from already calculated vertices.
		if( _tempQuadLookup == null || _tempQuadLookup.Length != 4 ) _tempQuadLookup = new int[ 4 ];
		_tempQuadLookup[0] = 0;
		_tempQuadLookup[1] = _edgeCount+1;
		_tempQuadLookup[2] = _edgeCount+2;
		_tempQuadLookup[3] = 1;
		for( int p=_points.Length-2; p>0; p-- ){ // don't copy vertices from first circle
			for( int e=_edgeCount-1; e>=0; e-- ){
				v = p * _edgeCount * 4 + e * 4;
				int lv = p * (_edgeCount+1) + e;
				Vector3 normal = Vector3.Cross( _vertices[lv+_tempQuadLookup[3]] - _vertices[lv], _vertices[lv+_tempQuadLookup[1]] - _vertices[lv] );
				normal.Normalize();
				for( int q=0; q<4; q++ ){
					_vertices[v] = _vertices[lv+_tempQuadLookup[q]];
					_normals[v].Set( normal.x, normal.y, normal.z );
					v++;
				}
			}
		}

		// Copy from the temporary first two segments.
		for( int e=_edgeCount-1; e>=0; e-- ){
			v = e * 4;
			Vector3 normal = Vector3.Cross( _tempVector3s[e+_tempQuadLookup[2]] - _tempVector3s[e], _tempVector3s[e+_tempQuadLookup[1]] - _tempVector3s[e] );
			normal.Normalize();
			for( int q=0; q<4; q++ ){
				_normals[v].Set( normal.x, normal.y, normal.z );
				_vertices[v] = _tempVector3s[e+_tempQuadLookup[q]];
				v++;
			}
		}

		// Calculate tangents.
		if( _calculateTangents ){
			if( _radiuses.Length == 0 ){
				for( int p=0; p<_points.Length-1; p++ ){
					Vector4 tangent = _directions[p];
					tangent.w = -1;
					v = p * _edgeCount * 4;
					for( int e=0; e<_edgeCount; e++ ) for( int q=0; q<4; q++ ) _tangents[v++] = tangent;
				}
			} else {
				for( int p=0; p<_points.Length-1; p++ ){
					for( int e=0; e<_edgeCount; e++ ){
						v = p * _edgeCount * 4 + e * 4;
						if( e == 0 ){
							_tangents[v] = ( _vertices[v+1] - _vertices[v] ).normalized;
							_tangents[v].w = -1;
						} else {
							_tangents[v] = _tangents[v-1];
						}
						if( e == _edgeCount-1 ){
							_tangents[v+2] = _tangents[v-(_edgeCount-1)*4];
						} else {
							_tangents[v+2] = ( _vertices[v+2] - _vertices[v+3] ).normalized;
							_tangents[v+2].w = -1;
						}
						_tangents[v+1] = _tangents[v];
						_tangents[v+3] = _tangents[v+2];
					}
				}
			}
		}
	}


	void ReDrawHardNormalEdges()
	{
		int v;
		bool usingRadiuses = radiuses.Length != 0;

		// Store the first two segments temporarily to avoid overwriting what we are about to read.
		int ringVertexCount = (_edgeCount + 1) * 2;
		if( _tempVector3s == null || _tempVector3s.Length != ringVertexCount ) _tempVector3s = new Vector3[ringVertexCount];
		for( int p=0; p<2; p++ ){
			for( int e=0; e<_edgeCount+1; e++ ){
				v = p * (_edgeCount+1) + e;
				_tempVector3s[v] = _vertices[v];
			}
		}

		// Go backwards and copy from already calculated vertices.
		int invertSign = _invertMesh ? -1 : 1;
		for( int p=_points.Length-1; p>1; p-- ){ // don't copy vertices from first segment
			for( int e=_edgeCount-1; e>=0; e-- ){
				v = p * _edgeCount * 2 + e * 2;
				int lv = p * (_edgeCount+1) + e;
				if( usingRadiuses ) _normals[v] = _rotations[p] * Quaternion.AngleAxis( _steepnessAngles[p], _circleTangentLookup[e] ) * _circleNormalLookup[e] * invertSign;
				else _normals[v] = _rotations[p] * _circleNormalLookup[e] * invertSign;
				_normals[v+1] = _normals[v];
				_vertices[v] = _vertices[lv];
				_vertices[v+1] = _vertices[lv+1];
			}
		}

		// Copy from the temporary first two segments.
		for( int p=1; p>=0; p-- ){
			for( int e=_edgeCount-1; e>=0; e-- ){
				v = p * _edgeCount * 2 + e * 2;
				int lv = p * (_edgeCount+1) + e;
				if( usingRadiuses ) _normals[v] = _rotations[p] * Quaternion.AngleAxis( _steepnessAngles[p], _circleTangentLookup[e] ) * _circleNormalLookup[e] * invertSign;
				else _normals[v] = _rotations[p] * _circleNormalLookup[e] * invertSign;
				_normals[v+1] = _normals[v];
				_vertices[v] = _tempVector3s[lv];
				_vertices[v+1] = _tempVector3s[lv+1];
			}
		}

		// Calculate tangents.
		if( _calculateTangents ){
			if( usingRadiuses ){
				for( int p=0; p<_points.Length; p++ ){
					for( int e=0; e<_edgeCount; e++ ){
						v = p * _edgeCount * 2 + e * 2;
						if( e == 0 ){
							_tangents[v] = Vector3.Cross( _rotations[p] * _circleTangentLookup[e], _normals[v] );
							_tangents[v].w = -1;
						} else {
							_tangents[v] = _tangents[v-1];
						}
						if( e == _edgeCount-1 ){
							_tangents[v+1] = _tangents[v-(_edgeCount-1)*2];
						} else {
							_tangents[v+1] = Vector3.Cross( _rotations[p] * _circleTangentLookup[e], _normals[v+1] );
							_tangents[v+1].w = -1;
						}
					}
				}
			} else {
				for( int p=0; p<_points.Length; p++ ){
					Vector4 tangent = _directions[p].normalized;
					tangent.w = -1;
					v = p * _edgeCount * 2;
					for( int e=0; e<_edgeCount; e++ ){
						_tangents[v++] = tangent;
						_tangents[v++] = tangent;
					}
				}
			}
		}
	}
	
	
	void UpdateCircleLookup()
	{
		if( _circlePointLookup == null || _circlePointLookup.Length != _edgeCount ){
			_circlePointLookup = new Vector3[ _edgeCount ];
			_circleNormalLookup = new Vector3[ _edgeCount ];
			_circleTangentLookup = new Vector3[ _edgeCount ];
		}
		
		float normalizer = 1 / (float) _edgeCount;
		float forwardAngleOffsetRad = _forwardAngleOffset * Mathf.Deg2Rad;
		for( int e=0; e<_edgeCount; e++ ){
			float pointAngle = e * normalizer * tau + forwardAngleOffsetRad;
			_circlePointLookup[e] = new Vector3( Mathf.Cos( pointAngle ), Mathf.Sin( pointAngle ), 0 );
			if( _normalMode == NormalMode.HardEdges ){
				float normalAngle = pointAngle + normalizer * Mathf.PI;
				_circleNormalLookup[e] = new Vector3( Mathf.Cos( normalAngle ), Mathf.Sin( normalAngle ), 0 );
			} else {
				_circleNormalLookup[e] = _circlePointLookup[e];
			}
			_circleTangentLookup[e] = Vector3.Cross( _circleNormalLookup[e], Vector3.forward );
		}

		_dirtyCircle = false;
	}
	
	
	void UpdateRotations()
	{

		bool hasLoop = _points[0] == _points[_points.Length-1];
		bool usingTwists = twists.Length == _points.Length;

		// Update array lengths.
		if( _rotations == null || _points.Length != _rotations.Length ){
			_rotations = new Quaternion[ _points.Length ];
			_directions = new Vector3[ _points.Length ];
			_lengths = new float[ _points.Length ];
		}
		
		// Calculate directions and lengths.
		_length = 0;
		for( int p=0; p<points.Length-1; p++ ){
			_directions[p] = _points[p+1] - _points[p];
			_lengths[p] = _directions[p].magnitude;
		}

		// Normalize directions and compute length.
		for( int p=0; p<points.Length-1; p++ ){
			if( _lengths[p] > 0 ){
				_directions[p] /= _lengths[p];
				_length += _lengths[p];
			} else {
				// Length is zero. Grab the previous (now normalized) direction, or if first point, try grab the next direction.
				if( p > 0 ){
					_directions[p] = _directions[p-1];
				} else {
					if( _lengths[p+1] > 0 ){
						_directions[p] = _directions[p+1] / _lengths[p+1];
					} else {
						_directions[p] = new Vector3( 0, 1, 0 ); // Last resort.
					}
				}
			}
		}
			
		// Duplicate last direction.
		_directions[ _points.Length-1 ] = _directions[ _points.Length-2 ];
		
		// Compute forward direction.
		Vector3 forward = hasLoop ? (_directions[_points.Length-1] + _directions[0] ).normalized : _directions[0];

		// If the past up direction has not been set in last frame or we are not playing then compute initial up.
		Vector3 up;
		bool isFirstFrame = _pastBeginUp == Vector3.zero;
		if( isFirstFrame ) {
			up = Mathf.Abs( Vector3.Dot( _directions[0], Vector3.right ) ) < 0.5f ? Vector3.right : Vector3.up;
		} else {
			up = _pastBeginUp;
		}
 
		// Compute begin point rotation.
		Vector3 right = Vector3.Cross( up, forward );
		if( !isFirstFrame ) {
			up = Vector3.Cross( forward, right );
		}
		_pastBeginUp = up;
		_rotations[0].SetLookRotation( forward, up );
		if( usingTwists ) _rotations[0] = Quaternion.AngleAxis( _twists[0], forward ) * _rotations[0];

		// Compute rotations for all points.
		for( int p=1; p<points.Length-1; p++ )
		{
			forward.Set( _directions[p].x + _directions[p-1].x, _directions[p].y + _directions[p-1].y, _directions[p].z + _directions[p-1].z );

			// In case the forward vector is zero (probably because the last
			// point was at the same position at this point).
			if( forward == Vector3.zero ){
				_rotations[p] = _rotations[p-1];
				continue;
			}

			// For better corners ...
			forward.Normalize();

			// To find the optimal up-rotation of the circle plane we do the following:
			// The cross product of last points up and forward gives us a vector that is rotated 90
			// degrees right on the forward axis from the new up vector. Taking the cross 
			// product of forward and right gives us the new up vector.
			// Vector3 up = Vector3.Cross( Vector3.forward, Vector3.right );
			// http://en.wikipedia.org/wiki/Right-hand_rule

			right = Vector3.Cross( up, forward ); // up is last up
			//right.Normalize();

			up = Vector3.Cross( forward, right );
			//up.Normalize();

			// Create a Quaternion rotation using LookRotation
			_rotations[p].SetLookRotation( forward, up );

			// Twist.
			if( usingTwists ) _rotations[p] = Quaternion.AngleAxis( _twists[p], forward ) * _rotations[p];
		}

		// Compute end point rotation.
		if( hasLoop ){
			_rotations[_points.Length-1] = _rotations[0];
		} else {
			forward = _directions[_points.Length-1];
			if( forward == Vector3.zero ){
				_rotations[_points.Length-1] = _rotations[_points.Length-2];
			} else {
				right = Vector3.Cross( up, forward );
				up = Vector3.Cross( forward, right );
				_rotations[_points.Length-1].SetLookRotation( forward, up );
			}
			if( usingTwists ) _rotations[_points.Length-1] = Quaternion.AngleAxis( _twists[_points.Length-1], forward ) * _rotations[_points.Length-1];
		}

		// If the tube is looping, then figure out how much forward twist is needed to make ends meet, and apply it.
		if( hasLoop ){
			Vector3 beginRight = _rotations[0] * Vector3.right;
			Vector3 endRight = _rotations[_rotations.Length-2] * Vector3.right;
			float angleOffsetStep = Vector3.Angle( beginRight, endRight );
			if( Vector3.Dot( forward, Vector3.Cross( beginRight, endRight ) ) > 0 ) angleOffsetStep *= -1;
			angleOffsetStep /= (float) _points.Length-1f;
			for( int r = 1; r < _rotations.Length-1; r++ ) {
				_rotations[r] = Quaternion.AngleAxis( r * angleOffsetStep, _rotations[r] * Vector3.forward ) * _rotations[r];
			}
		}

		_dirtyRotations = false;
	}


	void UpdateSteepnessAngles()
	{
		// If we have the same radius then all steepness angles will be zero.
		if( _radiuses.Length == 0 ) return;

		// Update array length.
		if( _steepnessAngles == null || _steepnessAngles.Length != _points.Length ){
			_steepnessAngles = new float[ _points.Length ];
		}

		// Compute differences in radius.
		if( _radiusDiffs == null || _radiusDiffs.Length != _points.Length-1 ) _radiusDiffs = new float[ _points.Length-1 ];
		int radiusCount = _radiuses.Length;
		for( int p=0; p<_points.Length-1; p++ ){
			_radiusDiffs[p] = _radiuses[ (p+1)%radiusCount ] - _radiuses[ p%radiusCount ];
		}

		// Compute steepness.
		for( int p=0; p<_points.Length-1; p++ ){
			float avgRadiusDiff;
			if( p == 0 ) avgRadiusDiff = _radiusDiffs[0];
			else avgRadiusDiff = ( _radiusDiffs[p] + _radiusDiffs[p-1] ) * 0.5f;
			float avgRadiusDiffAbs = avgRadiusDiff > 0 ? avgRadiusDiff : -avgRadiusDiff;
			if( avgRadiusDiffAbs <= 0 ) _steepnessAngles[p] = 0;
			else _steepnessAngles[p] = -Mathf.Atan2( avgRadiusDiff, _lengths[p] ) * Mathf.Rad2Deg;
		}
		_steepnessAngles[_points.Length-1] = -Mathf.Atan2( _radiusDiffs[_points.Length-2], _lengths[_points.Length-1] ) * Mathf.Rad2Deg;

		_dirtySteepnessAngles = false;
	}
	
	
	void UpdateTriangles()
	{
		// Update array length.
		int triangleCount = (points.Length-1) * _edgeCount * 3 * 2;
		if( _caps == CapMode.Both || _caps == CapMode.Begin ) triangleCount += _edgeCount * 3;
		if( _caps == CapMode.Both || _caps == CapMode.End ) triangleCount += _edgeCount * 3;
		if( _triangles == null || _triangles.Length != triangleCount ) _triangles = new int[triangleCount];

		// Stitch the tube.
		int v=0; int t=0;
		int[] quad;
		switch( _normalMode ){

		case NormalMode.Smooth:
			if(!_invertMesh) quad = new int[]{ 0, 1, _edgeCount+2, 0, _edgeCount+2, _edgeCount+1 };
			else quad = new int[]{ 0, _edgeCount+2, 1, 0, _edgeCount+1, _edgeCount+2 };
			for( int p=0; p<points.Length-1; p++ ){
				for( int e=0; e<_edgeCount; e++ ){
					for( int q = 0; q < quad.Length; q++ ) _triangles[ t++ ] = v + quad[ q ];
					v++;
				}
				v++; // skip hidden vertex
			}
			v += _edgeCount+1; // skip last point
			break;

		case NormalMode.Hard:
			if(!_invertMesh) quad = new int[]{ 0, 3, 1, 3, 2, 1 };
			else quad = new int[]{ 0, 1, 3, 3, 1, 2 };
			for( int p=0; p<points.Length-1; p++ ){
				for( int e=0; e<_edgeCount; e++ ){
					for( int q = 0; q < quad.Length; q++ ) _triangles[ t++ ] = v + quad[ q ];
					v += 4;
				}
			}
			break;

		case NormalMode.HardEdges:
			if(!_invertMesh) quad = new int[]{ 0, 1, _edgeCount*2, _edgeCount*2, 1, _edgeCount*2+1 };
			else quad = new int[]{ 0, _edgeCount*2, 1, 1, _edgeCount*2, _edgeCount*2+1 };
			for( int p=0; p<points.Length-1; p++ ){
				for( int e=0; e<_edgeCount; e++ ){
					for( int q = 0; q < quad.Length; q++ ) _triangles[ t++ ] = v + quad[ q ];
					v += 2;
				}
			}
			v += _edgeCount*2;
			break;

		}

		// Stitch the begin cap.
		if( _caps  == CapMode.Both || _caps  == CapMode.Begin ){
			int vCenter = v + _edgeCount+1;
			if(!_invertMesh){ // ugly but fast
				for( int e=0; e<_edgeCount; e++ ){
					_triangles[ t++ ] = v;
					_triangles[ t++ ] = vCenter;
					_triangles[ t++ ] = v+1;
					v++;
				}
			} else {
				for( int e=0; e<_edgeCount; e++ ){
					_triangles[ t++ ] = v;
					_triangles[ t++ ] = v+1;
					_triangles[ t++ ] = vCenter;
					v++;
				}
			}
		}

		// Stitch the end cap.
		if( _caps  == CapMode.Both || _caps  == CapMode.End ){
			if( _caps  == CapMode.Both ){
				v++; // skip hidden vertex
				v++; // skip center vertex
			}
			int vCenter = v + _edgeCount+1;
			if(!_invertMesh){ // ugly but fast
				for( int e=0; e<_edgeCount; e++ ){
					_triangles[ t++ ] = v;
					_triangles[ t++ ] = v+1;
					_triangles[ t++ ] = vCenter;
					v++;
				}
			} else {
				for( int e=0; e<_edgeCount; e++ ){
					_triangles[ t++ ] = v;
					_triangles[ t++ ] = vCenter;
					_triangles[ t++ ] = v+1;
					v++;
				}
			}
		}

		// Upload.
		_mesh.triangles = _triangles;

		_dirtyTriangles = false;
	}


	void UpdateUVs()
	{
		float u, v;
		if( _uvs == null || _uvs.Length != _vertices.Length ) _uvs = new Vector2[ _vertices.Length ];
		int uv = 0;
		float uStep = 1 / ( _uvAntiStretching ? _length : ( _points.Length -1f ) );
		float vStep = 1 / (float) _edgeCount;
		float uPos = 0;
		
		switch( _normalMode )
		{
			case NormalMode.Smooth:
				for( int p=0; p<points.Length; p++ ){
					u =  _uvRect.xMin + _uvRect.width * (uPos*uStep);
					for( int e=0; e<_edgeCount+1; e++ ){
						v = _uvRect.yMin + _uvRect.height * (e*vStep);
						_uvs[ uv++ ].Set( u, v );
					}
					uPos += _uvAntiStretching ? _lengths[p] : 1;
				}
				break;
				
			case NormalMode.Hard:
				for( int p=0; p<points.Length-1; p++ ){
					u =  _uvRect.xMin + _uvRect.width * (uPos*uStep);
					float nextUPos = uPos + ( _uvAntiStretching ? _lengths[p] : 1 );
					float nextU = _uvRect.xMin + _uvRect.width * (nextUPos*uStep);
					for( int e=0; e<_edgeCount; e++ ){
						v = _uvRect.yMin + _uvRect.height * (e*vStep);
						float nextV = _uvRect.yMin + _uvRect.height * ((e+1)*vStep);
						_uvs[ uv++ ].Set( u, v );
						_uvs[ uv++ ].Set( nextU, v );
						_uvs[ uv++ ].Set( nextU, nextV );
						_uvs[ uv++ ].Set( u, nextV );
					}
					uPos += _uvAntiStretching ? _lengths[p] : 1;
				}
				break;
			case NormalMode.HardEdges:
				for( int p=0; p<points.Length; p++ ){
					u =  _uvRect.xMin + _uvRect.width * (uPos*uStep);
					for( int e=0; e<_edgeCount; e++ ){
						v = _uvRect.yMin + _uvRect.height * (e*vStep);
						float nextV = _uvRect.yMin + _uvRect.height * ((e+1)*vStep);
						_uvs[ uv++ ].Set( u, v );
						_uvs[ uv++ ].Set( u, nextV );
					}
					uPos += _uvAntiStretching ? _lengths[p] : 1;
				}
				break;
		}

		if( _caps == CapMode.Both || _caps  == CapMode.Begin ){
			for( int e=0; e<_edgeCount; e++ ){
				u = _uvRectCap.yMin + _uvRectCap.height * ( _circlePointLookup[e].y*0.5f+0.5f );
				v = _uvRectCap.xMin + _uvRectCap.width * ( 1-(_circlePointLookup[e].x*0.5f+0.5f) );
				_uvs[ uv++ ].Set( u, v );
			}
			_uvs[uv] = _uvs[uv-_edgeCount]; // uv wrap
			uv++;
			u = _uvRectCap.yMin + _uvRectCap.height * 0.5f;
			v = _uvRectCap.xMin + _uvRectCap.width * 0.5f;
			_uvs[ uv++ ].Set( u, v ); // center
		}
		
		if( _caps == CapMode.Both || _caps  == CapMode.End ){
			for( int e=0; e<_edgeCount; e++ ){
				if( _uvRectCapEndMirrored ) u = _uvRectCap.yMin + _uvRectCap.height * ( _circlePointLookup[e].y*0.5f+0.5f );
				else u = _uvRectCap.yMin + _uvRectCap.height * ( 1-(_circlePointLookup[e].y*0.5f+0.5f) );
				v = _uvRectCap.xMin + _uvRectCap.width * ( 1-(_circlePointLookup[e].x*0.5f+0.5f) );
				_uvs[ uv++ ].Set( u, v );
			}
			_uvs[uv] = _uvs[uv-_edgeCount]; // uv wrap
			uv++;
			u = _uvRectCap.yMin + _uvRectCap.height * 0.5f;
			v = _uvRectCap.xMin + _uvRectCap.width * 0.5f;
			_uvs[ uv++ ].Set( u, v ); // center
		}
		
		_mesh.uv = _uvs;

		_dirtyUVs = false;
	}

	
	void UpdateColors()
	{
		if( _pointColors.Length == 0 ){
			if( mesh.colors32.Length != 0 ) mesh.colors32 = new Color32[0];
			_colors32 = null;
			_dirtyColors = false;
			return;
		}
			
		if( _colors32 == null || _colors32.Length != _vertices.Length ) _colors32 = new Color32[ _vertices.Length ];

		int v = 0;
		switch( _normalMode ){

		case NormalMode.Smooth:
			for( int p=0; p<_points.Length; p++ ) {
				int c = p % _pointColors.Length;
				for( int s=0; s<_edgeCount+1; s++ ) _colors32[ v++ ] = _pointColors[ c ];
			}
			break;

		case NormalMode.Hard:
			for( int p=0; p<_points.Length-1; p++ ) {
				int c0 = p % _pointColors.Length;
				int c1 = (p+1) % _pointColors.Length;
				for( int s=0; s<_edgeCount; s++ ){
					_colors32[ v++ ] = _pointColors[ c0 ];
					_colors32[ v++ ] = _pointColors[ c1 ];
					_colors32[ v++ ] = _pointColors[ c1 ];
					_colors32[ v++ ] = _pointColors[ c0 ];
				}
			}
			break;

		case NormalMode.HardEdges:
			for( int p=0; p<_points.Length; p++ ) {
				int c = p % _pointColors.Length;
				for( int s=0; s<_edgeCount; s++ ){
					_colors32[ v++ ] = _pointColors[c];
					_colors32[ v++ ] = _pointColors[c];
				}
			}
			break;
		}

		if( _caps == CapMode.Both || _caps == CapMode.Begin ){
			for( int s=0; s<_edgeCount+2; s++ ) _colors32[ v++ ] = _pointColors[ 0 ]; // start cap
		}
		if( _caps == CapMode.Both || _caps == CapMode.End ){
			for( int s=0; s<_edgeCount+2; s++ ) _colors32[ v++ ] = _pointColors[ (_points.Length-1) % colors.Length ]; // end cap
		}

		mesh.colors32 = _colors32;

		_dirtyColors = false;
	}


	static int ComputeVertexCountForProperties( int pointCount, int edgeCount, NormalMode normalMode, CapMode capMode )
	{
		int targetVertexCount = 0;

		switch( normalMode ){
		case NormalMode.Smooth: targetVertexCount = pointCount * (edgeCount+1); break;
		case NormalMode.Hard: targetVertexCount = (pointCount-1) * edgeCount * 4; break;
		case NormalMode.HardEdges: targetVertexCount = pointCount * 2 * edgeCount; break;
		}
		if( capMode == CapMode.Both ) targetVertexCount += ( (edgeCount+1) + 1 ) * 2;
		else if( capMode == CapMode.Begin || capMode == CapMode.End ) targetVertexCount += (edgeCount+1) + 1;

		return targetVertexCount;
	}
}