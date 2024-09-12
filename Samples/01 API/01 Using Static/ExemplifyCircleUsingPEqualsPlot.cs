/*
	Copyright © Carl Emil Carlsen 2020
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

		void Update()
		{
			P.SetNoStrokeColor();
			P.SetFillColor( color );

			P.PushCanvas();
			P.SetCanvas( transform );

			P.DrawCircle( 0, 0, 1 );

			P.PopCanvas();
		}
	}
}