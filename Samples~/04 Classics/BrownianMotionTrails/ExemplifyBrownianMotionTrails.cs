/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class BrownianMotionTrails : MonoBehaviour
	{
		[Range(0.1f,2f)] public float strokeWidth = 1f;
		[Range(0f,1f)] public float alpha = 0.5f;
		public bool prewarm = true;

		Vector2[] _brushes;
		RenderTexture _rt;

		const int brushCount = 256;
		const int width = 3840;
		const int height = 2180;
		const int prewarmIterations = 256;
		

		void OnEnable()
		{
			_rt = new RenderTexture( width, height, 0, RenderTextureFormat.ARGB32, mipCount: 4 ){
				useMipMap = true // Render nicely when zooming out in the scene.
			};
			Reset();

			if( prewarm ) for( int i = 0; i < prewarmIterations; i++ ) Update();
		}


		void OnDisable()
		{
			if( _rt ) _rt.Release();
			_rt = null;
		}


		void Update()
		{
			if( Input.GetMouseButtonDown( 0 ) ) Reset();

			MoveAndDrawBrushesToRenderTexture();
			DrawTextureRenderInScene();
		}


		void MoveAndDrawBrushesToRenderTexture()
		{
			BeginDrawNowToRenderTexture( _rt, Plot.Space.Pixels );

			SetBlend( Blend.TransparentAdditive );
			SetStrokeWidth( strokeWidth );
			for( int i = 0; i < brushCount; i++ ){
				var p0 = _brushes[ i ];
				var p1 = p0 + Random.insideUnitCircle * 10;
				SetStrokeColor( Color.white, alpha * Random.value );
				DrawLineNow( p0, p1 );
				if( p1.x <= 0 || p1.x >= width || p1.y < 0 || p1.y >= width ){
					p1 = new Vector2( Random.value * width, Random.value * height );
				}
				_brushes[ i ] = p1;
			}

			EndDrawNowToRenderTexture();
		}


		void DrawTextureRenderInScene()
		{
			SetFillTexture( _rt );
			SetFillColor( Color.black );
			SetNoStroke();
			DrawRect( 0, 0, width / (float) height, 1 );
		}


		void Reset()
		{
			ClearRenderTextureNow( _rt, Color.black );
			if( _brushes == null ) _brushes = new Vector2[ brushCount ];
			for( int i = 0; i < brushCount; i++ ) _brushes[ i ] = new Vector2( width / 2f, height / 2f );
		}
	}
}