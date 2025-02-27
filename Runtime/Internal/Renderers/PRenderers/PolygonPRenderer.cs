﻿/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;

namespace PlotInternals
{
	public class PolygonPRenderer : FillPRenderer
	{
		static class ShaderIDs
		{
			public static readonly int _StrokeData = Shader.PropertyToID( nameof( _StrokeData ) );
			public static readonly int _RoundStrokeCornersFlag = Shader.PropertyToID( nameof( _RoundStrokeCornersFlag ) );
		}

		public PolygonPRenderer
		(
			bool antialias, Plot.Blend blend, Texture fillTexture, Plot.FillTextureBlend fillTextureBlend
		) : base ( "Polygon", antialias, blend, fillTexture, fillTextureBlend )
		{
			
		}


		public void Render
		(
			Plot.Polygon polygon,
			bool drawNow, ref Matrix4x4 matrix, ref Plot.Style style // Note that matrix and style are passed by reference for performance reasons, they are not changed.
		){ 
			bool hasFill = style.hasVisibleFill;
			bool hasStroke = style.hasVisibleStroke;

			float strokeOffsetMin = GetStokeOffsetMin( ref style ); 
			float actualStrokeWidth = hasStroke ? style.strokeWidth : 0;
			bool roundStrokeCornersFlag = style.strokeCornerProfile == Plot.StrokeCornerProfile.Round;

			Mesh mesh = polygon.AdaptAndGetMesh( drawNow, hasFill, hasStroke, style.antialias );

			EnsureAvailableMaterialBeforeSubmission( drawNow );

			// Set constants.
			if( isFillColorDirty || isStrokeColorDirty ) UpdateFillAndStroke( ref style, drawNow );
			//if( style.fillTexture ) { // Texture is set in EnsureAvailableMaterialBeforeSubmission TODO
			//	_material.SetVector( FillShaderIDs._TexUVRect, style.fillTextureUVRect );
			//	_material.SetColor( FillShaderIDs._TexTint, style.fillTextureTint );
			//}
			SetVector( drawNow, ShaderIDs._StrokeData, new Vector4( actualStrokeWidth, strokeOffsetMin, style.strokeFeather ) );
			SetFloat( drawNow, ShaderIDs._RoundStrokeCornersFlag, roundStrokeCornersFlag ? 1 : 0 );

			// Draw.
			if( drawNow ) {
				_material.SetPass( 0 );
				Graphics.DrawMeshNow( mesh, matrix );
			} else {
				Graphics.DrawMesh( mesh, matrix, _material, style.layer, null, 0, _propBlock, false, false, false );
			}
		}
	}
}