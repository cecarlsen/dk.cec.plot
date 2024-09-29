/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyTransparencySorting : MonoBehaviour
	{
		public Blend blend = Blend.Transparent;
		public Camera cam = null;

		const float cameraCircleRadius = 2;


		void LateUpdate()
		{
			PushCanvasAndStyle();

			SetNoStroke();
			SetBlend( blend );

			const int count = 10;
			Random.InitState( 0 );
			for( int i = 0; i < count; i++ ) {
				TranslateCanvas( 0, 0, 0.1f ); // You need to separate shapes along the z axis to avoid z-fighting.
				Vector2 position = Random.insideUnitCircle * 2;
				SetFillColor( Color.HSVToRGB( Random.value * 0.2f, 1, 1 ), 0.9f );
				DrawCircle( position, 1 );
			}

			PopCanvasAndStyle();

			// Animate camera to reveal typical sorting issue.
			if( Application.isPlaying ) {
				cam.transform.position = new Vector3( Mathf.Cos( Time.time ) * cameraCircleRadius, Mathf.Sin( Time.time ) * cameraCircleRadius, cam.transform.position.z );
				cam.transform.LookAt( Vector3.zero );
			}
		}
	}
}