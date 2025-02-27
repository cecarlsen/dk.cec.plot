﻿/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

#define ANTIALIAS_EXTENTS 0.6 //0.55 // PX
//#define ANTIALIAS_EXTENTS 30.0 // TEST
#if _ANTIALIAS
	#define MIN_EXTENTS 0.5 // PX
#else
	#define MIN_EXTENTS 0.57 // When there is no antialiasing, thin lines dissapear sometimes when they are only 1px wide. So we expand a bit.
#endif


#ifdef _HAS_TEXTURE
	sampler2D _Tex;
#endif


/// Returns the shape extents including canvas scaling, which is both contained in the model matrix.
/// Because both shape and canvas transforationas are included in unity_ObjectToWorld, Unity can compute frustum culling.
float2 GetModelMatrixScale2D()
{
	return float2(
		length( unity_ObjectToWorld._m00_m10_m20 ),
		length( unity_ObjectToWorld._m01_m11_m21 )
	);
}


/// OLD VERSION
/// Returns the world space pixel size at view position (as seen from camera).
float GetWorldSpacePixelSizeRelativeToCamera( float4 vertexMS )
{
	float3 posVS = UnityObjectToViewPos( vertexMS );	// Model + View transformation.

	// Compute pixel size in world coordinates.
	//		unity_OrthoParams.y == float height the orthographic camera view.
	//		unity_OrthoParams.w == orthographics camera flag (0.0 or 1.0).
	//		_ScreenParams.y == height of the target texture.
	// -posVS.z / _ScreenParams.y will give you the height of a pixel in world space for a perspective camera.
	return ( -posVS.z * 0.5 * ( 1 - unity_OrthoParams.w ) + unity_OrthoParams.y * unity_OrthoParams.w ) / _ScreenParams.y;
}



/// Maintains stroke extents and compute min size alpha fade to avoid lines looking shitty when very thin.
/// Called from fragment functions.
void MaintainSafeStrokeExtents( float fSizeShape, inout float strokeExtents, out float alphaMult )
{
	float minExtents = fSizeShape * MIN_EXTENTS;
	float extension = max( 0, minExtents - strokeExtents );
	strokeExtents += extension;
	alphaMult = 1 - extension / minExtents;
}


/// Returns the extension needed for preserving min size with optional extension for antialiasing.
float2 GetMinSizePreservingExpansion
(
	float pixelSizeWS, float2 shapeScaleWithCanvasScale
){
	float minSize = pixelSizeWS * MIN_EXTENTS;
	float2 expansion = max( 0, minSize.xx - shapeScaleWithCanvasScale ); // Expand to preseve minimum size.
	return expansion;
}


/// Transforms vertex and computes shape position.
/// Called from vertex functions.
void TransformVertexAndComputeShapePosition
(
	float2 expansion,
	float2 shapeScaleWithoutCanvasScale, float2 shapeScaleWithCanvasScale, float2 modelScale,
	inout float4 vertex, inout float2 posSS
){
	posSS += expansion * shapeScaleWithoutCanvasScale / shapeScaleWithCanvasScale;		// Extend the shape position relative to canvas scaling.
	vertex.xy += expansion / modelScale;												// Extend the vertex, while compensating for the scaling that is about to be applied.

	// Transform the vertex that has possibly been extended.
	vertex = UnityObjectToClipPos( vertex );
}


void GetScales( float2 shapeScaleWithoutCanvasScale, out float2 shapeScaleWithCanvasScale, out float2 modelScale )
{
	// Get the shape extents influenced by canvas scaling.
	modelScale = GetModelMatrixScale2D();

	// Compute shapeScaleWithCanvasScale depending on whether it's included in the model matrix.
	#ifdef SHAPE_EXTENTS_EXCLUDED_FROM_MODEL_MATRIX // For example, the case for Polyline.
		shapeScaleWithCanvasScale = shapeScaleWithoutCanvasScale * modelScale;
	#else
		shapeScaleWithCanvasScale = modelScale;
	#endif
}


