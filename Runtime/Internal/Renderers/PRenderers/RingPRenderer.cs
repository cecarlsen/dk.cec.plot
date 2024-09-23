/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using UnityEngine.Rendering;

namespace PlotInternals
{
	public class RingPRenderer : FillPRenderer
	{
		public Mesh mesh;

		static class ShaderIDs
		{
			public static readonly int _VertData = Shader.PropertyToID( nameof( _VertData ) );
			public static readonly int _FragData = Shader.PropertyToID( nameof( _FragData ) );
		}


		public RingPRenderer
		(
			bool antialias, Plot.Blend blend, Texture fillTexture, Plot.FillTextureBlend fillTextureBlend
		) : base ( "Ring", antialias, blend, fillTexture, fillTextureBlend )
		{
			CreateMesh();
		}


		public void Render
		(
			float x, float y, float innerDiameter, float outerDiameter,
			bool drawNow, Matrix4x4 matrix, ref Plot.Style style, ref Vector2 pivotPosition // Note that style and pivot are passed by reference for performance reasons, they are not changed.
		){
			bool hasFill = style.hasVisibleFill;
			bool hasStroke = style.hasVisibleStroke;
			
			float boundScale = outerDiameter;
			float meshScale = boundScale;
			float ringExtents = ( outerDiameter - innerDiameter ) * 0.25f;
			float actualStrokeWidth = hasStroke ? style.strokeWidth : 0f;
			float strokeOffsetMin = 0f;
			float innerVertexFactor;
			if( hasStroke ) {
				strokeOffsetMin = GetStokeOffsetMin( ref style );
				float extension = ( strokeOffsetMin + actualStrokeWidth ) * 2f;
				meshScale += extension;
				if( !hasFill && innerDiameter < 0 ) { // For stroked cricle without fill, make a hole. 
					innerVertexFactor = ( meshScale - actualStrokeWidth * 2f ) / meshScale;
				} else {
					innerVertexFactor = ( innerDiameter - extension ) / meshScale;
				}
			} else {
				innerVertexFactor = innerDiameter / meshScale;
			}
			if( innerVertexFactor < 0 ) innerVertexFactor = 0f;
			float ringRadius = innerDiameter * 0.5f + ringExtents;

			if( x != 0f || y != 0f ) matrix.Translate3x4( x, y );
			if( meshScale != 1f ) matrix.Scale3x4( meshScale, meshScale );
			if( style.pivot != Plot.Pivot.Center ){
				float pivotFactor = 0.5f * boundScale / meshScale;
				matrix.Translate3x4( -pivotPosition.x * pivotFactor, -pivotPosition.x * pivotFactor );
			}

			EnsureAvailableMaterialBeforeSubmission( drawNow );

			if( isFillColorDirty || isStrokeColorDirty ) UpdateFillAndStroke( ref style, drawNow );

			if( style.hasVisibleTextureEnabled ) { // Texture is set in EnsureAvailableMaterialBeforeSubmission
				if( drawNow ) {
					_material.SetVector( FillShaderIDs._Tex_ST, style.fillTextureST );
					_material.SetColor( FillShaderIDs._TexTint, style.fillTextureTint );
				} else {
					_propBlock.SetVector( FillShaderIDs._Tex_ST, style.fillTextureST );
					_propBlock.SetColor( FillShaderIDs._TexTint, style.fillTextureTint );
				}
			}

			Vector4 vertData = new Vector4( meshScale, innerVertexFactor );
			Vector4 fragData = new Vector4( ringRadius, ringExtents, actualStrokeWidth, strokeOffsetMin ); 

			if( drawNow ) {
				_material.SetVector( ShaderIDs._VertData, vertData );
				_material.SetVector( ShaderIDs._FragData, fragData );
				_material.SetPass( 0 );
				Graphics.DrawMeshNow( mesh, matrix );
			} else {
				//Debug.Log( _material.enableInstancing + " " + _material.GetHashCode() );
				_propBlock.SetVector( ShaderIDs._VertData, vertData );
				_propBlock.SetVector( ShaderIDs._FragData, fragData );
				Graphics.DrawMesh( mesh, matrix, _material, style.layer, camera: null, submeshIndex: 0, _propBlock, false, false, false );
			}
		}



		void CreateMesh()
		{
			const int cornerCount = 8; // Octagon
			const int vertexCount = cornerCount * 2;
			const int indexCount = cornerCount * 4;

			mesh = new Mesh(){
				name = "Ring",
				hideFlags = HideFlags.HideAndDontSave
			};

			VertexAttributeDescriptor[] vertexDataLayout = new VertexAttributeDescriptor[] {
				new VertexAttributeDescriptor( VertexAttribute.Position, VertexAttributeFormat.Float32, 4 ) // ( outwards.xy, radiusMult, outerFlag )
			};
			SubMeshDescriptor meshDescriptor = new SubMeshDescriptor() {
				vertexCount = vertexCount,
				indexCount = indexCount,
				topology = MeshTopology.Quads,
				bounds = new Bounds( Vector3.zero, new Vector3( 1, 1, 0 ) )
			};
			const MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
			mesh.SetVertexBufferParams( vertexCount, vertexDataLayout );
			mesh.SetIndexBufferParams( indexCount, IndexFormat.UInt16 );
			mesh.subMeshCount = 1;
			mesh.SetSubMesh( 0, meshDescriptor, meshFlags );

			Vector4[] vertexData = new Vector4[ vertexCount ];
			ushort[] indices = new ushort[ indexCount ];
			float angleStep = Mathf.PI * 2 / (float) cornerCount;
			float outerRadius = 0.5f / Mathf.Cos( angleStep * 0.5f );
			const float innerRadius = 0.5f;
			Vector4 outwards_radiusMult_outerFlag = Vector4.zero;
			for( ushort p = 0, v1 = 0, i = 0; p < cornerCount; p++, v1 += 2 )
			{
				ushort v0 = v1 > 0 ? (ushort) ( v1 - 2 ) : (ushort) ( vertexCount - 2 );
				float a = p * angleStep;

				outwards_radiusMult_outerFlag.x = Mathf.Cos( a );
				outwards_radiusMult_outerFlag.y = Mathf.Sin( a );

				outwards_radiusMult_outerFlag.z = innerRadius;
				outwards_radiusMult_outerFlag.w = 0;
				vertexData[ v1 ] = outwards_radiusMult_outerFlag;

				outwards_radiusMult_outerFlag.z = outerRadius;
				outwards_radiusMult_outerFlag.w = 1;
				vertexData[ v1 + 1 ] = outwards_radiusMult_outerFlag;

				indices[ i++ ] = v0;
				indices[ i++ ] = v1;
				indices[ i++ ] = (ushort) ( v1 + 1 );
				indices[ i++ ] = (ushort) ( v0 + 1 );
			}
			mesh.SetVertexBufferData( vertexData, 0, 0, vertexCount, 0, meshFlags );
			mesh.SetIndexBufferData( indices, 0, 0, indexCount, meshFlags );
			mesh.bounds = meshDescriptor.bounds;

			mesh.UploadMeshData( true );
		}
	}
}