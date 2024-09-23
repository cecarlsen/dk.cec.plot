/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Polyline" 
{
	Properties
	{
		// We need to define these properties so that Unity's material remember the settings.
		_SrcBlend( "_SrcBlend", Float ) = 0
		_DstBlend( "_DstBlend", Float ) = 0
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" } 
		ZWrite Off
		Blend [_SrcBlend] [_DstBlend]

		Pass
		{
			CGPROGRAM 

			#pragma vertex Vert
			#pragma fragment Frag

			// Instancing Options.
			//		nolodfade - Prevent Unity from applying GPU Instancing to LOD fade values.
			//		nolightprobe - Prevent Unity from applying GPU Instancing to Light Probe values
			//		nolightmap - Prevent Unity from applying GPU Instancing to Lightmap ST
			#pragma instancing_options nolodfade nolightprobe nolightmap

			#pragma multi_compile_fog 				// Support fog.
			#pragma multi_compile_instancing		// Support instancing
			#pragma multi_compile_local __ _ANTIALIAS

			#define SHAPE_EXTENTS_EXCLUDED_FROM_MODEL_MATRIX	// Used by TransformVertexAndComputeShapePosition

			#include "UnityCG.cginc"
			#include "Base.cginc"
			
			
			struct ToVert
			{
				float2 position : POSITION;						// Unexpanded point position. Different for each point vertex pair.
				float4 outwards_sideFlag_endFlag : TEXCOORD0;	// ( outwards.xy, sideMult, endMult ). Both are normalized. Different for each point vertex pair. For sideMult: -1 and 1. For endMult: -1, 0 or 1.
				float4 points : TEXCOORD1;						// ( pointA.xy, pointB.xy ). Different for each quad segment.
				// TODO: we will beusing pos 1D for dashing in the future.
				float4 caps_pos1D : TEXCOORD2;				// ( caps.xy, posAlongLineA, posAlongLineB ). Different for each quad segment. For caps: 0 is None, 1 is Square, 2 is Round.
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};


			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float3 posSS_shrinkFlag : TEXCOORD0;				// ( pos.xy, shrink ). For shrink, -1 for begin when Cap.None, 0 for no shrinking, +1 for begin when Cap.None.
				nointerpolation float4 points : TEXCOORD1;	
				nointerpolation float4 caps_pos1D : TEXCOORD2;	
				UNITY_FOG_COORDS( 3 ) 						// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 				// Support instanced properties in fragment Shader.
			};


			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( half4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeExtents )
			UNITY_INSTANCING_BUFFER_END( Props )
			

			float SdPolylineSegment( float2 p, float2 a, float2 b, float2 r )
			{
				float2 ap = p - a;
				float2 ab = b - a;
				float abSqL = dot( ab, ab );		// Square length of AB.
				float h = dot( ap, ab );
				if( abSqL > 0 ) h /= abSqL;			// Normalized position along segment.
				float l = sqrt( abSqL );			// Actual position along segment.
				if( h <= 0 && r.x < 2 ){			// Hard cap for pointA
					l *= -h;
				}
				else if( h >= 1 && r.y < 2 ){		// Hard cap for pointB
					l *= h - 1;
				}
				else {								// Round cap for A or B.
					l = 0;
					h = saturate( h );
				}
				return max( length( ap - ab * h ), l );
			}


			ToFrag Vert( ToVert v )
			{
				ToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				// Get properties.
				half strokeExtents = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeExtents );

				// Get scales.
				// Polygon is a special case. When stroke width is zero, then the stroke is still used for antialising.
				// By setting shapeExtentsWithoutCanvasScaling to one here, we are effectively diabling min size maintenance while preserving antialiasing.
				float2 shapeScaleWithCanvasScale, modelScale;
				GetScales( strokeExtents, shapeScaleWithCanvasScale, modelScale );

				// Create vertex vertex. For polyline, the vertex is the non-expanded A or B point of a line segment. So we need to expand it here...
				// Compute end and side directions. outwardsSideEnd.z indicates lef-right side of the line. outwardsSideEnd.w indicates end direction if point is end point.
				o.vertex = float4( v.position, 0, 1 );
				float2 endDirection = float2( -v.outwards_sideFlag_endFlag.y, v.outwards_sideFlag_endFlag.x ) * v.outwards_sideFlag_endFlag.w; // Flip the outwards and we have the forward, then mult by the end flag.
				float2 sideDirection = v.outwards_sideFlag_endFlag.xy * v.outwards_sideFlag_endFlag.z; // Flip outwards depending on side flag.
				float2 expansionDirection = endDirection + sideDirection;
				o.vertex.xy += sideDirection * strokeExtents;

				// Expand caps.
				float endExtMult = 0;
				if( v.outwards_sideFlag_endFlag.w < 0 ) endExtMult = v.caps_pos1D.x;
				else if( v.outwards_sideFlag_endFlag.w > 0 ) endExtMult = v.caps_pos1D.y;
				endExtMult = saturate( endExtMult );
				o.vertex.xy += endDirection * strokeExtents * endExtMult; // Expand along end direction when cap is Round or Square.

				// Copy to shape position.
				o.posSS_shrinkFlag.xy = o.vertex.xy;

				// Compute world space pizel size at transformed position as seen by camera.
				float pixelSizeWS = GetWorldSpacePixelSizeRelativeToCamera( o.vertex );

				// Compute min size preserving expansion.
				float2 expansion = expansionDirection * GetMinSizePreservingExpansion( pixelSizeWS, shapeScaleWithCanvasScale );

				// Add antialiasing preserving expansion.
				#ifdef _ANTIALIAS
					expansion += expansionDirection * pixelSizeWS * ANTIALIAS_EXTENTS;
				#endif

				// Transform vertex and shape position.
				TransformVertexAndComputeShapePosition( expansion, strokeExtents, shapeScaleWithCanvasScale, modelScale, o.vertex, o.posSS_shrinkFlag.xy );

				// Transfer data.
				o.points = v.points;
				o.caps_pos1D = v.caps_pos1D;

				// Set shrink flag.
				o.posSS_shrinkFlag.z = v.outwards_sideFlag_endFlag.w * ( 1 - endExtMult );

				// Support fog.
				UNITY_TRANSFER_FOG( o, o.vertex );

				return o;
			}
			

			fixed4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.

				// Read properties.
				half strokeExtents = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeExtents );
				fixed4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				// Compute fragment size in shape space. We presume uniform shape space, so we only have to measure one dimension.
				float fSizeSS = fwidth( i.posSS_shrinkFlag.y );

				// Shrink by translating position when Cap.None. We take advantage of interpolation from A to B.
				float shrinkMult = ceil( abs( i.posSS_shrinkFlag.z ) - 0.5 ) * sign( i.posSS_shrinkFlag.z );
				i.posSS_shrinkFlag.xy += ( i.points.zw - i.points.xy ) / ( i.caps_pos1D.w - i.caps_pos1D.z ) * shrinkMult * strokeExtents;
				//return fixed4( abs( shrinkMult ).xxx, 1 ); // DEBUG visualize what area s translated.

				// Ensure that stroke extents stays above safe size and compute alpha to fade out below that safe size.
				float thiningAlphaMult;
				MaintainSafeStrokeExtents( fSizeSS, strokeExtents, thiningAlphaMult );
				strokeCol.a *= thiningAlphaMult;

				// Compute signed distance to shape.
				float d = SdPolylineSegment( i.posSS_shrinkFlag.xy, i.points.xy, i.points.zw, i.caps_pos1D.xy ) - strokeExtents;

				// Evaluate color.
				return EvaluateStrokeColor( d, fSizeSS, strokeCol );
			}
			ENDCG
		}
	}
}