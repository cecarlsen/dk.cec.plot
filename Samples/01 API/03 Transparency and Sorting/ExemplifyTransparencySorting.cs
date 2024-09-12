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


		void Update()
		{
			PushCanvas();
			PushStyle();

			SetNoStrokeColor();
			SetBlend( blend );

			const int count = 10;
			Random.InitState( 0 );
			for( int i = 0; i < count; i++ ) {
				TranslateCanvas( 0, 0, 0.1f );
				Vector2 position = Random.insideUnitCircle * 2;
				SetFillColor( Color.HSVToRGB( Random.value * 0.2f, 1, 1 ), 0.9f );
				DrawCircle( position, 1 );
			}

			PopStyle();
			PopCanvas();

			// Animate camera to reveal typical sorting issue.
			if( Application.isPlaying ) {
				cam.transform.position = new Vector3( Mathf.Cos( Time.time ) * cameraCircleRadius, Mathf.Sin( Time.time ) * cameraCircleRadius, cam.transform.position.z );
				cam.transform.LookAt( Vector3.zero );
			}
		}
	}
}