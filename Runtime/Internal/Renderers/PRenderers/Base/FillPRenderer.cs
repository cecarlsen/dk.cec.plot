﻿/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;

namespace PlotInternals
{
	public class FillPRenderer : PRenderer
	{
		public bool isFillColorDirty = true;

		Texture _fillTexture;
		Plot.FillTextureBlend _fillTextureBlend;


		protected static class FillShaderIDs
		{
			public static readonly int _FillColor = Shader.PropertyToID( nameof( _FillColor ) );
			public static readonly int _Tex = Shader.PropertyToID( nameof( _Tex ) );
			public static readonly int _Tex_ST = Shader.PropertyToID( nameof( _Tex_ST ) );
			public static readonly int _TexTint = Shader.PropertyToID( nameof( _TexTint ) );
		}


		public FillPRenderer( string name, bool antialias, Plot.Blend blend, Texture fillTexture, Plot.FillTextureBlend fillTextureBlend ) : base( name, antialias, blend )
		{
			_fillTexture = fillTexture;
			_fillTextureBlend = fillTextureBlend;

			isFillColorDirty = true;
		}

		protected void UpdateFillAndStroke( ref Plot.Style style, bool drawNow )
		{
			if( isFillColorDirty || ( !style.hasVisibleFill && isStrokeColorDirty ) ) {
				Color color = style.hasVisibleFill ? style.fillColor : ColorWithAlpha( style.strokeColor, 0 );
				if( drawNow ) _material.SetColor( FillShaderIDs._FillColor, color );
				else _propBlock.SetColor( FillShaderIDs._FillColor, color );
			}
			if( isStrokeColorDirty || ( !style.hasVisibleStroke && isFillColorDirty )) {
				Color color = style.hasVisibleStroke ? style.strokeColor : ColorWithAlpha( style.fillColor, 0 );
				if( drawNow ) _material.SetColor( SharedShaderIDs._StrokeColor, color );
				else _propBlock.SetColor( SharedShaderIDs._StrokeColor, color );
			}
			isFillColorDirty = false;
			isStrokeColorDirty = false;
		}


		public void SetFillTextureFeature( Texture texture )
		{
			if( _fillTexture == texture ) return;

			_fillTexture = texture;
			_areFeaturesDirty = true;
		}


		public void SetFillTextureBlendFeature( Plot.FillTextureBlend blend )
		{
			if( _fillTextureBlend == blend ) return;

			_fillTextureBlend = blend;
			_areFeaturesDirty = true;
		}


		protected override void ApplyFeatures()
		{
			// Texture.
			if( _fillTexture ) {
				_material.SetTexture( FillShaderIDs._Tex, _fillTexture );
				if( _fillTextureBlend == Plot.FillTextureBlend.Overlay ) {
					_material.EnableKeyword( textureOverlayKeyword );
					_material.DisableKeyword( textureMultiplyKeyword );
				} else {
					_material.EnableKeyword( textureMultiplyKeyword );
					_material.DisableKeyword( textureOverlayKeyword );
				}
			} else {
				_material.DisableKeyword( textureOverlayKeyword );
				_material.DisableKeyword( textureMultiplyKeyword );
			}

			// Call base.
			base.ApplyFeatures();
		}
	}
}