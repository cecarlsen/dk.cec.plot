/*
	Copyright © Carl Emil Carlsen 2021-2024
	http://cec.dk
*/

using UnityEngine;
using TMPro;
using System;

public partial class Plot
{
	[Serializable] 
	public class Text : ScriptableObject
	{
		internal TextMeshPro _tmpText;
		
		// TODO: This belongs in Style. Move it.
		//public TMP_FontAsset font {
		//	get { return _tmpText?.font; }
		//	set { if( _tmpText ) _tmpText.font = value; }
		//}

		// Make constructor private to force use of CreateText().
		Text(){}


		void OnEnable()
		{
			if( !_tmpText )
			{
				_tmpText = new GameObject( "PlotText" ).AddComponent<TextMeshPro>();
				_tmpText.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
				_tmpText.renderer.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
				_tmpText.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
				_tmpText.renderer.enabled = false; // Plot renderes text in immediate mode, so we switch off the renderer.
			}
			
			// TODO: I would like to deactivate the GameObject, but doing so prevents TextMeshPro from doing it's work.
			// This would also get rid of the TextMeshPro gizmos.
			//_tmpText.gameObject.SetActive( false );
		}


		void OnDisable()
		{
			// "Objects created as hide and don't save must be explicitly destroyed by the owner of the object."
			// https://docs.unity3d.com/ScriptReference/HideFlags.html
			if( _tmpText ) DestroyImmediate( _tmpText.gameObject );
		}

		public void SetContent( string text )
		{
			if( _tmpText ) _tmpText.text = text;
		}
	}
}