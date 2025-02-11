/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

Shader "Hidden/Draw/Arc"
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

			#pragma enable_d3d11_debug_symbols // Switch this off in production!

			// Instancing Options.
			//		nolodfade - Prevent Unity from applying GPU Instancing to LOD fade values.
			//		nolightprobe - Prevent Unity from applying GPU Instancing to Light Probe values
			//		nolightmap - Prevent Unity from applying GPU Instancing to Lightmap ST
			#pragma instancing_options nolodfade nolightprobe nolightmap

			#pragma multi_compile_fog 				// Support fog.
			#pragma multi_compile_instancing		// Support instancing
			#pragma multi_compile_local __ _ANTIALIAS
			#pragma multi_compile_local __ _TEXTURE_OVERLAY _TEXTURE_MULTIPLY _TEXTURE_REPLACE

			#if defined( _TEXTURE_OVERLAY ) || defined( _TEXTURE_MULTIPLY ) || defined( _TEXTURE_REPLACE )
				#define _HAS_TEXTURE
			#endif

			#define HALF_PI 1.57079632679
			#define PI 3.14159265359
			#define TAU 6.28318530718
			#define CONER_COUNT 6.0 // Hexagon. Must match conerCount in ArcPRenderer.cs

			#include "UnityCG.cginc"
			#include "Base.cginc"


			struct ToVert
			{
				float3 t_outerFlag_flapFlag : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct ToFrag
			{
				float4 vertex : SV_POSITION;
				float2 posSS : TEXCOORD0;
				nointerpolation float2 c : TEXCOORD1;	// Sincos to angle extents.
				UNITY_FOG_COORDS( 2 ) 					// Support fog.
				UNITY_VERTEX_INPUT_INSTANCE_ID 			// Support instanced properties in fragment Shader.
				#ifdef _HAS_TEXTURE 
					float2 uv : TEXCOORD3;
				#endif
			};
			
			
			UNITY_INSTANCING_BUFFER_START( Props )
				UNITY_DEFINE_INSTANCED_PROP( half4, _VertData )			// ( x: mesh scale, y: inner vertex factor, z: frag angle extents, w: vert angle extents )
				UNITY_DEFINE_INSTANCED_PROP( half, _AngleOffset )
				UNITY_DEFINE_INSTANCED_PROP( half4, _FragData )			// ( x: ringRadius, y: ringExtents, z: horseshoeExtention, w: fillDistOffset )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeWidth )
				UNITY_DEFINE_INSTANCED_PROP( half4, _FillColor )
				UNITY_DEFINE_INSTANCED_PROP( half4, _StrokeColor )
				UNITY_DEFINE_INSTANCED_PROP( half, _StrokeFeather )
				#ifdef _HAS_TEXTURE
					UNITY_DEFINE_INSTANCED_PROP( half4, _TexUVRect )
					UNITY_DEFINE_INSTANCED_PROP( half, _StrokeAlignmentExtension )
					UNITY_DEFINE_INSTANCED_PROP( half4, _TexTint )
				#endif
			UNITY_INSTANCING_BUFFER_END( Props )


			// Compute the signed distance to a horseshoe shape.
			// by IQ From IQ: https://www.iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
			// p == position, c = float2(cos(a),sin(a)), r = mid radius, w.x == shoe extents, w.y == radial extents
			float SdHorseshoe( float2 p, float2 c, float r, float2 w )
			{
				p.x = abs( p.x );
				float l = length( p );
				p = mul( float2x2( -c.x, c.y, c.y, c.x ), p );
				p = float2( ( p.y > 0.0 ) ? p.x : l * sign( -c.x ), ( p.x > 0.0 ) ? p.y : l );
				p = float2( p.x, abs( p.y - r ) ) - w;
				return length( max( p, 0.0 ) ) + min( 0.0, max( p.x, p.y ) );
			}


			ToFrag Vert( ToVert v )
			{
				ToFrag o;
				float2 c;

				UNITY_SETUP_INSTANCE_ID( v );			// Support instancing
				UNITY_TRANSFER_INSTANCE_ID( v, o );		// Support instanced properties in fragment Shader.

				// Read properties.
				half4 data = UNITY_ACCESS_INSTANCED_PROP( Props, _VertData ); // ( x: mesh scale, y: inner vertex factor, z: frag angle extents, w: vert angle extents )
				half angleOffset = UNITY_ACCESS_INSTANCED_PROP( Props, _AngleOffset );

				// Get scales.
				float2 shapeScaleWithCanvasScale, modelScale;
				GetScales( data.xx, shapeScaleWithCanvasScale, modelScale );

				// Compute outwards direction.
				float2 outwards;
				float angleSpread = v.t_outerFlag_flapFlag.x * data.w;
				sincos( angleOffset + angleSpread, outwards.y, outwards.x );

				// Create vertex.
				float2 outwardsShape = outwards;
				float radiusWithCanvasScale = max( shapeScaleWithCanvasScale.x, shapeScaleWithCanvasScale.y );
				o.vertex = float4( outwards, 0, 1 );
				float outwardsFactor;
				if( v.t_outerFlag_flapFlag.y < 1 ){				// outerFlag == 0 indicates an inner vertex.
					outwardsFactor = 0.5 * data.y;				// Scale inner vertex.
					radiusWithCanvasScale *= data.y;
					outwardsShape *= -1;
				} else {
					outwardsFactor = 0.5 / cos( data.w / CONER_COUNT ); // Adapt outer vertex to contain circle. Presuming a hexagon. cos( ( 2 * data.z / 6.0 ) * 0.5f ). 
				}
				o.vertex.xy *= outwardsFactor;

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
				float2 expansion = outwards * GetMinSizePreservingExpansion( pixelSizeWS, shapeScaleWithCanvasScale );

				// Add antialiasing preserving expansion.
				#ifdef _ANTIALIAS
					float antialiasExpansion = pixelSizeWS * ANTIALIAS_EXTENTS;
					// If inner vertex, then constrain so that we don't shrink beyond the center.
					antialiasExpansion = antialiasExpansion * v.t_outerFlag_flapFlag.y + min( antialiasExpansion, data.y * 0.5 ) * -( v.t_outerFlag_flapFlag.y - 1 ); // Same as if( v.t_outerFlag_flapFlag.y < 1 )
					expansion += outwardsShape * antialiasExpansion;
					// If horse shoe flap, then extend flap too.
					float shoeExpandMax = ( TAU - data.z * 2 ) * radiusWithCanvasScale * 0.5;
					expansion += v.t_outerFlag_flapFlag.z * float2( -outwards.y, outwards.x ) * min( antialiasExpansion, shoeExpandMax );
				#endif

				// Transform vertex and shape position.
				TransformVertexAndComputeShapePosition( expansion, data.xx, shapeScaleWithCanvasScale, modelScale, o.vertex, o.posSS );

				// Compute and transfer sincos.
				sincos( data.z - HALF_PI, c.x, c.y ); // z: frag angle extents.
				o.c = c;

				// Support fog.
				UNITY_TRANSFER_FOG( o, o.vertex );

				return o;
			}


			half4 Frag( ToFrag i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i ); // Support instanced properties in fragment Shader.
				
				// Get properties.
				half4 data = UNITY_ACCESS_INSTANCED_PROP( Props, _FragData );			// ( x: ringRadius, y: ringExtents, z: horseshoeExtention, w: fillDistOffset )
				half angleOffset = UNITY_ACCESS_INSTANCED_PROP( Props, _AngleOffset );
				half strokeWidth = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeWidth );
				half4 fillCol = UNITY_ACCESS_INSTANCED_PROP( Props, _FillColor );
				half4 strokeCol = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeColor );
				half strokeFeather = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeFeather );
				
				// Compute fragment size in shape space. We presume uniform shape space, so we only have to measure one dimension.
				float fSize = fwidth( i.posSS.y );

				// Compute SDF.
				float2 c;
				sincos( angleOffset + PI, c.x, c.y );
				i.posSS = mul( float2x2( c.x, -c.y, c.y, c.x ), i.posSS );

				half d = SdHorseshoe( i.posSS, i.c, data.x, data.zy ) + data.w;

				// Compute total extents for maintaining min size.
				float totalExtents = data.x + data.y + strokeWidth - data.w;

				// Evaluate color.
				#ifdef _HAS_TEXTURE
					half strokeAlignmentExtension = UNITY_ACCESS_INSTANCED_PROP( Props, _StrokeAlignmentExtension );
					half4 texTint = UNITY_ACCESS_INSTANCED_PROP( Props, _TexTint );
					i.uv = ( ( i.uv * 2 - 1 ) * ( 1 + ( strokeAlignmentExtension / ( data.x + data.y - data.w ) ) ) ) * 0.5 + 0.5; // NOTE: could rewrite this to be transformed in the vert shader.
					half4 col = EvaluateFillStrokeColor( d, fSize, totalExtents, strokeWidth, fillCol, strokeCol, strokeFeather, i.uv, texTint );
				#else
					half4 col =  EvaluateFillStrokeColor( d, fSize, totalExtents, strokeWidth, fillCol, strokeCol, strokeFeather );
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