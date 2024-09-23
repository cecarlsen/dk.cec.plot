/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyNoiseTrails : MonoBehaviour
	{
		public int brushCount = 64;
		[Range(1f,10f)]public float frequency = 5f;
		[Range(0.1f,2f)] public float strokeWidth = 1f;
		[Range(0f,1f)] public float alpha = 0.1f;

		Vector2[] _brushes;
		RenderTexture _rt;

		const int resolution = 4096;


		void Update()
		{
			bool initRenderTexture = false;
			if( !_rt ){
				if( _rt ) _rt.Release();
				_rt = new RenderTexture( resolution, resolution, 0, RenderTextureFormat.ARGB32 );
				initRenderTexture = true;
			}
			
			if( _brushes == null || _brushes.Length != brushCount ) ResetBrushes();

			if( initRenderTexture) BeginDrawNowToRenderTexture( _rt, Plot.Space.Pixels, Color.black );
			else BeginDrawNowToRenderTexture( _rt, Plot.Space.Pixels );

			SetStrokeWidth( strokeWidth );
			SetStrokeColor( Color.white, alpha );
			for( int i = 0; i < brushCount; i++ ){
				var p0 = _brushes[ i ];
				var pSample = frequency * p0 / resolution;
				var delta = CurlNoise( pSample ) * 10;
				var p1 = p0 + delta;
				DrawLineNow( p0, p1 );
				if( p1.x <= 0 || p1.x >= resolution || p1.y < 0 || p1.y >= resolution ){
					p1 = new Vector2( Random.value * resolution, Random.value * resolution );
				}
				_brushes[ i ] = p1;
			}

			EndDrawNowToRenderTexture();
			
			SetFillTexture( _rt );
			SetFillColor( Color.white );
			SetNoStroke();
			DrawRect( 0, 0, 1, 1 );
		}


		void ResetBrushes()
		{
			if( _brushes == null || _brushes.Length != brushCount ) _brushes = new Vector2[ brushCount ];
			for( int i = 0; i < brushCount; i++ ) _brushes[ i ] = new Vector2( Random.Range( 0f, resolution ), Random.Range( 0f, resolution ) );
		}


		void OnDisable()
		{
			if( _rt ) _rt.Release();
			_rt = null;
		}


		void OnValidate()
		{
			brushCount = Mathf.Clamp( brushCount, 1, 512 );
		}


		static Vector2 CurlNoise( Vector2 pos )
		{
			const float e = 0.01f;
			return new Vector2(
				( Mathf.PerlinNoise( pos.x, pos.y + e ) - Mathf.PerlinNoise( pos.x, pos.y - e ) ) / ( 2 * e ), 
				- ( ( Mathf.PerlinNoise( pos.x + e, pos.y ) - Mathf.PerlinNoise( pos.x - e, pos.y ) ) / ( 2f * e ) )
			);
		}
	}
}