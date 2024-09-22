/*
	Copyright © Carl Emil Carlsen 2021-2024
	http://cec.dk
*/

using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public partial class Plot
{
	[Serializable] 
	public class Text : ScriptableObject
	{
		string _content;
		TextMeshPro _tmp;
		Queue<(int,TextMeshPro)> _framedTmpPool;
		protected int _meshSubmissionFrame = -1;
		static int tmpCounter;



		// Make constructor private to force use of CreateText().
		Text(){}


		void OnEnable()
		{
			_tmp = CreateTextMeshProText();
			_tmp.text = _content;
		}

 
		void OnDisable()
		{
			// "Objects created as hide and don't save must be explicitly destroyed by the owner of the object."
			// https://docs.unity3d.com/ScriptReference/HideFlags.html
			if( _tmp ) DestroyImmediate( _tmp.gameObject );
			if( _framedTmpPool != null ){
				foreach( var pair in _framedTmpPool ) if( pair.Item2 ) DestroyImmediate( pair.Item2 );
				_framedTmpPool.Clear();
			}
		}


		/// <summary>
		/// Set the text content of the Test object.
		/// </summary>
		public void SetContent( string text )
		{
			_content = text;
		}


		public TextMeshPro GetTextMeshPro( bool drawNow )
		{
			EnsureAvailableMeshBeforeSubmission( drawNow );
			return _tmp;
		}


		protected void EnsureAvailableMeshBeforeSubmission( bool reuse )
		{
			if( reuse ) {
				// No need for pooling when there is no mesh change (then we use instancing) or the shape is drawn immediately.
				if( !_tmp ){
					_tmp = CreateTextMeshProText();
					_tmp.text = _content;
				}
			} else {
				// Handle pooling. Ensure that we don't return the same mesh twice within one frame.
				int currentFrame = Time.frameCount;
				if( !_tmp ) {
					_tmp = CreateTextMeshProText(); 
					_tmp.text = _content;
				} else if( _meshSubmissionFrame == currentFrame ) {
					if( _framedTmpPool == null ) _framedTmpPool = new Queue<(int,TextMeshPro)>();
					_framedTmpPool.Enqueue( ( currentFrame, _tmp ) );
					var pooledFramedMesh = _framedTmpPool.Peek();
					if( pooledFramedMesh.Item1 == currentFrame ) {
						_tmp = CreateTextMeshProText();
						_tmp.text = _content;
					} else {
						_tmp = _framedTmpPool.Dequeue().Item2;
						if( _content != _tmp.text ) _tmp.text = _content;
					}
				} else {
					if( _content != _tmp.text ) _tmp.text = _content;
				}
				_meshSubmissionFrame = currentFrame;
			}
		}


		static TextMeshPro CreateTextMeshProText()
		{
			tmpCounter++;
			var tmp = new GameObject( "PlotText (" + tmpCounter + ")" ).AddComponent<TextMeshPro>();
			tmp.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			tmp.renderer.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			tmp.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
			tmp.renderer.enabled = false; // Plot renderes text in immediate mode, so we switch off the renderer.

			// TODO: I would like to deactivate the GameObject, but doing so prevents TextMeshPro from doing it's work.
			// This would also get rid of the TextMeshPro gizmos.
			//_tmpText.gameObject.SetActive( false );

			return tmp;
		}
	}
}