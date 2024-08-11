/*
	Copyright © Carl Emil Carlsen 2021
	http://cec.dk
*/

using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PlotInternals;


public partial class Plot
{
	[Serializable]
	public class Polyline : MeshDependentShape
	{
		int[] _indices;
		Vertex[] _vertexData;

		float[] _positionsAlongLine;

		StrokeCap _beginCap = StrokeCap.Round;
		StrokeCap _endCap = StrokeCap.Round;
		StrokeCornerProfile _strokeCornerProfile = StrokeCornerProfile.Round;

		Vector2 _posMin, _posMax;

		protected override bool dirtyMesh => _dirtyVerticies;

		static VertexAttributeDescriptor[] vertexDataLayout = new VertexAttributeDescriptor[]
		{
			new VertexAttributeDescriptor( VertexAttribute.Position, VertexAttributeFormat.Float32, 2 ),
			new VertexAttributeDescriptor( VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4 ),
			new VertexAttributeDescriptor( VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4 ),
			new VertexAttributeDescriptor( VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 4 ),
		};

		[Serializable]
		struct Vertex
		{
			public Vector2 position;
			public Vector4 outwards_sideMult_endmult;
			public Vector4 points;
			public Vector4 roundedCaps_posAlongLineA_posAlongLineB;
		}

		public Polyline() { }


		public Polyline( int pointCount )
		{
			SetPointCount( pointCount );
		}


		/// <summary>
		/// Creates a new Polyline to be drawn using Plot.DrawPolyline(). Points must be provided in clockwise order. 
		/// </summary>
		public Polyline( Vector2[] points )
		{
			SetPoints( points );
		}


		/// <summary>
		/// Creates a new Polyline to be drawn using Plot.DrawPolyline(). Points must be provided in clockwise order. 
		/// </summary>
		public Polyline( List<Vector2> points )
		{
			SetPoints( points );
		}


		/// <summary>
		/// Fill this polyline with bezier curve points.
		/// </summary>
		public Polyline SetAsBezierCurve( Vector2 anchorA, Vector2 controlA, Vector2 controlB, Vector2 anchorB, int resolution = 32 )
		{
			if( resolution < 3 ) resolution = 3;
			if( _points == null || _points.Length != resolution ) _points = new Vector2[ resolution ];
			_points[ 0 ] = anchorA;
			_points[ resolution - 1 ] = anchorB;
			float step = 1 / ( resolution - 1f );
			for( int r = 1; r < resolution - 1; r++ ) {
				float t = r * step;
				float x = PlotMath.QuadraticInterpolation( anchorA.x, controlA.x, controlB.x, anchorB.x, t );
				float y = PlotMath.QuadraticInterpolation( anchorA.y, controlA.y, controlB.y, anchorB.y, t );
				_points[ r ] = new Vector2( x, y );
			}
			_dirtyVerticies = true;

			return this;
		}


		// Undocumented on purpose.
		public void AdaptAndGetMesh( bool drawNow, StrokeCap beginCap, StrokeCap endCap, StrokeCornerProfile strokeCornerProfile, out Mesh mesh )
		{
			if( beginCap != _beginCap || endCap != _endCap || strokeCornerProfile != _strokeCornerProfile ) {
				_beginCap = beginCap;
				_endCap = endCap;
				_strokeCornerProfile = strokeCornerProfile;
				_dirtyVerticies = true;
			}

			bool reuseMesh = drawNow || !_dirtyVerticies;
			EnsureAvailableMesh( reuseMesh );

			if( _dirtyVerticies ) Build();

			mesh = _mesh;
		}



		protected override void ApplyMeshData()
		{
			Vector2 size = _posMax - _posMin;
			if( _mesh.vertexCount != _vertexData.Length ) _mesh.Clear();
			SubMeshDescriptor meshDescriptor = new SubMeshDescriptor() {
				vertexCount = _vertexData.Length,
				indexCount = _indices.Length,
				topology = MeshTopology.Quads,
				bounds = new Bounds( new Vector3( _posMin.x + size.x * 0.5f, _posMin.y + size.y * 0.5f ), size ) // TODO: We are not taking stroke width into account here =(
			};
			_mesh.SetVertexBufferParams( _vertexData.Length, vertexDataLayout );
			_mesh.SetIndexBufferParams( _indices.Length, IndexFormat.UInt32 ); // TODO: Rewrite Earcut.Tessellate to work with ushort indices.
			_mesh.subMeshCount = 1;
			_mesh.SetSubMesh( 0, meshDescriptor, meshFlags );

			_mesh.SetVertexBufferData( _vertexData, 0, 0, _vertexData.Length, 0, meshFlags );
			_mesh.SetIndexBufferData( _indices, 0, 0, _indices.Length, meshFlags );
			_mesh.bounds = meshDescriptor.bounds;
		}


