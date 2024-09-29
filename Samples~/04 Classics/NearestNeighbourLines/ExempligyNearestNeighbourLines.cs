/*
	Copyright © Carl Emil Carlsen 2023-2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExempligyNearestNeighbourLines : MonoBehaviour
	{
		public bool prewarm = true;
		[Range(1,10)] public int iterationsPerUpdate = 5;
		[Range(0f,1f)] public float alpha = 0.01f;

		Vector2[] _pos, _vel;

		RenderTexture _rt;

		const int count = 64;
		const float strokeWidth = 1; // Px
		const float distMin = 300; // Px
		const float distMax = 500; // Px
		const int prewarmIterations = 8;


		void OnEnable()
		{
			_rt = new RenderTexture( 3840, 2160, 0, RenderTextureFormat.ARGB32 );
			ClearRenderTextureNow( _rt, Color.clear );

			_pos = new Vector2[ count ];
			_vel = new Vector2[ count ];
			for( int i = 0; i < count; i++ ) {
				_pos[ i ] = new Vector2( Random.value * _rt.width, Random.value * _rt.height );
				_vel[ i ] = Random.insideUnitCircle.normalized; // Px
			}

			if( prewarm ) for( int i = 0; i < prewarmIterations; i++ ) StepAndDrawLines();
		}


		void OnDisable()
		{
			if( _rt ) _rt.Release();
			_rt = null;
		}


		void LateUpdate()
		{
			for( int i = 0; i < iterationsPerUpdate; i++ ) StepAndDrawLines();
			DrawRenderTexture();
		}


		void StepAndDrawLines()
		{
			MoveAndWrapPositions();
			DrawToRenderTexture();
		}


		void MoveAndWrapPositions()
		{
			for( int i = 0; i < count; i++ )
			{
				_pos[ i ] += _vel[ i ];
				_pos[ i ].Set(
					Mathf.Repeat( _pos[ i ].x+distMax, _rt.width + distMax*2 )-distMax, 
					Mathf.Repeat( _pos[ i ].y+distMax, _rt.height + distMax*2 )-distMax
				);
			}
		}


		void DrawToRenderTexture()
		{
			PushCanvasAndStyle();
			BeginDrawNowToRenderTexture( _rt, Plot.Space.Pixels );

			SetBlend( Blend.TransparentAdditive );
			SetStrokeWidth( strokeWidth );
			float distMinSqr = distMin * distMin;
			float distMaxSqr = distMax * distMax;
			for( int ia = 0; ia < count; ia++ ) {
				for( int ib = 0; ib < ia; ib++ ) {
					float distSqr = ( _pos[ ib ] - _pos[ ia ] ).sqrMagnitude;
					if( distSqr < distMaxSqr ){
						SetStrokeColor( Color.white, Mathf.InverseLerp( distMaxSqr, distMinSqr, distSqr ) * 0.01f );
						DrawLineNow( _pos[ ia ], _pos[ ib ] );
					}
				}
			}

			EndDrawNowToRenderTexture();
			PopCanvasAndStyle();
		}


		void DrawRenderTexture()
		{
			PushCanvasAndStyle();
			SetCanvas( transform );

			SetBlend( Blend.TransparentAdditive );
			SetNoStroke();
			SetFillTextureTint( Color.white, alpha );
			SetFillColor( Color.clear );
			SetFillTexture( _rt );
			DrawRect( 0f, 0f, _rt.width / (float) _rt.height, 1f );

			PopCanvasAndStyle();
		}
	}
}