// Compute color assuming that d == 0 is where fill ends and stroke begins.
float4 EvaluateFillStrokeColor
(
	float d, float fSize, float totalExtents, float strokeWidth, float4 fillCol, float4 strokeCol, float strokeFeather
	#ifdef _HAS_TEXTURE
	, float2 uv, float4 texTint
	#endif
){
	if( strokeWidth <= 0.0 ) strokeCol = float4( fillCol.rgb, 0.0 );

	// Maintain min size and compute min size alpha.
	float minExtents = fSize * MIN_EXTENTS;
	float safeAdd = max( 0, minExtents - totalExtents );
	float minSizeAlpha = 1 - safeAdd / minExtents;
	d -= safeAdd;

	// When stroke width drops below minSize, then we interpolate stroke color towards fill color.
	// This way we completely avoid small scale jitter for strokes. The downside is that tiny shapes will lean towards fill color.
	float strokeThiningFactor = strokeCol.a * saturate( minExtents - strokeWidth ) / minExtents;
	strokeCol = lerp( strokeCol, fillCol, strokeThiningFactor );
	//return float4( strokeFactor.xxx, 1 ); // DEBUG visualize stroke factor

	// Sample fill texture.
	#ifdef _HAS_TEXTURE
		//return float4( 1, 0, 0, 1 );
		float4 texCol = tex2D( _Tex, uv ) * texTint;
		#ifdef _TEXTURE_OVERLAY
			fillCol.rgb = fillCol.rgb * ( 1 - texCol.a ) + texCol.rgb * texCol.a;
			fillCol.a = saturate( fillCol.a + texCol.a );
		#elif _TEXTURE_MULTIPLY
			fillCol *= texCol;
		#else // Replace.
			fillCol = texCol;
		#endif
	#endif

	//float strokeFactor = saturate( ceil( strokeWidth ) );
	#ifdef _ANTIALIAS
		// Compute extents to use for fading.
		float gradientExtents = fSize * ANTIALIAS_EXTENTS;

		// Check for discard.
		if( d > strokeWidth + gradientExtents ) discard;

		// Interpolate between fill and stroke colors.
		float4 col = lerp( fillCol, strokeCol, smoothstep( -gradientExtents, gradientExtents, d ) );
		//float4 col = lerp( fillCol, strokeCol, smoothstep( -gradientExtents, gradientExtents, d ) * strokeFactor );

		// Apply smooth edge. If no stroke, then strokeCol will be transparent and therefore we have already applied smooth edge by now.
		if( strokeWidth > 0 ) col.a *= smoothstep( gradientExtents, -gradientExtents, d - strokeWidth ); // Smooth version
		//if( strokeWidth > 0 ) col.a *= 1-saturate( ( d - strokeWidth + gradientExtents ) / ( gradientExtents * 2 ) ); // Linear version
	#else
		// Check for discard.
		if( d > strokeWidth ) discard;

		// Switch between fill and stroke colors.
		float4 col = lerp( fillCol, strokeCol, saturate( ceil( d ) ) );
	#endif

	// Apply min alpha fade.
	col.a *= minSizeAlpha;

	// Stroke feather.
	if( strokeFeather > 0.0 ){
		//#ifdef _FEATHER_FILL
			//col.a = lerp( col.a, col.a * saturate( -d / ( totalExtents * feather ) ), saturate( ceil(-d) ) );
		//#ifdef _FEATHER_STROKE
			col.a *= saturate( ( 1.0 - ( max(d,0.0) / strokeWidth ) ) / strokeFeather );
		//#else // ALL
		//	col.a *= saturate( ( strokeWidth - d ) / ( ( totalExtents + strokeWidth ) * feather ) );
		//#endif
	}
	
	return col;
}


float4 EvaluateStrokeColor( float d, float fSizeWS, float4 strokeCol )
{
	#ifdef _ANTIALIAS
		// Compute the antialisation extents in space space.
		float gradientExtents = fSizeWS * ANTIALIAS_EXTENTS;

		// Check for discard.
		if( d > gradientExtents ) discard;

		// Apply smooth edge.
		strokeCol.a *= smoothstep( gradientExtents, -gradientExtents, d );
	#else
		// Check for discard.
		if( d > 0 ) discard;
	#endif

	return strokeCol;
}