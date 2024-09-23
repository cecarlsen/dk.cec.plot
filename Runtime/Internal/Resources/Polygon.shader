/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Polygon"
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
				float2 position : POSITION;
				float4 points : TEXCOORD0;
				float3 outwards : TEXCOORD1;				// ( outwards.xy, directionMult )
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float2 posSS : TEXCOORD0;					// Shape position.
				nointerpolation float4 points : TEXCOORD1;
				UNITY_FOG_COORDS( 2 ) 						// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 				// Support instanced properties in fragment Shader.
			};

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( half4, _FillColor )
				UNITY_DEFINE_INSTANCED_PROP( half4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half2, _StrokeData ) // x: strokeWidth, y: strokeOffsetMin
				UNITY_DEFINE_INSTANCED_PROP( half, _RoundStrokeCornersFlag )
			UNITY_INSTANCING_BUFFER_END( Props )
			

			// From IQ: https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
			float SdPolygonLineSegment( float2 p, float2 a, float2 b, bool useRoundedStrokeCorners )
			{
				float2 ap = p - a;
				float2 ab = b - a;
				float h = dot( ap, ab ) / dot( ab, ab );	// Compute normalized position along the segment. Dot( ab, ab ) is square length.

				// On the right side, make it hard and negative.
				if( dot( ap, float2( -ab.y, ab.x ) ) < 0 ) return -distance( a + ab * h, p );

				if( useRoundedStrokeCorners ) h = saturate( h );
				return length( ap - ab * h );
			}


			ToFrag Vert( ToVert v )
			{
				ToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );				// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );			// Support instanced properties in fragment Shader.

				// Get properties.
				half2 strokeData = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeData );
				half strokeExtents = strokeData.x * 0.5;

				// Get scales.
				// Polygon is a special case. When stroke width is zero, then the stroke is still used for antialising.
				// By setting shapeExtentsWithoutCanvasScaling to one here, we are effectively diabling min size maintenance while preserving antialiasing.
				const float shapeExtentsWithoutCanvasScaling = 1;
				float2 shapeScaleWithCanvasScale, modelScale;
				GetScales( shapeExtentsWithoutCanvasScaling, shapeScaleWithCanvasScale, modelScale );

				// Create vertex. For polygon, the vertex is the non-expanded A or B point of a line segment. So we need to expand it here...
				o.vertex = float4( v.position, 0, 1 );
				float2 expansionDirection = v.outwards.xy * v.outwards.z;
				o.vertex.xy += expansionDirection * strokeExtents + v.outwards.xy * ( strokeExtents + strokeData.y );

				// Copy to shape position.
				o.posSS = o.vertex.xy;

				// Compute world space pizel size at transformed position as seen by camera.
				float pixelSizeWS = GetWorldSpacePixelSizeRelativeToCamera( o.vertex );

				// Compute min size preserving expansion.
				float2 expansion = v.outwards * GetMinSizePreservingExpansion( pixelSizeWS, shapeScaleWithCanvasScale );

				// Add antialiasing preserving expansion.
				#ifdef _ANTIALIAS
					expansion += expansionDirection * pixelSizeWS * ANTIALIAS_EXTENTS;
				#endif

				// Transform vertex and shape position.
				TransformVertexAndComputeShapePosition( expansion, shapeExtentsWithoutCanvasScaling, shapeScaleWithCanvasScale, modelScale, o.vertex, o.posSS );

				// Transfer points.
				o.points = v.points;

				// Support fog.
				UNITY_TRANSFER_FOG( o, o.vertex );

				return o;
			}

			
			fixed4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.
				
				// Handle color inside polygon.
				fixed4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				if( !any( i.points ) ) return fillCol;

				// Read properties.
				half2 strokeData = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeData );
				bool useRoundStrokeCorners = UNITY_ACCESS_INSTANCED_PROP( Props, _RoundStrokeCornersFlag ) > 0;
				fixed4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				// Compute signed distance to shape.
				float d = SdPolygonLineSegment( i.posSS, i.points.xy, i.points.zw, useRoundStrokeCorners ) - strokeData.y;

				// Compute fragment size in shape space. We presume uniform shape space, so we only have to measure one dimension.
				float fSizeSS = fwidth( i.posSS.y );

				float totalExtents = strokeData.x;
				#ifdef _ANTIALIAS
					totalExtents += ANTIALIAS_EXTENTS;
				#endif

				return EvaluateFillStrokeColor( d, fSizeSS, totalExtents, strokeData.x, fillCol, strokeCol );
			}
			ENDCG
		}
	}
}