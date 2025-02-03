/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk

	About instancing
	https://docs.unity3d.com/Manual/GPUInstancing.html
*/

Shader "Hidden/Draw/Ring"
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
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector" = "True" }
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
				float4 outwards_radiusMult_outerFlag : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float2 posSS : TEXCOORD0;			// Shape position
				UNITY_FOG_COORDS( 1 ) 				// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 		// Support instanced properties in fragment Shader.
				#ifdef _HAS_TEXTURE
					float2 uv : TEXCOORD2;
				#endif
			};

			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( half2, _VertData )		// ( x: mesh scale, y: inner vertex factor )
				UNITY_DEFINE_INSTANCED_PROP( half4, _FillColor )
				UNITY_DEFINE_INSTANCED_PROP( half4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half4, _FragData )
				#ifdef _HAS_TEXTURE
					UNITY_DEFINE_INSTANCED_PROP( half4, _TexUVRect )
					UNITY_DEFINE_INSTANCED_PROP( half4, _TexTint )
				#endif
			UNITY_INSTANCING_BUFFER_END( Props )
			

			half SDRing( half2 p, half2 r )
			{
				return abs( length( p ) - r.x ) - r.y;
			}
			
			
			ToFrag Vert( ToVert v )
			{
				ToFrag o;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				// Read properties.
				half2 data = UNITY_ACCESS_INSTANCED_PROP( Props, _VertData ); // ( x: mesh scale, y: inner vertex factor )

				// Get scales.
				float2 shapeScaleWithCanvasScale, modelScale;
				GetScales( data.xx, shapeScaleWithCanvasScale, modelScale );

				// Create vertex.
				float2 outwardsShape = v.outwards_radiusMult_outerFlag.xy;
				o.vertex = float4( v.outwards_radiusMult_outerFlag.xy * v.outwards_radiusMult_outerFlag.z, 0, 1 );
				if( v.outwards_radiusMult_outerFlag.w < 1 ){ // outerFlag == 0 indicates an inner vertex.
					o.vertex.xy *= data.y; // data.y is inner radius factor
					outwardsShape *= -1;
				}

				// Compute shape position.
				o.posSS = o.vertex.xy * data.x;

				// Compute uv.
				#ifdef _HAS_TEXTURE
					half4 texST = UNITY_ACCESS_INSTANCED_PROP( Props, _TexUVRect );
					o.uv = ( o.vertex.xy + 0.5 ) * texST.zw + texST.xy;
				#endif

				// Compute world space pixel size at transformed position as seen by camera.
				float pixelSizeWS = GetWorldSpacePixelSizeRelativeToCamera( o.vertex );

				// Compute min size preserving expansion.
				float2 expansion = v.outwards_radiusMult_outerFlag.xy * GetMinSizePreservingExpansion( pixelSizeWS, shapeScaleWithCanvasScale );

				// Add antialiasing preserving expansion.
				#ifdef _ANTIALIAS
					float antialiasExpansion = pixelSizeWS * ANTIALIAS_EXTENTS;
					// If inner vertex, then constrain so that we don't shrink beyond the center.
					/* If */ antialiasExpansion = antialiasExpansion * v.outwards_radiusMult_outerFlag.w + min( antialiasExpansion, data.y * 0.5 ) * -( v.outwards_radiusMult_outerFlag.y - 1 ) ;
					expansion += outwardsShape * antialiasExpansion;
				#endif

				// Transform vertex and shape position.
				TransformVertexAndComputeShapePosition( expansion, data.xx, shapeScaleWithCanvasScale, modelScale, o.vertex, o.posSS );

				// Support fog.
				UNITY_TRANSFER_FOG( o, o.vertex );

				return o;
			}
			

			fixed4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.
				
				// Get properties.
				half4 data = UNITY_ACCESS_INSTANCED_PROP( Props, _FragData ); // ( x: ringRadius, y: ringExtents, z: strokeWidth, w: strokeOffMin ).
				half4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				half4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );

				// Compute fragment size in shape space. We presume uniform shape space, so we only have to measure one dimension.
				// It's the absolute difference between 'i.posSS.y' at this fragment and 'i.posSS.y' at the neighboring fragments.
				// The actual values of 'i.posSS.y' are picked up from neightbor threads, which are executing in same group.
				float fSize = fwidth( i.posSS.y );

				/*
				// TODO: Fade out fill when it gets too thin. So far only working for Stroke == Inside =(
				float minFillExtents = fSize * 0.5;
				//float safeFillAdd = max( 0, minFillExtents - ( data.y - data.z ) );
				data.y += safeFillAdd;
				data.z -= safeFillAdd;
				fillCol = lerp( fillCol, strokeCol, saturate( safeFillAdd / minFillExtents ) );
				*/

				// Compute shape SDF.
				half d = SDRing( i.posSS, data.xy ) - data.w;

				// Compute total extents for maintaining min size.
				float totalExtents = data.x + data.y + data.z; // Ring radius + ring extents + stroke width.

				// Evaluate color.
				#ifdef _HAS_TEXTURE
					i.uv = ( ( i.uv * 2 - 1 ) * ( 1 + ( ( data.z + data.w ) / ( data.x + data.y ) ) ) ) * 0.5 + 0.5;
					half4 texTint = UNITY_ACCESS_INSTANCED_PROP( Props, _TexTint );
					half4 col = EvaluateFillStrokeColor( d, fSize, totalExtents, data.z, fillCol, strokeCol, i.uv, texTint );
				#else
					half4 col =  EvaluateFillStrokeColor( d, fSize, totalExtents, data.z, fillCol, strokeCol );
				#endif

				// Support fog.
				UNITY_APPLY_FOG( i.fogCoord, col );

				// Done.
				return col;
			}
			ENDCG
		}
	}
}