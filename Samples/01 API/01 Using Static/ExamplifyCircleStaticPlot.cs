/*
	Copyright © Carl Emil Carlsen 2020
	http://cec.dk
*/

using UnityEngine;

namespace PlotExamples
{
	[ExecuteInEditMode]						// Make sure that Update is called in Edit Mode.
	public class ExamplifyCircleStaticPlot : MonoBehaviour
	{
		public Color color = Color.red;

		void Update()
		{
			Plot.SetNoStroke();				// Request not stroke for subsequently drawn shapes.
			Plot.SetFillColor( color );		// Request a fill color for subsequently drawn shapes.
			
			Plot.PushCanvas();				// Save the current canvas matrix (used later).
			Plot.SetCanvas( transform );	// We want to draw locally to this transform.
		
			Plot.DrawCircle( 0, 0, 1 );		// Draw a circle at canvas center.
		
			Plot.PopCanvas();				// Load previously saved canvas matrix to isolate transformations
											// made in this script.
		}
	}
}