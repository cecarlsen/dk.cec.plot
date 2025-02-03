/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

Shader "Hidden/Draw/Line"
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

			#include "UnityCG.cginc"
			#include "Base.cginc"

			struct ToVert
			{
				float2 outwards : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
	 
		
			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float2 posSS : TEXCOORD0;				// Shape space position.
				UNITY_FOG_COORDS( 1 ) 					// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 			// Support instanced properties in fragment Shader.
			};


			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( float4, _Data ) // x: meshExtentsX, y: meshExtentsY, z: roundedBeginCapFlag, w: rounedEndCapFlag,
				UNITY_DEFINE_INSTANCED_PROP( float4, _StrokeColor )
				//UNITY_DEFINE_INSTANCED_PROP( float4, _StrokeEndColor )
			UNITY_INSTANCING_BUFFER_END( Props )


			// Similar to sdRoundedBox. https://iquilezles.org/www/articles/distfunctions/distfunctions.htm
			float SdCenteredHorizontalLineSegment( float2 p, float2 extents, float2 roundCapFlags )
			{
				float r = ( p.x < 0.0 ? roundCapFlags.x : roundCapFlags.y ) * extents.y;
				float2 q = abs( p ) - extents + r;
				return min( max( q.x, q.y ), 0.0 ) + length( max( q, 0.0 ) ) - r;
			}
			


			ToFrag Vert( ToVert v )
			{
				ToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.
				
				// Read properties.
				float2 shapeExtentsWithoutCanvasScaling = UNITY_ACCESS_INSTANCED_PROP( Props, _Data ).xy;

				// Get scales.
				float2 shapeScaleWithCanvasScale, modelScale;
				GetScales( shapeExtentsWithoutCanvasScaling, shapeScaleWithCanvasScale, modelScale );

				// Copy vertex.
				o.vertex = float4( v.outwards, 0, 1 );

				// Compute shape position.
				o.posSS = v.outwards * shapeExtentsWithoutCanvasScaling;

				// Compute world space pizel size at transformed position as seen by camera.
				float pixelSizeWS = GetWorldSpacePixelSizeRelativeToCamera( o.vertex );

				// Compute min size preserving expansion.
				float2 expansion = v.outwards * GetMinSizePreservingExpansion( pixelSizeWS, shapeScaleWithCanvasScale );

				// Add antialiasing preserving expansion.
				#ifdef _ANTIALIAS
					expansion += v.outwards * pixelSizeWS * ANTIALIAS_EXTENTS;
				#endif

				// Transform vertex and shape position.
				TransformVertexAndComputeShapePosition( expansion, shapeExtentsWithoutCanvasScaling, shapeScaleWithCanvasScale, modelScale, o.vertex, o.posSS );

				// Support fog.
				UNITY_TRANSFER_FOG( o, o.vertex );

				return o;
			}



			float4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.
				
				// Read properties.
				float4 data = UNITY_ACCESS_INSTANCED_PROP( Props, _Data );
				float4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );
				//float4 strokeEndCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeEndColor );

				// Interpolate end color. (WORK IN PROGRESS)
				//float startEndT = saturate( ( i.posSS.x / data.x ) * 0.5 + 0.5 );
				//strokeCol = lerp( strokeCol, strokeEndCol, startEndT );

				// Compute fragment size in shape space. We presume uniform shape space, so we only have to measure one dimension.
				float fSizeShape = fwidth( i.posSS.y );

				// Ensure that extents stays above safe size and compute alpha to fade out below that safe size.
				float thiningAlphaMult;
				MaintainSafeStrokeExtents( fSizeShape, data.y, thiningAlphaMult );
				strokeCol.a *= thiningAlphaMult;

				// Compute signed distance to shape.
				float d = SdCenteredHorizontalLineSegment( i.posSS, data.xy, data.zw );

				// Evaluate color.
				half4 col = EvaluateStrokeColor( d, fSizeShape, strokeCol );

				// Support fog.
				UNITY_APPLY_FOG( i.fogCoord, col );

				// Done.
				return col;
			}

			ENDCG
		}
	}
}