/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk

	Inspired by our beloved Processing guru Daniel Shiffman
	https://processing.org/examples/tree.html
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyRecursiveTree : MonoBehaviour
	{
		float _t;


		void OnEnable() => _t = 0;


		void LateUpdate()
		{
			_t += Time.deltaTime;
			float angle = 240 + Mathf.Lerp( 10, 90, ( Mathf.Sin( _t ) + 1 ) / 2f );
			
			PushCanvasAndStyle();
			SetCanvas( transform );

			SetStrokeColor( 1 );
			SetStrokeWidth( 0.02f );

			DrawLine( 0, 0, 0, -2 );
			DrawRecursiveBranch( 2, angle );

			PopCanvasAndStyle();
		}


		void DrawRecursiveBranch( float h, float a )
		{
			h *= 0.66f;

			if( h < 0.04f ) return;

			PushCanvas();
			RotateCanvas( a );
			DrawLine( 0, 0, 0, h );
			TranslateCanvas( 0, h );
			DrawRecursiveBranch( h, a );
			PopCanvas();

			PushCanvas();
			RotateCanvas( -a );
			DrawLine( 0, 0, 0, h );
			TranslateCanvas( 0, h );
			DrawRecursiveBranch( h, a );
			PopCanvas();
		}
	}
}