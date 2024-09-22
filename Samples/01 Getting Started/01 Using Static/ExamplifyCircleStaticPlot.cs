/*
	Copyright © Carl Emil Carlsen 2020-2024
	http://cec.dk
*/

using UnityEngine;

namespace PlotExamples
{
	[ExecuteInEditMode] // Make sure that Update is called in Edit Mode.
	public class ExamplifyCircleStaticPlot : MonoBehaviour
	{
		public Color color = Color.red;

		void Update()
		{
			Plot.SetNoStrokeColor();					// Request no stroke for subsequently drawn shapes.
			Plot.SetFillColor( color );					// Request a fill color for subsequently drawn shapes.
			
			Plot.PushCanvas();							// Save the current canvas matrix on the canvas stack.
			Plot.SetCanvas( transform );				// We want to draw locally to this transform.
		
			Plot.DrawCircle( x: 0, y: 0, diameter: 1 );	// Draw a circle at canvas center.
		
			Plot.PopCanvas();							// Load previously saved canvas matrix from the canvas stack
														// We do this to isolate transformations made in this script.

		}
	}
}