		void Build()
		{
			// Sanity check.
			if( _points.Length < 2 ) {
				_mesh.Clear();
				return;
			}

			// Adapt arrays
			int pointCount = _points.Length;
			int vertexCount = ( pointCount - 1 ) * 4;
			int quadIndexCount = vertexCount;
			if( _vertexData == null || _vertexData.Length != vertexCount ) {
				_vertexData = new Vertex[ vertexCount ];
				_indices = new int[ quadIndexCount ];
			}

			// Compute directions.
			PlotMath.ComputeNormalizedDirections( _points, ref _directions, ref _positionsAlongLine );

			// Update vertices.
			int lastP = pointCount - 1;
			int i = 0;
			Vertex vertex = new Vertex();
			Vector2 prevPoint = Vector2.zero;
			Vector2 prevOutwards90 = Vector2.zero;
			Vector4 capsAndPos1D = Vector4.zero;
			_posMin = _points[ 0 ];
			_posMax = _posMin;
			int strokeCornerMode = _strokeCornerProfile == StrokeCornerProfile.Round ? 2 : 1;
			float endMult;
			float thisPosAlongLine = 0;
			for( int thisP = 0; thisP < pointCount; thisP++ ) {
				int prevP = thisP - 1;

				int prevV0 = prevP * 4;
				int prevV2 = prevV0 + 2;
				int prevV3 = prevV0 + 3;

				int thisV0 = thisP * 4;
				int thisV1 = thisV0 + 1;
				int thisV2 = thisV0 + 2;
				int thisV3 = thisV0 + 3;

				Vector2 thisPoint = _points[ thisP ];
				Vector2 thisDir = _directions[ thisP ];
				Vector2 thisOutwards90 = new Vector2( thisDir.y, -thisDir.x ); // Rotate 90 degrees
				Vector2 outwardsLeft;

				if( thisPoint.x < _posMin.x ) _posMin.x = thisPoint.x;
				else if( thisPoint.x > _posMax.x ) _posMax.x = thisPoint.x;
				if( thisPoint.y < _posMin.y ) _posMin.y = thisPoint.y;
				else if( thisPoint.y > _posMax.y ) _posMax.y = thisPoint.y;

				if( thisP < lastP ) {
					int nextP = thisP + 1;

					Vector2 nextPoint = _points[ nextP ];
					Vector4 points = new Vector4( thisPoint.x, thisPoint.y, nextPoint.x, nextPoint.y );
					float nextPosAlongLine = _positionsAlongLine[ nextP ];

					if( thisP == 0 ) {
						// First point (and segment).
						capsAndPos1D.SetXY( (int) _beginCap, strokeCornerMode );
						outwardsLeft = thisOutwards90;
						endMult = -1;

						if( pointCount == 2 ) {
							// First segment is also the last segment.
							capsAndPos1D.y = (int) _endCap;
						}

					} else {
						// Other points.
						if( PlotMath.TryIntersectLineLine( prevPoint + prevOutwards90, thisPoint + prevOutwards90, thisPoint + thisOutwards90, nextPoint + thisOutwards90, out outwardsLeft ) ) {
							outwardsLeft.Subtract( thisPoint );
						} else {
							outwardsLeft = thisOutwards90; // Parallel.
						}
						endMult = 0;

						if( thisP == lastP - 1 ) {
							// Last segment.
							capsAndPos1D.SetXY( strokeCornerMode, (int) _endCap );
						} else {
							// Other segments.
							capsAndPos1D.SetXY( strokeCornerMode, strokeCornerMode );
						}

						vertex = _vertexData[ prevV2 ];
						vertex.position = thisPoint;
						vertex.outwards_sideMult_endmult = new Vector4( outwardsLeft.x, outwardsLeft.y, 1, endMult );
						_vertexData[ prevV2 ] = vertex;
						vertex = _vertexData[ prevV3 ];
						vertex.position = thisPoint;
						vertex.outwards_sideMult_endmult = new Vector4( outwardsLeft.x, outwardsLeft.y, -1, endMult );
						_vertexData[ prevV3 ] = vertex;
					}

					capsAndPos1D.SetZW( thisPosAlongLine, nextPosAlongLine );

					// Segment or first point.

					vertex.position = thisPoint;
					vertex.outwards_sideMult_endmult = new Vector4( outwardsLeft.x, outwardsLeft.y, 1, endMult );
					vertex.points = points;
					vertex.roundedCaps_posAlongLineA_posAlongLineB = capsAndPos1D;
					_vertexData[ thisV0 ] = vertex;

					vertex.outwards_sideMult_endmult.z = -1;
					_vertexData[ thisV1 ] = vertex; // Reusing some vertex data from thisV0.

					vertex = _vertexData[ thisV2 ];
					vertex.points = points;
					vertex.roundedCaps_posAlongLineA_posAlongLineB = capsAndPos1D;
					_vertexData[ thisV2 ] = vertex;

					vertex = _vertexData[ thisV3 ];
					vertex.points = points;
					vertex.roundedCaps_posAlongLineA_posAlongLineB = capsAndPos1D;
					_vertexData[ thisV3 ] = vertex;

					_indices[ i++ ] = thisV0;
					_indices[ i++ ] = thisV1;
					_indices[ i++ ] = thisV3;
					_indices[ i++ ] = thisV2;

					thisPosAlongLine = nextPosAlongLine;

				} else {
					// Last point.
					endMult = 1;

					vertex = _vertexData[ prevV2 ];
					vertex.position = thisPoint;
					vertex.outwards_sideMult_endmult = new Vector4( thisOutwards90.x, thisOutwards90.y, 1, endMult );
					_vertexData[ prevV2 ] = vertex;

					vertex = _vertexData[ prevV3 ];
					vertex.position = thisPoint;
					vertex.outwards_sideMult_endmult = new Vector4( thisOutwards90.x, thisOutwards90.y, -1, endMult );
					_vertexData[ prevV3 ] = vertex;
				}

				prevPoint = thisPoint;
				prevOutwards90 = thisOutwards90;
			}

			// Update mesh.
			ApplyMeshData();

			// Update flags.
			_dirtyVerticies = false;
		}
	}
}