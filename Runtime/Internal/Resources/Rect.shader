/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk 

	Instancing
	https://docs.huihoo.com/unity/5.6/Documentation/Manual/GPUInstancing.html
*/

Shader "Hidden/Draw/Rect"
{
	Properties
	{
		// We need to define these properties so that Unity's material remember the settings.
		_SrcBlend( "_SrcBlend", Float ) = 0
		_DstBlend( "_DstBlend", Float ) = 0
		_Tex( "Texture", 2D ) = "white" {}
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
			#pragma multi_compile_local __ _TEXTURE_OVERLAY _TEXTURE_MULTIPLY

			#if defined( _TEXTURE_OVERLAY ) || defined( _TEXTURE_MULTIPLY )
				#define _HAS_TEXTURE
			#endif

			#include "UnityCG.cginc"
			#include "Base.cginc"

			 
			struct ToVert
			{
				float3 outwards_outerFlag : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float2 posSS : TEXCOORD0;				// Shape position
				UNITY_FOG_COORDS( 1 ) 					// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 			// Support instanced properties in fragment Shader.
				#ifdef _HAS_TEXTURE
					float2 uv : TEXCOORD2;
				#endif
			};

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( half4, _VertData )			// ( xy: mesh extents, zw: inner vertex factor )
				UNITY_DEFINE_INSTANCED_PROP( half4, _FillColor )
				UNITY_DEFINE_INSTANCED_PROP( half4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half4, _FragData )			// ( xy: fillExtents, z: strokeWidth, w: strokeOffsetMin )
				UNITY_DEFINE_INSTANCED_PROP( half4, _Roundedness )
				#ifdef _HAS_TEXTURE
					UNITY_DEFINE_INSTANCED_PROP( half4, _TexUVRect )
					UNITY_DEFINE_INSTANCED_PROP( half4, _TexTint )
				#endif
			UNITY_INSTANCING_BUFFER_END( Props )


			// From IQ: https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
			float SDRoundedBox( float2 p, float2 b, float4 r )
			{
				r.xy = ( p.x < 0.0 ) ? r.xy : r.wz;
				r.x = ( p.y < 0.0 ) ? r.x : r.y;
				float2 q = abs( p ) - b + r.x;
				return min( max( q.x, q.y ), 0.0 ) + length( max( q, 0.0 ) ) - r.x;
			}

			
			ToFrag Vert( ToVert v )
			{
				ToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				// Read properties.
				half4 data = UNITY_ACCESS_INSTANCED_PROP( Props, _VertData );

				// Get scales.
				float2 shapeScaleWithCanvasScale, modelScale;
				GetScales( data.xy, shapeScaleWithCanvasScale, modelScale );

				// Create vertex.
				float2 outwardsRect = v.outwards_outerFlag.xy;
				o.vertex = float4( outwardsRect, 0, 1 );
				if( v.outwards_outerFlag.z < 1 ){
					o.vertex.xy *= data.zw;
					outwardsRect *= -1;
				}

				// Compute shape position.
				o.posSS = o.vertex.xy * data.xy;

				// Compute uv.
				#ifdef _HAS_TEXTURE
					half4 texST = UNITY_ACCESS_INSTANCED_PROP( Props, _TexUVRect );
					o.uv = ( o.vertex.xy * 0.5 + 0.5 ) * texST.zw + texST.xy;
				#endif

				// Compute world space pizel size at transformed position as seen by camera.
				float pixelSizeWS = GetWorldSpacePixelSizeRelativeToCamera( o.vertex );

				// Compute min size preserving expansion.
				float2 expansion = v.outwards_outerFlag.xy * GetMinSizePreservingExpansion( pixelSizeWS, shapeScaleWithCanvasScale );

				// Add antialiasing preserving expansion.
				#ifdef _ANTIALIAS
					float2 antialiasExpansion = pixelSizeWS * ANTIALIAS_EXTENTS;
					if( v.outwards_outerFlag.z < 1 ) antialiasExpansion = min( antialiasExpansion, data.zw * 0.5 );
					expansion += outwardsRect * antialiasExpansion; // Constrain so that we don't expand beyond the center.
				#endif

				// Transform vertex and shape position.
				TransformVertexAndComputeShapePosition( expansion, data.xy, shapeScaleWithCanvasScale, modelScale, o.vertex, o.posSS );

				UNITY_TRANSFER_FOG( o, o.vertex ); // Support fog.

				return o;
			}
			
			
			half4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.

				half4 data = UNITY_ACCESS_INSTANCED_PROP( Props, _FragData ); // ( xy: fillExtents, z: strokeWidth, w: strokeOffsetMin )
				half4 roundedness = UNITY_ACCESS_INSTANCED_PROP( Props, _Roundedness );
				half4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				half4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				// Evaluate shape SDF.
				float d = SDRoundedBox( i.posSS, data.xy, roundedness ) - data.w;

				// Compute fragment size in shape space. We presume uniform shape space, so we only have to measure one dimension.
				float fSize = fwidth( i.posSS.y );

				// Compute total extents for maintaining min size.
				float totalExtents = min( data.x, data.y ) + data.z; // data.xy: fillExtents, data.z: strokeWidth

				// Evaluate color.
				#ifdef _HAS_TEXTURE
					half4 texTint = UNITY_ACCESS_INSTANCED_PROP( Props, _TexTint );
					i.uv = ( ( i.uv * 2 - 1 ) * ( 1 + ( ( data.z + data.w ) / ( data.xy ) ) ) ) * 0.5 + 0.5;
					return EvaluateFillStrokeColor( d, fSize, totalExtents, data.z, fillCol, strokeCol, i.uv, texTint );
				#else
					return EvaluateFillStrokeColor( d, fSize, totalExtents, data.z, fillCol, strokeCol );
				#endif
			}

			ENDCG
		}
	}
}