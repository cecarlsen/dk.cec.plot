/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlotInternals
{
	public class PRenderer
	{
		public bool isStrokeColorDirty = true;

		protected Material _material;
		protected MaterialPropertyBlock _propBlock;
		bool _materialCreated = false;

		Queue<(int,Material)> _framedMaterialPool;
		int _materialSubmissionFrame = -1;

		protected bool _areFeaturesDirty;

		bool _isShapeAntialisationOn;
		Plot.Blend _blend;
		//Plot.FeatherMode _featherMode;

		Shader _shader;

		// TODO: replace these with LocalKeyword style integration
		const string antialiasKeyword = "_ANTIALIAS";
		protected const string textureOverlayKeyword = "_TEXTURE_OVERLAY";
		protected const string textureMultiplyKeyword = "_TEXTURE_MULTIPLY";
		protected const string textureReplaceKeyword = "_TEXTURE_REPLACE";
		//const string featherStrokeKeyword = "_FEATHER_STROKE";


		protected static class SharedShaderIDs
		{
			public static readonly int _StrokeColor = Shader.PropertyToID( nameof( _StrokeColor ) );
			public static readonly int _StrokeFeather = Shader.PropertyToID( nameof( _StrokeFeather ) );
			public static readonly int _SrcBlend = Shader.PropertyToID( nameof( _SrcBlend ) );
			public static readonly int _DstBlend = Shader.PropertyToID( nameof( _DstBlend ) );
		}


		public PRenderer
		(
			string shapeName,
			bool isShapeAntialisationOn, Plot.Blend blend
		){
			_shader = Shader.Find( "Hidden/Draw/" + shapeName );
			_propBlock = new MaterialPropertyBlock();

			_isShapeAntialisationOn = isShapeAntialisationOn;
			_blend = blend;

			_areFeaturesDirty = true;
			isStrokeColorDirty = true;
		}


		public void SetShapeAntialiasingFeature( bool isOn )
		{
			if( isOn == _isShapeAntialisationOn ) return;

			_isShapeAntialisationOn = isOn;
			_areFeaturesDirty = true;
		}


		public void SetBlendFeature( Plot.Blend blend )
		{
			if( blend == _blend ) return;

			_blend = blend;
			_areFeaturesDirty = true;
		}


		//public void SetFeatherModeFeature( Plot.FeatherMode featherMode )
		//{
		//	if( _featherMode == featherMode ) return;
//
		//	_featherMode = featherMode;
		//	_areFeaturesDirty = true;
		//}


		protected void UpdateStrokeColor( Color color, bool drawNow )
		{
			if( drawNow ) _material.SetColor( SharedShaderIDs._StrokeColor, color );
			else _propBlock.SetColor( SharedShaderIDs._StrokeColor, color );
			isStrokeColorDirty = false;
		}


		protected float GetStokeOffsetMin( ref Plot.Style style ) // Passed by ref for performance only.
		{
			if( !style.hasVisibleStroke ) return 0;
			switch( style.strokeAlignment ) {
				case Plot.StrokeAlignment.Inside: return -style.strokeWidth;
				case Plot.StrokeAlignment.Edge: return -style.strokeWidth * 0.5f;
			}
			return 0;
		}


		protected static Color ColorWithAlpha( Color color, float alpha )
		{
			return new Color( color.r, color.g, color.b, alpha );
		}


		static Material CreateMaterial( Shader shader, int num )
		{
			return new Material( shader ){
				hideFlags = HideFlags.DontSave,
				enableInstancing = true,
				name = shader.name + " " + num
			};
		}


		protected virtual void ApplyFeatures()
		{
			// Antialiasing.
			if( _isShapeAntialisationOn ) _material.EnableKeyword( antialiasKeyword );
			else _material.DisableKeyword( antialiasKeyword );

			// Blend.
			switch( _blend )
			{
				case Plot.Blend.Transparent:
					_material.SetInt( SharedShaderIDs._SrcBlend, (int) BlendMode.SrcAlpha );
					_material.SetInt( SharedShaderIDs._DstBlend, (int) BlendMode.OneMinusSrcAlpha );
					break;
				case Plot.Blend.TransparentAdditive:
					_material.SetInt( SharedShaderIDs._SrcBlend, (int) BlendMode.One );
					_material.SetInt( SharedShaderIDs._DstBlend, (int) BlendMode.One );
					break;
			}

			// Feather.
			//switch( _featherMode )
			//{
			//	case Plot.FeatherMode.Stroke:
			//		_material.EnableKeyword( featherStrokeKeyword );
			//		break;
			//	case Plot.FeatherMode.All:
			//		_material.DisableKeyword( featherStrokeKeyword );
			//		break;
			//}

			// Reset flag.
			_areFeaturesDirty = false;
		}


		protected void EnsureAvailableMaterialBeforeSubmission( bool drawNow )
		{
			int currentFrame = Time.frameCount;

			// Create material if its missing.
			if( !_materialCreated ) {
				_material = CreateMaterial( _shader, 0 );
				ApplyFeatures();
				_materialSubmissionFrame = currentFrame;
				_materialCreated = true;
				return;
			}

			//if( drawNow ) return; // Why was this here?

			// If a feature changes between draw calls in the same frame, we need keep multiple materials with different features.
			bool materialSubmittedThisFrame = _materialSubmissionFrame == currentFrame;
			bool swapMaterial = _areFeaturesDirty && materialSubmittedThisFrame;
			if( swapMaterial )
			{
				// Add the current material to the pool, so it can be used again in a subsequent update frame.
				if( _framedMaterialPool == null ) _framedMaterialPool = new Queue<(int,Material)>();
				_framedMaterialPool.Enqueue( ( currentFrame, _material ) );

				// Grab the oldest material from the pool. If it was used this frame, then create new.
				var pooledFramedMesh = _framedMaterialPool.Peek();
				if( pooledFramedMesh.Item1 == currentFrame ) _material = CreateMaterial( _shader, _framedMaterialPool.Count );
				else _material = _framedMaterialPool.Dequeue().Item2;	
			}

			// Apply settings.
			if( _areFeaturesDirty ) ApplyFeatures();

			// Update frame stamp.
			_materialSubmissionFrame = currentFrame;
		}


		protected void SetVector( bool drawNow, int id, Vector4 value )
		{
			if( drawNow ) _material.SetVector( id, value );
			else _propBlock.SetVector( id, value );
		}


		protected void SetColor( bool drawNow, int id, Color value )
		{
			if( drawNow ) _material.SetColor( id, value );
			else _propBlock.SetColor( id, value );
		}


		protected void SetFloat( bool drawNow, int id, float value )
		{
			if( drawNow ) _material.SetFloat( id, value );
			else _propBlock.SetFloat( id, value );
		}
	}
}