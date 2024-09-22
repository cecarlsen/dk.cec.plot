/*
	Copyright © Carl Emil Carlsen 2020
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
		float _angle;

		void Update()
		{
			_angle = Mathf.Lerp( 10, 90, ( Mathf.Sin( Time.time ) + 1 ) / 2f );
			
			SetStrokeColor( 1 );
			SetStrokeWidth( 0.02f );

			DrawLine( 0, 0, 0, -2 );
			DrawRecursiveBranch( 2 );
		}


		void DrawRecursiveBranch( float h )
		{
			h *= 0.66f;

			if( h < 0.04f ) return;

			PushCanvas();
			RotateCanvas( _angle );
			DrawLine( 0, 0, 0, h );
			TranslateCanvas( 0, h );
			DrawRecursiveBranch( h );
			PopCanvas();

			PushCanvas();
			RotateCanvas( -_angle );
			DrawLine( 0, 0, 0, h );
			TranslateCanvas( 0, h );
			DrawRecursiveBranch( h );
			PopCanvas();
		}
	}
}