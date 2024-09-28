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
	abstract public class AdaptiveMeshShape : ScriptableObject
	{
		public int _pointCount;
		protected Queue<(int,Mesh)> _framedMeshPool;
		protected Mesh _mesh;
		protected int _meshSubmissionFrame = -1;

		protected bool _dirtyVerticies;

		protected Vector2[] _points;
		protected Vector2[] _directions;

		/// <summary>
		/// The point count.
		/// </summary>
		public int pointCount => _pointCount;

		/// <summary>
		/// The point Capacity.
		/// </summary>
		public int pointCapacity => _points == null ? 0 : _points.Length;

		protected string logPrepend => "<b>[" + this.GetType().Name + "]</b> ";

		protected abstract bool dirtyMesh { get; }


		protected abstract void ApplyMeshData();

		protected const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;


		/// <summary>
		/// Set point capacity. This will adapt the internal arrays erasing current content.
		/// </summary>
		public void SetPointCapacity( int pointCapacity )
		{
			if( _points == null || _points.Length != pointCapacity ) _points = new Vector2[ pointCapacity ];

			_dirtyVerticies = true;
		}


		/// <summary>
		/// Set point count. If point count is greater than point capacity, then the capacity will adapt, preserving the current content.
		/// </summary>
		public void SetPointCount( int pointCount )
		{
			if( pointCount > pointCapacity ){
				var oldArray = _points;
				SetPointCapacity( pointCount );
				if( oldArray != null ) Array.Copy( oldArray, _points, oldArray.Length );
			}

			if( pointCount != _pointCount ){
				_dirtyVerticies = true;
				_pointCount = pointCount;
			}
		}


		/// <summary>
		/// Set point at index. Points must be provided in clockwise order and 
		/// index must be within point capacity.
		/// </summary>
		public void SetPoint( int index, Vector2 point )
		{
			if( index >= _pointCount ){
				Debug.LogWarning( logPrepend + "SetPoint failed. Index is greater or equal to pointCount. Perhaps you forgot to call SetPointCount()?\n" );
				return;
			}
			_points[ index ] = point;
			_dirtyVerticies = true;
		}
		public void SetPoint( int index, float x, float y ) { SetPoint( index, new Vector2( x, y ) ); }


		/// <summary>
		/// Append point, increasing point count (not capacity) by one. Points must be provided in clockwise order and your point count must stay within point capacity.
		/// </summary>
		public void AppendPoint( Vector2 point )
		{
			if( _pointCount+1 >= pointCapacity ){
				Debug.LogWarning( logPrepend + "AppendPoint failed. You are exceeding point capacity. Use SetPointCapacity() to set an appropriate maximum count.\n" );
				return;
			}
			_points[ _pointCount++ ] = point;
			_dirtyVerticies = true;
		}
		public void AppendPoint( float x, float y ) { AppendPoint( new Vector2( x, y ) ); }



		/// <summary>
		/// Prepend point, increasing point count (not capacity) by one. Points must be provided in clockwise order and your point count must stay within point capacity.
		/// </summary>
		public void PreppendPoint( Vector2 point )
		{
			if( _pointCount+1 >= pointCapacity ){
				Debug.LogWarning( logPrepend + "PreppendPoint failed. You are exceeding point capacity. Use SetPointCapacity() to set an appropriate maximum count.\n" );
				return;
			}
			for( int p = _pointCount-1; p > 0; p-- ) _points[ p ] = _points[ p-1 ];
			_points[ 0 ] = point;
			_pointCount++;
			_dirtyVerticies = true;
		}
		public void PreppendPoint( float x, float y ) { PreppendPoint( new Vector2( x, y ) ); }


		/// <summary>
		/// Insert point, increasing point count (not capacity) by one. Points must be provided in clockwise order and your point count must stay within point capacity.
		/// </summary>
		public void InsertPoint( int index, Vector2 point )
		{
			if( _pointCount+1 >= pointCapacity ){
				Debug.LogWarning( logPrepend + "InsertPoint failed. You are exceeding point capacity. Use SetPointCapacity() to set an appropriate maximum count.\n" );
				return;
			}
			for( int p = _pointCount-1; p > index; p-- ) _points[ p ] = _points[ p-1 ];
			_points[ index ] = point;
			_pointCount++;
			_dirtyVerticies = true;
		}
		public void InsertPoint( int index, float x, float y ) { InsertPoint( index, new Vector2( x, y ) ); }


		/// <summary>
		/// Remove point, decreasing point count (not capacity) by one. Points must be provided in clockwise order and your point count must stay within point capacity.
		/// </summary>
		public void RemovePoint( int index )
		{
			if( _pointCount == 0 ){
				Debug.LogWarning( logPrepend + "RemovePoint failed. Point count is zero. Perhaps you forgot to call SetPointCount()?\n" );
				return;
			}
			if( index >= _pointCount ){
				Debug.LogWarning( logPrepend + "RemovePoint failed. Point index is greater or equal to pointCount. Perhaps you forgot to call SetPointCount()?\n" );
				return;
			}
			for( int p = index; p < _pointCount-1; p++ ) _points[ p ] = _points[ p+1 ];
			_pointCount--;
			_dirtyVerticies = true;
		}


		/// <summary>
		/// Set points. Points must be provided in clockwise order. If an array is provided points will be copied.
		/// </summary>
		public void SetPoints( Vector2[] points )
		{
			if( points == null ){
				Debug.LogWarning( logPrepend + "You are provided a null array to SetPoints().\n" );
				return;
			}
			if( points.Length > pointCapacity ) SetPointCapacity( points.Length );
			Array.Copy( points, 0, points, 0, points.Length );

			_pointCount = points.Length;
			_dirtyVerticies = true;
		}
		public void SetPoints( List<Vector2> points )
		{
			if( points == null ){
				Debug.LogWarning( logPrepend + "You are provided a null List to SetPoints().\n" );
				return;
			}
			if( points.Count > pointCapacity ) SetPointCapacity( points.Count );
			points.CopyTo( _points, 0 );

			_pointCount = points.Count;
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