/*
	Copyright © Carl Emil Carlsen 2021
	http://cec.dk
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PlotInternals;

public partial class Plot
{
	[Serializable]
	public class Polygon : MeshDependentShape
	{
		List<int> _indices;
		Vertex[] _vertexData;

		bool _fillEnabled;
		bool _strokeOrAntialiasEnabled;

		bool _dirtyIndices;
		static int[] noHoles = new int[ 0 ];

		Vector2 _posMin, _posMax;

		protected override bool dirtyMesh => _dirtyVerticies || _dirtyIndices;

		static VertexAttributeDescriptor[] vertexDataLayout = new VertexAttributeDescriptor[]
		{
			new VertexAttributeDescriptor( VertexAttribute.Position, VertexAttributeFormat.Float32, 2 ),
			new VertexAttributeDescriptor( VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4 ),
			new VertexAttributeDescriptor( VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 3 ),
		};


		[Serializable]
		struct Vertex
		{
			public Vector2 position;
			public Vector4 points;
			public Vector3 outwards_directionMult;
		}
		

		/// <summary>
		/// Fill this polygon with a N-gon shape.
		/// </summary>
		public Polygon SetAsNGon( float diameter, int sideCount )
		{
			if( sideCount < 3 ) sideCount = 3;

			SetPointCount( sideCount );

			float radius = diameter * 0.5f;
			float angleStep = Tau / (float) sideCount;
			for( int p = 0; p < sideCount; p++ ) {
				float a = p * angleStep;
				_points[ p ] = new Vector2( Mathf.Cos( a ) * radius, -Mathf.Sin( a ) * radius );
			}

			_dirtyVerticies = true;

			return this;
		}


		/// <summary>
		/// Fill this polygon with a star shape.
		/// </summary>
		public Polygon SetAsStar( float innerDiameter, float outerDiameter, int armCount )
		{
			if( armCount < 3 ) armCount = 3;

			int pointCount = armCount * 2;
			SetPointCount( pointCount );

			float innerRadius = innerDiameter * 0.5f;
			float outerRadius = outerDiameter * 0.5f;
			float angleStep = Tau / (float) pointCount;
			for( int p = 0; p < pointCount; p++ ) {
				float a = p * angleStep;
				float radius = p % 2 == 0 ? outerRadius : innerRadius;
				_points[ p ] = new Vector2( Mathf.Cos( a ) * radius, -Mathf.Sin( a ) * radius );
			}

			_dirtyVerticies = true;

			return this;
		}


		// Undocumented on purpose.
		public void AdaptAndGetMesh( bool drawNow, bool fillEnabled, bool strokeEnabled, bool antialias, out Mesh mesh )
		{
			if( fillEnabled != _fillEnabled ) {
				_fillEnabled = fillEnabled;
				_dirtyIndices = true;
			}

			bool strokeOrAntialiasEnabled = strokeEnabled || antialias;
			if( strokeOrAntialiasEnabled != _strokeOrAntialiasEnabled ) {
				_strokeOrAntialiasEnabled = strokeOrAntialiasEnabled;
				_dirtyIndices = true;
			}

			bool meshChange = _dirtyVerticies || _dirtyIndices;
			bool reuseMesh = drawNow || !meshChange;
			EnsureAvailableMesh( reuseMesh );

			if( _dirtyVerticies || _dirtyIndices ) Build();

			mesh = _mesh;
		}


		protected override void ApplyMeshData()
		{
			Vector2 size = _posMax - _posMin;
			SubMeshDescriptor meshDescriptor = new SubMeshDescriptor(){
				vertexCount = _vertexData.Length,
				indexCount = _indices.Count,
				topology = MeshTopology.Triangles,
				bounds = new Bounds( new Vector3( _posMin.x + size.x * 0.5f, _posMin.y + size.y * 0.5f ), size ) // TODO: We are not taking stroke width into account here =(
			};
			_mesh.SetVertexBufferParams( _vertexData.Length, vertexDataLayout );
			_mesh.SetIndexBufferParams( _indices.Count, IndexFormat.UInt32 ); // TODO: Rewrite Earcut.Tessellate to work with ushort indices.
			_mesh.subMeshCount = 1;
			_mesh.SetSubMesh( 0, meshDescriptor, meshFlags );

			_mesh.SetVertexBufferData( _vertexData, 0, 0, _vertexData.Length, 0, meshFlags );
			_mesh.SetIndexBufferData( _indices, 0, 0, _indices.Count, meshFlags );
			_mesh.bounds = meshDescriptor.bounds;
		}


		void Build()
		{
			// Sanity check.
			if( _points.Length < 3 ) {
				_mesh.Clear();
				return;
			}

			if( _dirtyVerticies ) _dirtyIndices = true; // Request index update.

			_posMin = _points[ 0 ];
			_posMax = _posMin;

			if( _dirtyVerticies )
			{
				// Adapt arrays.
				int vertexCount = _points.Length * 5;
				if( _vertexData == null || _vertexData.Length != vertexCount ) _vertexData = new Vertex[ vertexCount ];
				if( _mesh.vertexCount != vertexCount ) _mesh.Clear();

				// Compute directions.
				const bool wrap = true;
				PlotMath.ComputeNormalizedDirections( _points, ref _directions, wrap );

				// Compute vertex data.
				int lastP = pointCount - 1;
				Vector2 prevOutwards90 = new Vector2( -_directions[ lastP ].y, _directions[ lastP ].x ); // Rotate 90 degrees
				Vector2 prevPoint = _points[ lastP ];
				Vector2 thisPoint = _points[ 0 ];
				Vertex vertex = new Vertex();
				for( int thisP = 0; thisP < pointCount; thisP++ )
				{
					if( thisPoint.x < _posMin.x ) _posMin.x = thisPoint.x;
					else if( thisPoint.x > _posMax.x ) _posMax.x = thisPoint.x;
					if( thisPoint.y < _posMin.y ) _posMin.y = thisPoint.y;
					else if( thisPoint.y > _posMax.y ) _posMax.y = thisPoint.y;

					int prevP = thisP > 0 ? thisP - 1 : lastP;
					int nextP = thisP < lastP ? thisP + 1 : 0;

					int prevV1 = pointCount + prevP * 4;
					int prevV3 = prevV1 + 2;
					int prevV4 = prevV1 + 3;
					int thisV1 = pointCount + thisP * 4;
					int thisV2 = thisV1 + 1;

					Vector2 nextPoint = _points[ nextP ];
					Vector2 thisDir = _directions[ thisP ];
					Vector2 thisOutwards90 = new Vector2( -thisDir.y, thisDir.x ); // Rotate 90 degrees

					Vector2 outwards;
					if( PlotMath.TryIntersectLineLine( prevPoint + prevOutwards90, thisPoint + prevOutwards90, thisPoint + thisOutwards90, nextPoint + thisOutwards90, out outwards ) ) {
						outwards -= thisPoint;
					} else {
						outwards = thisOutwards90; // Parallel.
					}

					vertex.position = thisPoint;
					vertex.outwards_directionMult.x = outwards.x;
					vertex.outwards_directionMult.y = outwards.y;

					vertex.points = Vector4.zero;
					vertex.outwards_directionMult.z = -1; // The inner fill is always moved inwards
					_vertexData[ thisP ] = vertex; // This Fill

					vertex.points = new Vector4( prevPoint.x, prevPoint.y, thisPoint.x, thisPoint.y );
					_vertexData[ prevV3 ] = vertex; // Prev Stroke Inner End

					vertex.outwards_directionMult.z = 1; // Outwards
					_vertexData[ prevV4 ] = vertex; // Prev Stroke Outer End

					vertex.points = new Vector4( thisPoint.x, thisPoint.y, nextPoint.x, nextPoint.y );
					vertex.outwards_directionMult.z = -1; // Inwards
					_vertexData[ thisV1 ] = vertex; // This Stroke Inner begin

					vertex.outwards_directionMult.z = 1; // Outwards
					_vertexData[ thisV2 ] = vertex; // This Stroke Outer begin

					prevPoint = thisPoint;
					thisPoint = nextPoint;
					prevOutwards90 = thisOutwards90;
				}
			}

			if( _dirtyIndices )
			{
				// Adap arrays.
				int theoreticalIndexCount = 0;
				if( _fillEnabled ) theoreticalIndexCount += ( _points.Length - 2 ) * 3;
				if( _strokeOrAntialiasEnabled ) theoreticalIndexCount += 6 * _points.Length;
				if( _indices == null ) _indices = new List<int>( theoreticalIndexCount );
				else if( _indices.Capacity < theoreticalIndexCount ) _indices.Capacity = theoreticalIndexCount;

				// Reset list.
				_indices.Clear();

				// Build fill (will clear array).
				if( _fillEnabled ) Earcut.Triangulate( _points, _points.Length, noHoles, ref _indices );

				// Build stroke.
				if( _strokeOrAntialiasEnabled ) {
					for( int thisP = 0; thisP < pointCount; thisP++ ) {
						int thisV1 = pointCount + thisP * 4;
						int thisV2 = thisV1 + 1;
						int thisV3 = thisV1 + 2;
						int thisV4 = thisV1 + 3;

						_indices.Add( thisV1 );
						_indices.Add( thisV2 );
						_indices.Add( thisV4 );
						_indices.Add( thisV4 );
						_indices.Add( thisV3 );
						_indices.Add( thisV1 );
					}
				}
			}

			// Update mesh.
			ApplyMeshData();

			// Update flags.
			_dirtyVerticies = false;
			_dirtyIndices = false;
		}
	}
}