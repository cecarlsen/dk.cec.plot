﻿/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using UnityEngine.Rendering;

namespace PlotInternals
{
	public class RectPRenderer : FillPRenderer
	{
		public Mesh mesh;

		static class ShaderIDs
		{
			public static readonly int _VertData = Shader.PropertyToID( nameof( _VertData ) );
			public static readonly int _FragData = Shader.PropertyToID( nameof( _FragData ) );
			public static readonly int _Roundedness = Shader.PropertyToID( nameof( _Roundedness ) );
		}

		public RectPRenderer
		(
			bool antialias, Plot.Blend blend, Texture fillTexture, Plot.FillTextureBlend fillTextureBlend
		) : base ( "Rect", antialias, blend, fillTexture, fillTextureBlend )
		{
			CreateMesh();
		}


		public void Render
		(
			float x, float y, float width, float height, float lowerLeftRoundness, float upperLeftRoundness, float upperRightRoundness, float lowerRightRoundness,
			bool drawNow, Matrix4x4 matrix, ref Plot.Style style, ref Vector2 pivotPosition // Note that style and pivot are passed by reference for performance reasons, they are not changed.
		){
			bool hasStroke = style.hasVisibleStroke;
			bool hasFill = style.hasVisibleFill;
			bool hasHardStrokeProfile = style.strokeCornerProfile == Plot.StrokeCornerProfile.Hard;

			float boundsExtentsX = width * 0.5f; // Extents to edge, where the pivot will align.
			float boundsExtentsY = height * 0.5f;
			float fillExtentsX = boundsExtentsX;
			float fillExtentsY = boundsExtentsY;
			float strokeOffsetMin = 0f;
			float actualStrokeWidth = hasStroke ? style.strokeWidth : 0f;
			float meshExtentsX = fillExtentsX;
			float meshExtentsY = fillExtentsY;
			if( hasStroke ) {
				float strokeOffsetMax = strokeOffsetMin + actualStrokeWidth;
				meshExtentsX += strokeOffsetMax;
				meshExtentsY += strokeOffsetMax;
				strokeOffsetMin = GetStokeOffsetMin( ref style );
				meshExtentsX += strokeOffsetMin;
				meshExtentsY += strokeOffsetMin;
				if( hasHardStrokeProfile ) {
					fillExtentsX += actualStrokeWidth;
					fillExtentsY += actualStrokeWidth;
					strokeOffsetMin -= actualStrokeWidth;
				}
			}
			float innerVertexFactorX = 0, innerVertexFactorY = 0;
			if( lowerLeftRoundness > 0 || upperLeftRoundness > 0 || upperRightRoundness > 0 || lowerRightRoundness > 0 ) {
				if( upperLeftRoundness < 0 ) upperLeftRoundness = 0;
				else if( upperLeftRoundness > 1 ) upperLeftRoundness = 1;
				if( upperRightRoundness < 0 ) upperRightRoundness = 0;
				else if( upperRightRoundness > 1 ) upperRightRoundness = 1;
				if( lowerRightRoundness < 0 ) lowerRightRoundness = 0;
				else if( lowerRightRoundness > 1 ) lowerRightRoundness = 1;
				if( lowerLeftRoundness < 0 ) lowerLeftRoundness = 0;
				else if( lowerLeftRoundness > 1 ) lowerLeftRoundness = 1;
				float fillExtentsMin = fillExtentsX < fillExtentsY ? fillExtentsX : fillExtentsY;
				if( hasHardStrokeProfile ) { // For hard corners, start roundness at - GetStokeOffsetMin().
					float min = -( strokeOffsetMin + actualStrokeWidth );
					float range = fillExtentsMin - min;
					lowerLeftRoundness = min + lowerLeftRoundness * range;
					upperLeftRoundness = min + upperLeftRoundness * range;
					upperRightRoundness = min + upperRightRoundness * range;
					lowerRightRoundness = min + lowerRightRoundness * range;
				} else {
					lowerLeftRoundness *= fillExtentsMin;
					upperLeftRoundness *= fillExtentsMin;
					upperRightRoundness *= fillExtentsMin;
					lowerRightRoundness *= fillExtentsMin;
				}
				if( !hasFill ) { // For rects without fill, make a hole.
					const float quaterPIFactor = 0.2928932f; // 1 - cos( PI* 0.25 )
					innerVertexFactorX = lowerLeftRoundness > upperLeftRoundness ? lowerLeftRoundness : upperLeftRoundness;
					if( upperRightRoundness > innerVertexFactorX ) innerVertexFactorX = upperRightRoundness;
					if( lowerRightRoundness > innerVertexFactorX ) innerVertexFactorX = lowerRightRoundness;
					innerVertexFactorX = -actualStrokeWidth - ( innerVertexFactorX + strokeOffsetMin ) * quaterPIFactor;
					if( innerVertexFactorX > -actualStrokeWidth ) innerVertexFactorX = -actualStrokeWidth;
					innerVertexFactorY = ( meshExtentsY + innerVertexFactorX ) / meshExtentsY;
					innerVertexFactorX = ( meshExtentsX + innerVertexFactorX ) / meshExtentsX;
				}
			} else {
				if( !hasFill ) { // For rects without fill, make a hole.
					innerVertexFactorY = ( meshExtentsY - actualStrokeWidth ) / meshExtentsY;
					innerVertexFactorX = ( meshExtentsX - actualStrokeWidth ) / meshExtentsX;
				}
			}
			if( x != 0f || y != 0f ) matrix.Translate3x4( x, y );
			if( meshExtentsX != 1f || meshExtentsY != 1f ) matrix.Scale3x4( meshExtentsX, meshExtentsY );
			if( style.pivot != Plot.Pivot.Center ) matrix.Translate3x4( -pivotPosition.x * boundsExtentsX / meshExtentsX, -pivotPosition.y  * boundsExtentsY / meshExtentsY );

			EnsureAvailableMaterialBeforeSubmission( drawNow );

			//Debug.Log( "vertData.xy: " + meshExtentsX + ", " + meshExtentsY + ". fragData.xy: " + fillExtentsX + "," + fillExtentsY );

			// Set constants.
			if( isFillColorDirty || isStrokeColorDirty ) UpdateFillAndStroke( ref style, drawNow );
			if( style.fillTexture ) { // Texture is set in EnsureAvailableMaterialBeforeSubmission
				SetVector( drawNow, FillShaderIDs._TexUVRect, style.fillTextureUVRect );
				SetColor( drawNow, FillShaderIDs._TexTint, style.fillTextureTint );
			}
			SetVector( drawNow, ShaderIDs._VertData, new Vector4( meshExtentsX, meshExtentsY, innerVertexFactorX, innerVertexFactorY ) );
			SetVector( drawNow, ShaderIDs._FragData, new Vector4( fillExtentsX, fillExtentsY, actualStrokeWidth, strokeOffsetMin ) );
			SetVector( drawNow, ShaderIDs._Roundedness, new Vector4( lowerLeftRoundness, upperLeftRoundness, upperRightRoundness, lowerRightRoundness ) );
			SetFloat( drawNow, SharedShaderIDs._StrokeFeather, style.strokeFeather );
			
			// Draw.
			if( drawNow ) {
				_material.SetPass( 0 );
				Graphics.DrawMeshNow( mesh, matrix );
			} else {
				Graphics.DrawMesh( mesh, matrix, _material, style.layer, null, 0, _propBlock, false, false, false );
			}
		}


