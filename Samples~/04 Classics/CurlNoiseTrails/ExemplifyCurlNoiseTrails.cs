/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyCurlNoiseTrails : MonoBehaviour
	{
		[Range(1f,10f)] public float frequency = 5f;
		public Vector2 sampleOffset = new Vector2( 46.23476f, 7.1974f );
		[Range(0.1f,2f)] public float speed = 0.5f;
		[Range(0.1f,4f)] public float strokeWidth = 1f;
		[Range(0f,1f)] public float alpha = 0.1f;
		public bool prewarm = true;

		Vector2[] _brushes;
		RenderTexture _rt;

		const int brushCount = 64;
		const int width = 3840;
		const int height = 2160;
		const int prewarmIterations = 8;
		

		void OnEnable()
		{
			_rt = new RenderTexture( width, height, 0, RenderTextureFormat.ARGB32, mipCount: 4 ){
				useMipMap = true // Render nicely when zooming out in the scene.
			};
			Reset();

			if( prewarm ) for( int i = 0; i < prewarmIterations; i++ ) LateUpdate();
		}


		void OnDisable()
		{
			if( _rt ) _rt.Release();
			_rt = null;
		}


		void LateUpdate()
		{
			if( _brushes == null || Input.GetMouseButtonDown( 0 ) ) Reset();

			PushCanvasAndStyle();

			MoveAndDrawBrushesToRenderTexture();
			DrawTextureRenderInScene();

			PopCanvasAndStyle();
		}


		void MoveAndDrawBrushesToRenderTexture()
		{
			BeginDrawNowToRenderTexture( _rt, Plot.Space.Pixels );

			SetBlend( Blend.Transparent );
			SetStrokeWidth( strokeWidth );
			SetStrokeColor( Color.white, alpha );
			for( int i = 0; i < brushCount; i++ ){
				var p0 = _brushes[ i ];
				var pSample = frequency * p0 / width + sampleOffset;
				var delta = CurlNoise( pSample ) * height * speed * Time.fixedDeltaTime;
				var p1 = p0 + delta;
				DrawLineNow( p0, p1 );
				if( p1.x <= 0 || p1.x >= width || p1.y < 0 || p1.y >= height ){
					p1 = new Vector2( Random.value * width, Random.value * height );
				}
				_brushes[ i ] = p1;
			}

			EndDrawNowToRenderTexture();
		}


		void DrawTextureRenderInScene()
		{
			SetCanvas( transform );
			SetBlend( Blend.TransparentAdditive );
			SetFillTexture( _rt );
			SetFillColor( Color.black );
			SetNoStroke();
			DrawRect( 0, 0, width / (float) height, 1 );
		}


		void Reset()
		{
			ClearRenderTextureNow( _rt, Color.clear );
			_brushes = new Vector2[ brushCount ];
			for( int i = 0; i < brushCount; i++ ) _brushes[ i ] = new Vector2( Random.value * width, Random.value * height );
		}


		static Vector2 CurlNoise( Vector2 pos )
		{
			const float e = 0.001f;
			return new Vector2(
				( Mathf.PerlinNoise( pos.x, pos.y + e ) - Mathf.PerlinNoise( pos.x, pos.y - e ) ) / ( 2f * e ), 
				- ( ( Mathf.PerlinNoise( pos.x + e, pos.y ) - Mathf.PerlinNoise( pos.x - e, pos.y ) ) / ( 2f * e ) )
			);
		}
	}
}