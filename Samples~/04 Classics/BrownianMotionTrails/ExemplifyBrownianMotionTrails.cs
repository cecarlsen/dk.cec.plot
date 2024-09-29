/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk

	Brownian motion
	https://en.wikipedia.org/wiki/Brownian_motion
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class BrownianMotionTrails : MonoBehaviour
	{
		[Range(0.1f,5f)] public float strokeWidth = 1f;
		[Range(0f,1f)] public float alpha = 0.5f;
		[Range(0.1f,5f)] public float step = 7;
		[Range(1,20)] public int iterationsPerUpdate = 3;
		public bool prewarm = true;

		Vector2[] _brushes;
		RenderTexture _rt;

		const int brushCount = 32;
		const int prewarmIterations = 4;
		

		void OnEnable()
		{
			_rt = new RenderTexture( 3840, 2160, 0, RenderTextureFormat.ARGB32 );
			ClearRenderTextureNow( _rt, Color.clear );
			_brushes = new Vector2[ brushCount ];
			for( int i = 0; i < brushCount; i++ ) _brushes[ i ] = new Vector2( _rt.width / 2f, _rt.height / 2f );
			if( prewarm ) for( int i = 0; i < prewarmIterations; i++ ) LateUpdate();
		}


		void OnDisable()
		{
			if( _rt ) _rt.Release();
			_rt = null;
		}


		void LateUpdate()
		{
			for( int i = 0; i < iterationsPerUpdate; i++ ) MoveAndDrawBrushesToRenderTexture();
			DrawTextureRenderInScene();
		}


		void MoveAndDrawBrushesToRenderTexture()
		{
			PushCanvasAndStyle();
			BeginDrawNowToRenderTexture( _rt, Plot.Space.Pixels );

			SetBlend( Blend.TransparentAdditive );
			SetStrokeWidth( strokeWidth );
			for( int i = 0; i < brushCount; i++ ){
				var p0 = _brushes[ i ];
				var p1 = p0 + Random.insideUnitCircle * _rt.height * step * Time.fixedDeltaTime;
				SetStrokeColor( Color.white, alpha );
				DrawLineNow( p0, p1 );
				_brushes[ i ] = p1;
			}

			EndDrawNowToRenderTexture();
			PopCanvasAndStyle();
		}


		void DrawTextureRenderInScene()
		{
			PushCanvasAndStyle();
			SetCanvas( transform );

			SetBlend( Blend.Transparent );
			SetNoStroke();
			SetFillTextureTint( Color.white );
			SetFillColor( Color.clear );
			SetFillTexture( _rt );
			DrawRect( 0f, 0f, _rt.width / (float) _rt.height, 1f );

			PopCanvasAndStyle();
		}
	}
}