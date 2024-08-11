/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;

namespace PlotInternals
{
	public class PolygonPRenderer : FillPRenderer
	{
		static class ShaderIDs
		{
			public static readonly int strokeData = Shader.PropertyToID( "_StrokeData" );
			public static readonly int roundStrokeCornersFlag = Shader.PropertyToID( "_RoundStrokeCornersFlag" );
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
			bool hasFill = style.fillEnabled;
			bool hasStroke = style.strokeEnabled;

			float strokeOffsetMin = GetStokeOffsetMin( ref style ); 
			float actualStrokeWidth = hasStroke ? style.strokeWidth : 0;
			bool roundStrokeCornersFlag = style.strokeCornerProfile == Plot.StrokeCornerProfile.Round;

			Mesh mesh;
			polygon.AdaptAndGetMesh( drawNow, hasFill, hasStroke, style.antialias, out mesh );

			EnsureAvailableMaterialBeforeSubmission( drawNow );

			if( isFillColorDirty || isStrokeColorDirty ) UpdateFillAndStroke( ref style, drawNow );

			if( style.fillTexture ) { // Texture is set in EnsureAvailableMaterialBeforeSubmission
				_material.SetVector( FillShaderIDs.texST, style.fillTextureST );
				_material.SetColor( FillShaderIDs.texTint, style.fillTextureTint );
			}

			Vector4 strokeData = new Vector4( actualStrokeWidth, strokeOffsetMin );

			if( drawNow ) {
				_material.SetVector( ShaderIDs.strokeData, strokeData );
				_material.SetFloat( ShaderIDs.roundStrokeCornersFlag, roundStrokeCornersFlag ? 1 : 0 );
				_material.SetPass( 0 );
				Graphics.DrawMeshNow( mesh, matrix );
			} else {
				_propBlock.SetVector( ShaderIDs.strokeData, strokeData );
				_propBlock.SetFloat( ShaderIDs.roundStrokeCornersFlag, roundStrokeCornersFlag ? 1 : 0 );
				Graphics.DrawMesh( mesh, matrix, _material, style.layer, null, 0, _propBlock, false, false, false );
			}
		}
	}
}