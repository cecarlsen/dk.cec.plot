/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using UnityEngine.Rendering;

namespace PlotInternals
{
	public class ArcPRenderer : FillPRenderer
	{
		public Mesh mesh;


		public static class ShaderIDs
		{
			public static readonly int _VertData = Shader.PropertyToID( nameof( _VertData ) );
			public static readonly int _AngleOffset = Shader.PropertyToID( nameof( _AngleOffset ) );
			public static readonly int _FragData = Shader.PropertyToID( nameof( _FragData ) );
			public static readonly int _StrokeWidth = Shader.PropertyToID( nameof( _StrokeWidth ) );
			public static readonly int _StrokeAlignmentExtension = Shader.PropertyToID( nameof( _StrokeAlignmentExtension ) );
		}


		public ArcPRenderer
		(
			bool antialias, Plot.Blend blend, Texture fillTexture, Plot.FillTextureBlend fillTextureBlend
		) : base ( "Arc", antialias, blend, fillTexture, fillTextureBlend )
		{
			CreateMesh();
		}


		public void Render
		(
			float x, float y, float innerDiameter, float outerDiameter, float beginAngle, float deltaAngle, float cutOff, float roundness, bool useGeometricRoundness, bool constrainAngleSpanToRoundness,
			bool drawNow, Matrix4x4 matrix, ref Plot.Style style, ref Vector2 pivotPosition // Note that style and pivot are passed by reference for performance reasons, they are not changed.
		){
			if( cutOff < 0 ) cutOff = 0;

			bool hasStroke = style.hasVisibleStroke;
			bool hasRoundness = roundness > 0;

			float endAngle;
			if( deltaAngle > 0f ){
				endAngle = beginAngle + deltaAngle;	
			} else {
				endAngle = beginAngle;
				beginAngle = beginAngle + deltaAngle;
			}
			
			if( endAngle < beginAngle ) endAngle = beginAngle;
			else while( beginAngle > endAngle ) endAngle += 360f;

			float boundScale = outerDiameter;
			float meshScale = boundScale;
			float ringExtents = ( outerDiameter - innerDiameter ) * 0.25f;
			float ringRadius = ( innerDiameter + outerDiameter ) * 0.25f;
			float fillDistOffset = 0;
			float horseshoeExtention = -cutOff;
			float actualStrokeWidth = hasStroke ? style.strokeWidth : 0f;

			float fragAngleExtents = ( endAngle - beginAngle ) * 0.5f;
			if( fragAngleExtents > 180f ) {
				fragAngleExtents = 180f;
				if( style.antialias ) horseshoeExtention += ringRadius * 0.005f; // Hack for closing antialisation gab.
			}

			// Dynamic anglular vertex expansion.
			// When stroke alignment not inside, it makes it expensive/ complicated to expand the verticies. So for now, we don't.
			bool useAngularVertexExpansion = !hasStroke || style.strokeAlignment == Plot.StrokeAlignment.Inside;
			float vertAngleExtents = useAngularVertexExpansion ? fragAngleExtents : 180;
			
			float angleOffset = beginAngle + fragAngleExtents;

			float innerVertexFactor;
			float strokeAlignmentExtension = 0;
			if( hasStroke || hasRoundness ) {
				bool hardStrokeCorners = style.strokeCornerProfile == Plot.StrokeCornerProfile.Hard;
				strokeAlignmentExtension = style.strokeAlignment == Plot.StrokeAlignment.Outside ? actualStrokeWidth : style.strokeAlignment == Plot.StrokeAlignment.Edge ? actualStrokeWidth * 0.5f : 0f;
				float strokeAlignmentReduction = actualStrokeWidth - strokeAlignmentExtension;
				float strokeProfileReduction = hardStrokeCorners ? actualStrokeWidth : 0f;

				if( hasStroke ) {
					meshScale += strokeAlignmentExtension * 2;
					innerVertexFactor = ( innerDiameter - strokeAlignmentExtension * 2 ) / meshScale;
					ringExtents += strokeProfileReduction - strokeAlignmentReduction;
					fillDistOffset += strokeProfileReduction;
					if( useGeometricRoundness ) {
						if( roundness < 1f ) horseshoeExtention += ( strokeProfileReduction - strokeAlignmentReduction ) * ( 1 - roundness );
					} else {
						horseshoeExtention += strokeAlignmentExtension;
						if( !hardStrokeCorners ) horseshoeExtention -= actualStrokeWidth;
					}
				} else {
					innerVertexFactor = innerDiameter / meshScale;
				}
				if( hasRoundness ) {
					float roundnessReduction = roundness * ringExtents;
					if( useGeometricRoundness ) {
						// For geometric roundness we are extending the horseshoe, so we need to compensate by reducing angle extents to keep shape inside angle bounds.
						float innerRadius = innerDiameter * 0.5f;
						float roundedCornerRadius;
						if( hardStrokeCorners ) {
							roundedCornerRadius = ( ringExtents + strokeAlignmentReduction - strokeProfileReduction ) * roundness;
						} else {
							roundedCornerRadius = ( ringExtents + strokeAlignmentReduction - actualStrokeWidth ) * roundness + actualStrokeWidth;
						}
						float roundedCornerCenterDist = innerRadius + roundedCornerRadius;
						float contactRadius = Mathf.Sqrt( roundedCornerCenterDist * roundedCornerCenterDist + roundedCornerRadius * roundedCornerRadius ); // Right angled triangle: a^2 = c^2 - b^2
						contactRadius = innerRadius + roundedCornerRadius + ( roundedCornerCenterDist - contactRadius ) * 0.5f;
						float circumferenceCutoff = roundnessReduction + roundness * ( strokeAlignmentReduction - strokeProfileReduction );
						float geometricRoundnessAngleExtentsReduction = ( circumferenceCutoff / contactRadius ) * Mathf.Rad2Deg; // Circumference: C = 2*PI * r => a = C / r.
						fragAngleExtents -= geometricRoundnessAngleExtentsReduction;
						if( constrainAngleSpanToRoundness ) {
							if( fragAngleExtents < 0 ) {
								angleOffset -= -fragAngleExtents;
								vertAngleExtents += -fragAngleExtents;
								fragAngleExtents = 0;
							}
						}
					} else {
						horseshoeExtention -= roundnessReduction;
						if( constrainAngleSpanToRoundness ) {
							float minAngleExtents = roundness * ( outerDiameter - innerDiameter ) * 0.5f / innerDiameter * Mathf.Rad2Deg; // c = Tau * r.
							if( fragAngleExtents < minAngleExtents ) {
								angleOffset -= minAngleExtents - fragAngleExtents;
								vertAngleExtents = minAngleExtents;
								fragAngleExtents = minAngleExtents;
							}
						}
					}
					ringExtents -= roundnessReduction;
					fillDistOffset -= roundnessReduction;
				}
			} else {
				innerVertexFactor = innerDiameter / meshScale;
			}
			if( innerVertexFactor < 0 ) innerVertexFactor = 0;

			fragAngleExtents *= Mathf.Deg2Rad;
			vertAngleExtents *= Mathf.Deg2Rad;
			angleOffset *= Mathf.Deg2Rad;

			if( x != 0f || y != 0f ) matrix.Translate3x4( x, y );
			if( meshScale != 1f ) matrix.Scale3x4( meshScale, meshScale );
			if( style.pivot != Plot.Pivot.Center ){
				float pivotFactor = 0.5f * boundScale / meshScale;
				matrix.Translate3x4( -pivotPosition.x * pivotFactor, -pivotPosition.x * pivotFactor );
			}

			EnsureAvailableMaterialBeforeSubmission( drawNow );

			// Set constants.
			if( isFillColorDirty || isStrokeColorDirty ) UpdateFillAndStroke( ref style, drawNow );
			if( style.fillTexture ) { // Texture is set in EnsureAvailableMaterialBeforeSubmission
				SetVector( drawNow, FillShaderIDs._TexUVRect, style.fillTextureUVRect );
				SetColor( drawNow, FillShaderIDs._TexTint, style.fillTextureTint );
				SetFloat( drawNow, ShaderIDs._StrokeAlignmentExtension, strokeAlignmentExtension );
			}
			SetVector( drawNow, ShaderIDs._VertData, new Vector4( meshScale, innerVertexFactor, fragAngleExtents, vertAngleExtents ) );
			SetFloat( drawNow, ShaderIDs._AngleOffset, angleOffset );
			SetVector( drawNow, ShaderIDs._FragData, new Vector4( ringRadius, ringExtents, horseshoeExtention, fillDistOffset ) );
			SetFloat( drawNow, ShaderIDs._StrokeWidth, actualStrokeWidth );
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
			const int cornerCount = 6;					// Hexagon. Must match CORNER_COUNT in Arc.shader
			const int pointCount = cornerCount + 1;		// One for wrapping
			const int flapCornerCount = 2;				// Two for extending horseshoe flaps for antialiasing.
			const int vertexCount = ( pointCount + flapCornerCount ) * 2;
			const int indexCount = ( cornerCount + flapCornerCount ) * 4;

			mesh = new Mesh();
			mesh.name = "Ring";
			mesh.hideFlags = HideFlags.HideAndDontSave;

			VertexAttributeDescriptor[] vertexDataLayout = new VertexAttributeDescriptor[] {
				new VertexAttributeDescriptor( VertexAttribute.Position, VertexAttributeFormat.Float32, 3 ), // ( t, outerFlag, flapFlag )
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

			Vector3[] vertexData = new Vector3[ vertexCount ];
			ushort[] indices = new ushort[ indexCount ];
			float step = 1 / (float) cornerCount;
			Vector3 t_outerFlag_flapFlag = Vector3.zero; // t: interpolator from angle begin to end, outerFlag: is vertex on the outer size of the ring, flapFlag: is vertex at angle begin or end vertex.
			for( ushort p = 0, v1 = 2, i = 0; p < pointCount+1; p++, v1 += 2 )
			{
				if( p < pointCount ) {
					t_outerFlag_flapFlag.x = p * step * 2 - 1;
				
					t_outerFlag_flapFlag.y = 0;
					vertexData[ v1 ] = t_outerFlag_flapFlag;

					t_outerFlag_flapFlag.y = 1;
					vertexData[ v1 + 1 ] = t_outerFlag_flapFlag;
				}
				
				ushort v0 = (ushort) ( v1 - 2 );
				indices[ i++ ] = v0;
				indices[ i++ ] = v1;
				indices[ i++ ] = (ushort) ( v1 + 1 );
				indices[ i++ ] = (ushort) ( v0 + 1 );
			}

			// Flaps.
			vertexData[ 0 ] = vertexData[ 2 ];
			vertexData[ 1 ] = vertexData[ 3 ];
			vertexData[ vertexCount - 2 ] = vertexData[ vertexCount - 4 ];
			vertexData[ vertexCount - 1 ] = vertexData[ vertexCount - 3 ];
			vertexData[ 0 ].z = -1;
			vertexData[ 1 ].z = -1;
			vertexData[ vertexCount - 2 ].z = 1;
			vertexData[ vertexCount - 1 ].z = 1;

			mesh.SetVertexBufferData( vertexData, 0, 0, vertexCount, 0, meshFlags );
			mesh.SetIndexBufferData( indices, 0, 0, indexCount, meshFlags );
			mesh.bounds = meshDescriptor.bounds;

			mesh.UploadMeshData( true );
		}
	}
}