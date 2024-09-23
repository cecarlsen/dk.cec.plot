/*
	Copyright © Carl Emil Carlsen 2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyImage : MonoBehaviour
	{
		public Texture texture;

 
		void Update() 
		{
			PushCanvasAndStyle();
			SetCanvas( transform );

			SetFillTexture( texture );
			SetStrokeWidth( 0.1f );
			SetStrokeColor( Color.white, 0.9f );
			SetFillColor( Color.blue );
			DrawRect( 0, 0, 1, 1, roundness: 0.2f );

			PopCanvasAndStyle();
		}
	}
}