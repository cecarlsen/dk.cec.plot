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

		Shader _shader;

		const string antialiasKeyword = "_ANTIALIAS";
		protected const string textureOverlayKeyword = "_TEXTURE_OVERLAY";
		protected const string textureMultiplyKeyword = "_TEXTURE_MULTIPLY";


		protected static class SharedShaderIDs
		{
			public static readonly int _StrokeColor = Shader.PropertyToID( nameof( _StrokeColor ) );
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
			Material material = new Material( shader );
			material.hideFlags = HideFlags.DontSave;
			material.enableInstancing = true;
			material.name = shader.name + " " + num;
			return material;
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
					_material.SetInt( SharedShaderIDs._SrcBlend, (int) BlendMode.SrcAlpha );
					_material.SetInt( SharedShaderIDs._DstBlend, (int) BlendMode.One );
					break;
			}

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

			if( drawNow ) return;

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
	}
}