/*
	Copyright © Carl Emil Carlsen 2020-2021
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
			public static readonly int fillColor = Shader.PropertyToID( "_FillColor" );
			public static readonly int tex = Shader.PropertyToID( "_Tex" );
			public static readonly int texST = Shader.PropertyToID( "_Tex_ST" );
			public static readonly int texTint = Shader.PropertyToID( "_TexTint" );
		}


		public FillPRenderer( string name,  bool antialias, Plot.Blend blend, Texture fillTexture, Plot.FillTextureBlend fillTextureBlend ) : base( name, antialias, blend )
		{
			_fillTexture = fillTexture;
			_fillTextureBlend = fillTextureBlend;

			isFillColorDirty = true;
		}

		protected void UpdateFillAndStroke( ref Plot.Style style, bool drawNow )
		{
			if( isFillColorDirty || ( !style.fillEnabled && isStrokeColorDirty ) ) {
				Color color = style.fillEnabled ? style.fillColor : ColorWithAlpha( style.strokeColor, 0 );
				if( drawNow ) _material.SetColor( FillShaderIDs.fillColor, color );
				else _propBlock.SetColor( FillShaderIDs.fillColor, color );
			}
			if( isStrokeColorDirty || ( !style.strokeEnabled && isFillColorDirty )) {
				Color color = style.strokeEnabled ? style.strokeColor : ColorWithAlpha( style.fillColor, 0 );
				if( drawNow ) _material.SetColor( SharedShaderIDs.strokeColor, color );
				else _propBlock.SetColor( SharedShaderIDs.strokeColor, color );
			}
			isFillColorDirty = false;
			isStrokeColorDirty = false;
		}

		/*
		protected void UpdateFillColor( Color color, bool drawNow )
		{
			if( drawNow ) _material.SetColor( FillShaderIDs.fillColor, color );
			else _propBlock.SetColor( FillShaderIDs.fillColor, color );
			isFillColorDirty = false;
		}
		*/


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
				_material.SetTexture( FillShaderIDs.tex, _fillTexture );
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