		void CreateMesh()
		{
			const int vertexCount = 4*2;
			const int indexCount = 4*4;

			mesh = new Mesh();
			mesh.name = "Rect";
			mesh.hideFlags = HideFlags.HideAndDontSave;

			// Vertices.
			VertexAttributeDescriptor[] vertexDataLayout = new VertexAttributeDescriptor[] {
				new VertexAttributeDescriptor( VertexAttribute.Position, VertexAttributeFormat.Float32, 3 )
			};
			SubMeshDescriptor meshDescriptor = new SubMeshDescriptor() {
				vertexCount = vertexCount,
				indexCount = indexCount,
				topology = MeshTopology.Quads,
				bounds = new Bounds( Vector3.zero, Vector3.one * 2 )
			};
			const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
			mesh.SetVertexBufferParams( vertexCount, vertexDataLayout );
			mesh.SetIndexBufferParams( indexCount, IndexFormat.UInt16 );
			mesh.subMeshCount = 1;
			mesh.SetSubMesh( 0, meshDescriptor, meshFlags );

			mesh.SetVertexBufferData(
				new Vector3[]{
					new Vector3( -1, -1, 1 ), // x == 1 indicating an outer vertex.
					new Vector3( -1,  1, 1 ),
					new Vector3(  1,  1, 1 ),
					new Vector3(  1, -1, 1  ),
					new Vector3( -1, -1, 0 ),
					new Vector3( -1,  1, 0 ),
					new Vector3(  1,  1, 0 ),
					new Vector3(  1, -1, 0  ),
				},
				0, 0, vertexCount, 0,
				meshFlags
			);
			mesh.SetIndexBufferData(
				new ushort[] {
					0, 1, 5, 4,
					1, 2, 6, 5,
					2, 3, 7, 6,
					3, 0, 4, 7
				}, 
				0, 0, indexCount, meshFlags
			);
			mesh.bounds = meshDescriptor.bounds;

			mesh.UploadMeshData( true );
		}


		/*
		void CreateMesh()
		{
			const int vertexCount = 4;
			const int indexCount = 4;

			mesh = new Mesh();
			mesh.name = "Rect";
			mesh.hideFlags = HideFlags.HideAndDontSave;

			// Vertices.
			VertexAttributeDescriptor[] vertexDataLayout = new VertexAttributeDescriptor[] {
				new VertexAttributeDescriptor( VertexAttribute.Position, VertexAttributeFormat.Float32, 2 )
			};
			SubMeshDescriptor meshDescriptor = new SubMeshDescriptor() {
				vertexCount = vertexCount,
				indexCount = indexCount,
				topology = MeshTopology.Quads,
				bounds = new Bounds( Vector3.zero, Vector3.one * 2 )
			};
			const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
			mesh.SetVertexBufferParams( vertexCount, vertexDataLayout );
			mesh.SetIndexBufferParams( indexCount, IndexFormat.UInt16 );
			mesh.subMeshCount = 1;
			mesh.SetSubMesh( 0, meshDescriptor, meshFlags );

			mesh.SetVertexBufferData(
				new Vector2[]{
					new Vector2( -1, -1 ),
					new Vector2( -1,  1 ),
					new Vector2(  1,  1 ),
					new Vector2(  1, -1 ),
				},
				0, 0, vertexCount, 0,
				meshFlags
			);
			mesh.SetIndexBufferData( new ushort[]{ 0, 1, 2, 3 }, 0, 0, indexCount, meshFlags );
			mesh.bounds = meshDescriptor.bounds;

			mesh.UploadMeshData( true );
		}
		*/
	}
}