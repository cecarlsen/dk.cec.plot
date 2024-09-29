/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using static Plot;	// Import all static methods from Plot into this script for immediate access.

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyCircleUsingStaticPlot : MonoBehaviour
	{
		public Color color = Color.red;

		void LateUpdate()
		{
			PushCanvasAndStyle();
			SetCanvas( transform );

			SetNoStroke();
			SetFillColor( color );
			DrawCircle( 0, 0, 1 );

			PopCanvasAndStyle();
		}
	}
}