/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;
using P = Plot; // Create a shortcut to Plot named P.

namespace PlotExamples
{
	[ExecuteInEditMode]
	public class ExemplifyCircleUsingPEqualsPlot : MonoBehaviour
	{
		public Color color = Color.red;

		void LateUpdate()
		{
			P.PushCanvasAndStyle();
			P.SetCanvas( transform );

			P.SetNoStroke();
			P.SetFillColor( color );
			P.DrawCircle( 0, 0, 1 );

			P.PopCanvasAndStyle();
		}
	}
}