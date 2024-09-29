/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifySimultaneousContrastCompensation : MonoBehaviour
	{
		public int stepCount = 12;
		public Color color = Color.red;
		public bool backgroundCompensationEnabled = true;
		public bool startGraytoneAt5 = true;

		const float gradientWidth = 5;


		void LateUpdate()
		{
			SetNoStroke();
			SetPivot( Pivot.Center );

			float step = 1 / ( stepCount - 1f );
			float stepSize = gradientWidth / (float) stepCount;
			float xBegin = -gradientWidth * 0.5f + stepSize * 0.5f;
			float rectHeight = stepSize * 3;
			float circleDiameter = stepSize * 0.3f;
			float beginGraytone = startGraytoneAt5 ? 5 : 1;

			PushCanvas();

			// Draw backgrounds.
			SetAntiAliasing( false );				// We want seamless edge alignment, which can only be achived without anti-aliasing.
			for( int i = 0; i < stepCount; i++ ) {
				float t = i * step;

				SetFillColor( t );
				DrawRect( xBegin + i * stepSize, 0, stepSize, rectHeight );
			}

			// Draw circles.
			SetAntiAliasing( true );				// Switch anti-aliasing on again.
			TranslateCanvas( 0, 0, -0.01f );		// Translate towards camera to make sure the circles will be sorted on top of background.
			for( int i = 0; i < stepCount; i++ ) {
				float t = i * step;

				JChColor jchColor = new JChColor( color );
				if( backgroundCompensationEnabled ) jchColor.backgroundGraytone = Mathf.Lerp( beginGraytone, 100, t );
				else jchColor.backgroundGraytone = 20; // Default

				SetFillColor( jchColor );
				DrawCircle( xBegin + i * stepSize, 0, circleDiameter );
			}

			PopCanvas();
		}
	}
}