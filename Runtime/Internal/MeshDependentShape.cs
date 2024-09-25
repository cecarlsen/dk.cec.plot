/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlotInternals
{
	[Serializable]
	abstract public class MeshDependentShape : ScriptableObject
	{
		protected Queue<(int,Mesh)> _framedMeshPool;
		protected Mesh _mesh;
		protected int _meshSubmissionFrame = -1;

		protected bool _dirtyVerticies;

		protected Vector2[] _points;
		protected Vector2[] _directions;


		public int pointCount { get { return _points == null ? 0 : _points.Length; } }


		protected abstract bool dirtyMesh { get; }


		protected abstract void ApplyMeshData();

		protected const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;


		/// <summary>
		/// Set point count. This will adapt the internal arrays erasing current content.
		/// </summary>
		public void SetPointCount( int pointCount )
		{
			if( _points == null || _points.Length != pointCount ) _points = new Vector2[ pointCount ];

			_dirtyVerticies = true;
		}


		/// <summary>
		/// Set point at index. Points must be provided in clockwise order.
		/// </summary>
		public void SetPoint( int index, Vector2 point )
		{
			_points[ index ] = point;

			_dirtyVerticies = true;
		}
		public void SetPoint( int index, float x, float y ) { SetPoint( index, new Vector2( x, y ) ); }


		/// <summary>
		/// Set points. Points must be provided in clockwise order. Points will be copied.
		/// </summary>
		public void SetPoints( Vector2[] points )
		{
			if( _points == null || _points.Length != points.Length ) _points = new Vector2[ points.Length ];
			Array.Copy( points, 0, points, 0, points.Length );

			_dirtyVerticies = true;
		}
		public void SetPoints( List<Vector2> points )
		{
			int count = points.Count;
			if( _points == null || _points.Length != count ) _points = new Vector2[ count ];
			points.CopyTo( _points, 0 );

			_dirtyVerticies = true;
		}


		/// <summary>
		/// Get point at index.
		/// </summary>
		public Vector2 GetPoint( int index )
		{
			if( _points == null || index < 0 || index >= _points.Length ) return Vector2.zero;
			return _points[ index ];
		}


		static Mesh CreateMesh()
		{
			return new Mesh()
			{
				name = "Shape",
				hideFlags = HideFlags.HideAndDontSave
			};
		}


		protected void EnsureAvailableMeshBeforeSubmission( bool reuse )
		{
			if( reuse ) {
				// No need for pooling when there is no mesh change (then we use instancing) or the shape is drawn immediately.
				if( !_mesh ) _mesh = CreateMesh();
			} else {
				// Handle pooling. Ensure that we don't return the same mesh twice within one frame.
				int currentFrame = Time.frameCount;
				if( !_mesh ) {
					_mesh = CreateMesh();
				} else if( _meshSubmissionFrame == currentFrame ) {
					if( _framedMeshPool == null ) _framedMeshPool = new Queue<(int,Mesh)>();
					_framedMeshPool.Enqueue( ( currentFrame, _mesh ) );
					var pooledFramedMesh = _framedMeshPool.Peek();
					if( pooledFramedMesh.Item1 == currentFrame ) {
						_mesh = CreateMesh();
					} else {
						_mesh = _framedMeshPool.Dequeue().Item2;
					}
					if( dirtyMesh ) ApplyMeshData();
				}
				_meshSubmissionFrame = currentFrame;
			}
		}


		void OnDisable()
		{
			if( _framedMeshPool != null ){
				foreach( var pair in _framedMeshPool ) if( pair.Item2 ) DestroyImmediate( pair.Item2 );
				_framedMeshPool.Clear();
			}
			if( _mesh ) DestroyImmediate( _mesh );
		}
	}
}